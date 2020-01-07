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
        public static HandlerDelegate Handler { get; private set; } = DefaultHandler;

        [ThreadStatic] private static Stopwatch Stopwatch;
        [ThreadStatic] public static object CarryObject;
        [ThreadStatic] public static string CallerMemberName;
        [ThreadStatic] public static string CallerFilePath;
        [ThreadStatic] public static int CallerLineNumber;
        public static TypeLockParser LockParser = new TypeLockParser(nameof(Perf));

        public delegate void HandlerDelegate(object carryObj, string filePath, string lines, string memberName, long elapsedMilliseconds);
        public static void Register(HandlerDelegate handler)
        {
            Handler = handler;
        }

        public static void DefaultHandler(object carryObj, string filePath, string lines, string memberName, long elapsedMilliseconds)
        {
            Console.WriteLine(GetDefaultOutputString(carryObj, filePath, lines, memberName, elapsedMilliseconds));
        }

        public static HandlerDelegate BuildFileHandler(string file)
        {
            void ret(object carryObj, string filePath, string lines, string memberName, long elapsedMilliseconds)
            {
                using (LockParser.Parse<PerfLockType>().Begin())
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

        public static void UseConsoleOutput() => Handler = DefaultHandler;
        public static void UseFileStorage(string file) => Handler = BuildFileHandler(file);

        public static string GetDefaultOutputString(object carryObj, string filePath, string lines, string memberName, long elapsedMilliseconds)
        {
            var now = DateTime.Now;
            var threadId = Thread.CurrentThread.ManagedThreadId;

            return
                $"PerfProbe\tat  {now}\t(Thread:{threadId}){Environment.NewLine}" +
                $"  File:\t{filePath}\tLines:{lines}{Environment.NewLine}" +
                $"  Caller:\t{memberName}\tElapsedTime:\t{TimeSpan.FromMilliseconds(elapsedMilliseconds)} ms{Environment.NewLine}" +
                $"  CarryObject:\t{carryObj?.ToString()}{Environment.NewLine}";
        }

        public static void Reset(object carryObj, string callerFilePath, int callerLineNumber, string callerMemberName)
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
                    Handler(CarryObject, CallerFilePath, $"[{CallerLineNumber},{callerLineNumber})", CallerMemberName, Stopwatch.ElapsedMilliseconds);
                    reset();
                    Stopwatch.Reset();
                    Stopwatch.Start();
                }
                else reset();
            }
        }

        public static void End([CallerLineNumber] int callerLineNumber = 0)
        {
            if (Stopwatch is null)
                throw new InvalidOperationException("PerfProbe.Set must be called before calling the method ");
            else
            {
                Stopwatch.Stop();
                Handler(CarryObject, CallerFilePath, $"[{CallerLineNumber},{callerLineNumber})", CallerMemberName, Stopwatch.ElapsedMilliseconds);
                Stopwatch = null;
            }
        }

    }
}
