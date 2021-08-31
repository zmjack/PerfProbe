using NStandard;
using System.Text.RegularExpressions;
using System.Threading;
using Xunit;

namespace PerfProbe.Test
{
    public class UnitTest1
    {
        private static readonly object mutex = new object();

        [Fact]
        public void ConsoleTest()
        {
            lock (mutex)
            {
                Perf.ClearHandlers();
                Perf.UseVerboseConsole();
                Perf.UseUdpClient("127.0.0.1", 26778);

                using (ConsoleAgent.Begin())
                {
                    Perf.Set();
                    Thread.Sleep(1000);
                    Perf.Set("P2");
                    Thread.Sleep(2000);
                    Perf.End();

                    var output = ConsoleAgent.ReadAllText();

                    Assert.True(output.IsMatch(new Regex(@"PerfProbe at  \d{4}/\d{2}/\d{2} \d{2}:\d{2}:\d{2}  \(Thread: \d+\)
  File    : .+?PerfProbe.Test\\UnitTest1.cs
  Title   : \(null\)
  Lines   : \d+~\d+
  Caller  : ConsoleTest
  Elapsed : \d{2}:\d{2}:\d{2}\.\d{7}
  Under   : .*?

PerfProbe at  \d{4}/\d{2}/\d{2} \d{2}:\d{2}:\d{2}  \(Thread: \d+\)
  File    : .+?PerfProbe.Test\\UnitTest1.cs
  Title   : P2
  Lines   : \d+~\d+
  Caller  : ConsoleTest
  Elapsed : \d{2}:\d{2}:\d{2}.\d{7}
  Under   : .*?

")));
                }
            }
        }

        [Fact]
        public void ConsoleTest2()
        {
            lock (mutex)
            {
                Perf.ClearHandlers();
                Perf.UseConsole();
                Perf.UseUdpClient("127.0.0.1", 26778);

                using (ConsoleAgent.Begin())
                {
                    Perf.Set();
                    Thread.Sleep(1000);
                    Perf.Set("P2");
                    Thread.Sleep(2000);
                    Perf.End();

                    var output = ConsoleAgent.ReadAllText();

                    Assert.True(output.IsMatch(new Regex(@"\d{4}/\d{2}/\d{2} \d{2}:\d{2}:\d{2} #\d+ UnitTest1.cs \d+~\d+ \d{2}:\d{2}:\d{2}\.\d{7}
\d{4}/\d{2}/\d{2} \d{2}:\d{2}:\d{2} #\d+ UnitTest1.cs P2 \d{2}:\d{2}:\d{2}.\d{7}
")));
                }
            }
        }

    }
}