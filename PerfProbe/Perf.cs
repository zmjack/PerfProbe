using NStandard.Locks;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;

namespace PerfProbe
{
    public class PerfLockType { }

    public static class Perf
    {
        public static event HandlerDelegate OnHandle;

        [ThreadStatic] private static Stopwatch Stopwatch;
        [ThreadStatic] private static object CarryObject;
        [ThreadStatic] private static string CallerMemberName;
        [ThreadStatic] private static string CallerFilePath;
        [ThreadStatic] private static int CallerLineNumber;

        public delegate void HandlerDelegate(object carryObj, string filePath, string lines, string memberName, long elapsedMilliseconds);

        private static void ConsoleHandle(object carryObj, string filePath, string lines, string memberName, long elapsedMilliseconds)
        {
            Console.WriteLine(GetDefaultOutputString(carryObj, filePath, lines, memberName, elapsedMilliseconds));
        }

        private static readonly TypeLockParser FileHandlerLockParser = new TypeLockParser(nameof(Perf));
        private static HandlerDelegate BuildFileHandler(string file)
        {
            void ret(object carryObj, string filePath, string lines, string memberName, long elapsedMilliseconds)
            {
                using (FileHandlerLockParser.Parse<PerfLockType>().Begin())
                {
                    using (var stream = new FileStream(file, FileMode.Append, FileAccess.Write))
                    using (var writer = new StreamWriter(stream))
                    {
                        writer.WriteLine(GetDefaultOutputString(carryObj, filePath, lines, memberName, elapsedMilliseconds));
                    }
                }
            }

            return ret;
        }

        public static void UseConsole() => OnHandle += ConsoleHandle;
        public static void UseFile(string file) => OnHandle += BuildFileHandler(file);

        private static string GetDefaultOutputString(object carryObj, string filePath, string lines, string memberName, long elapsedMilliseconds)
        {
            var now = DateTime.Now;
            var threadId = Thread.CurrentThread.ManagedThreadId;

            return
                $"PerfProbe\tat  {now}\t(Thread: {threadId}){Environment.NewLine}" +
                $"  File:\t{filePath}\tLines:{lines}{Environment.NewLine}" +
                $"  Caller:\t{memberName}\tElapsed Time:\t{TimeSpan.FromMilliseconds(elapsedMilliseconds)}{Environment.NewLine}" +
                $"  Carry Object:\t{carryObj?.ToString()}{Environment.NewLine}" +
                $"  Run Under:\t{Environment.OSVersion} {(Environment.Is64BitOperatingSystem ? "x64" : "x86")}{Environment.NewLine}";
        }

        private static void Reset(object carryObj, string callerFilePath, int callerLineNumber, string callerMemberName)
        {
            Stopwatch = new Stopwatch();
            CarryObject = carryObj;
            CallerFilePath = callerFilePath;
            CallerLineNumber = callerLineNumber;
            CallerMemberName = callerMemberName;
            Stopwatch.Start();
        }

        public static void Set(object carryObj = null,
            [CallerFilePath] string callerFilePath = "",
            [CallerLineNumber] int callerLineNumber = 0,
            [CallerMemberName] string callerMemberName = "")
        {
            void reset()
            {
                Reset(carryObj, callerFilePath, callerLineNumber, callerMemberName);
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
            OnHandle?.Invoke(CarryObject, CallerFilePath, $"[{CallerLineNumber},{callerLineNumber})", CallerMemberName, Stopwatch.ElapsedMilliseconds);
        }

    }
}
