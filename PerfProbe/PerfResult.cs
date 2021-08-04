using System;
using System.IO;

namespace PerfProbe
{
    public struct PerfResult
    {
        public PerfParameters Parameters { get; set; }

        public string GetVerboseContent()
        {
            return
                $"PerfProbe at  {Parameters.TimeAt:yyyy/MM/dd HH:mm:ss}  (Thread: {Parameters.ManagedThreadId}){Environment.NewLine}" +
                $"  File    : {Parameters.FilePath}{Environment.NewLine}" +
                $"  Lines   : [{Parameters.StartLineNumber},{Parameters.StopLineNumber}){Environment.NewLine}" +
                $"  Caller  : {Parameters.MemberName}{Environment.NewLine}" +
                $"  Elapsed : {Parameters.Elapsed}{Environment.NewLine}" +
                $"  Carry   : {Parameters.CarryObject?.ToString() ?? "(null)"}{Environment.NewLine}" +
                $"  Under   : {Environment.OSVersion} {(Environment.Is64BitOperatingSystem ? "x64" : "x86")}{Environment.NewLine}";
        }

        public string GetContent()
        {
            return $"{Parameters.TimeAt:yyyy/MM/dd HH:mm:ss} #{Parameters.ManagedThreadId} {Path.GetFileName(Parameters.FilePath)} [{Parameters.StartLineNumber},{Parameters.StopLineNumber}) {Parameters.Elapsed}";
        }
    }
}
