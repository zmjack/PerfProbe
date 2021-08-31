using System;

namespace PerfProbe
{
    public struct PerfParameters
    {
        public DateTime TimeAt { get; set; }
        public int ManagedThreadId { get; set; }
        public string Title { get; set; }
        public string FilePath { get; set; }
        public int StartLineNumber { get; set; }
        public int StopLineNumber { get; set; }
        public string MemberName { get; set; }
        public TimeSpan Elapsed { get; set; }
    }
}
