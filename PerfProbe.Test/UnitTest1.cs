using NStandard;
using System.IO;
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
            Perf.UseConsoleOutput();

            using (var agent = new ConsoleAgent())
            {
                Perf.Set();
                Thread.Sleep(1000);
                Perf.Set();
                Thread.Sleep(2000);
                Perf.End();

                var output = agent.ReadAllText();

                Assert.True(output.IsMatch(new Regex(@"PerfProbe\tat  .+?\t\(Thread:\d+\)
  File:	.+?PerfProbe.Test\\UnitTest1.cs\tLines:\[18,20\)
  Caller:\tConsoleTest\tElapsedTime:\t.+? ms
  CarryObject:\t

PerfProbe\tat  .+?\t\(Thread:\d+\)
  File:	.+?PerfProbe.Test\\UnitTest1.cs\tLines:\[20,22\)
  Caller:\tConsoleTest\tElapsedTime:\t.+? ms
  CarryObject:\t

")));
            }
        }

        [Fact]
        public void FileTest()
        {
            var file = "PerfProbeOutput.txt";
            File.WriteAllText(file, "");

            Perf.UseFileStorage(file);

            Perf.Set();
            Thread.Sleep(1000);
            Perf.Set();
            Thread.Sleep(2000);
            Perf.End();

            var content = File.ReadAllText(file);

            Assert.True(content.IsMatch(new Regex(@"PerfProbe\tat  .+?\t\(Thread:\d+\)
  File:	.+?PerfProbe.Test\\UnitTest1.cs\tLines:\[48,50\)
  Caller:\tFileTest\tElapsedTime:\t.+? ms
  CarryObject:\t

PerfProbe\tat  .+?\t\(Thread:\d+\)
  File:	.+?PerfProbe.Test\\UnitTest1.cs\tLines:\[50,52\)
  Caller:\tFileTest\tElapsedTime:\t.+? ms
  CarryObject:\t

")));
        }

    }
}