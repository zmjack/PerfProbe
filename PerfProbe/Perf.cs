using NStandard;
using NStandard.Locks;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading;

namespace PerfProbe
{
    internal class PerfLockType { }

    public static class Perf
    {
        public static event HandleDelegate OnHandle;

        [ThreadStatic] private static Stopwatch Stopwatch;
        [ThreadStatic] private static string Title;
        [ThreadStatic] private static string CallerMemberName;
        [ThreadStatic] private static string CallerFilePath;
        [ThreadStatic] private static int CallerLineNumber;

        public delegate void HandleDelegate(PerfResult result);

        public static void ClearHandlers() => OnHandle = null;

        private static HandleDelegate BuildConsoleHandle(bool verbose)
        {
            void Handle(PerfResult result)
            {
                var content = verbose ? result.GetVerboseContent() : result.GetContent();
                Console.WriteLine(content);
            }
            return Handle;
        }

        private static readonly TypeLockParser FileHandlerLockParser = new(nameof(Perf));
        private static HandleDelegate BuildFileHandler(bool verbose, string file)
        {
            void Handle(PerfResult result)
            {
                var content = verbose ? result.GetVerboseContent() : result.GetContent();
                using (FileHandlerLockParser.Parse<PerfLockType>().Begin())
                {
                    using var stream = new FileStream(file, FileMode.Append, FileAccess.Write);
                    using var writer = new StreamWriter(stream);
                    writer.WriteLine(content);
                }
            }
            return Handle;
        }

        private static HandleDelegate BuildUdpHandler(bool verbose, IPEndPoint remote)
        {
            var udpClient = new UdpClient(new IPEndPoint(IPAddress.Any, 0));
            void Handle(PerfResult result)
            {
                var content = verbose ? result.GetVerboseContent() : result.GetContent();
                var bytes = content.Bytes();
                udpClient.Send(bytes, bytes.Length, remote);
            }
            return Handle;
        }

        public static void UseConsole() => OnHandle += BuildConsoleHandle(false);
        public static void UseFile(string file) => OnHandle += BuildFileHandler(false, file);
        public static void UseUdpClient(IPEndPoint remote) => OnHandle += BuildUdpHandler(false, remote);
        public static void UseUdpClient(string ipString, int port) => OnHandle += BuildUdpHandler(false, new IPEndPoint(IPAddress.Parse(ipString), port));

        public static void UseVerboseConsole() => OnHandle += BuildConsoleHandle(true);
        public static void UseVerboseFile(string file) => OnHandle += BuildFileHandler(true, file);
        public static void UseVerboseUdpClient(IPEndPoint remote) => OnHandle += BuildUdpHandler(true, remote);
        public static void UseVerboseUdpClient(string ipString, int port) => OnHandle += BuildUdpHandler(true, new IPEndPoint(IPAddress.Parse(ipString), port));

        private static void Reset(string title, string callerFilePath, int callerLineNumber, string callerMemberName)
        {
            Stopwatch = new Stopwatch();
            Title = title;
            CallerFilePath = callerFilePath;
            CallerLineNumber = callerLineNumber;
            CallerMemberName = callerMemberName;
            Stopwatch.Start();
        }

        public static void Set(string title = null,
            [CallerFilePath] string callerFilePath = "",
            [CallerLineNumber] int callerLineNumber = 0,
            [CallerMemberName] string callerMemberName = "")
        {
            void reset()
            {
                Reset(title, callerFilePath, callerLineNumber, callerMemberName);
            }

            if (Stopwatch is null) reset();
            else
            {
                if (CallerFilePath == callerFilePath && CallerLineNumber < callerLineNumber)
                {
                    Stopwatch.Stop();
                    Handle(callerLineNumber);
                    reset();
                    Stopwatch.Reset();
                    Stopwatch.Start();
                }
                else reset();
            }
        }

        public static void End([CallerLineNumber] int callerLineNumber = 0)
        {
            if (Stopwatch is null) throw new InvalidOperationException("PerfProbe.Set must be called before calling the method ");
            else
            {
                Stopwatch.Stop();
                Handle(callerLineNumber);
                Stopwatch = null;
            }
        }

        private static void Handle(int callerLineNumber)
        {
            var parameters = new PerfParameters
            {
                TimeAt = DateTime.Now,
                ManagedThreadId = Thread.CurrentThread.ManagedThreadId,
                Title = Title,
                FilePath = CallerFilePath,
                StartLineNumber = CallerLineNumber,
                StopLineNumber = callerLineNumber,
                MemberName = CallerMemberName,
                Elapsed = Stopwatch.Elapsed,
            };
            OnHandle?.Invoke(new PerfResult { Parameters = parameters });
        }

    }
}
