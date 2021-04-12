using NStandard;
using System.Text.RegularExpressions;
using System.Threading;
using Xunit;

namespace PerfProbe.Test
{
    public class UnitTest1
    {
        [Fact]
        public void ConsoleTest()
        {
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

                Assert.True(output.IsMatch(new Regex(@"PerfProbe at  \d{4}/\d{2}/\d{2} \d{2}:\d{2}:\d{2}  \(Thread: \d+\)
  File    : .+?PerfProbe.Test\\UnitTest1.cs
  Lines   : \[19,21\)
  Caller  : ConsoleTest
  Elapsed : .+?
  Carry   : \(null\)
  Under   : .*?

PerfProbe at  \d{4}/\d{2}/\d{2} \d{2}:\d{2}:\d{2}  \(Thread: \d+\)
  File    : .+?PerfProbe.Test\\UnitTest1.cs
  Lines   : \[21,23\)
  Caller  : ConsoleTest
  Elapsed : .+?
  Carry   : P2
  Under   : .*?

")));
            }
        }

    }
}