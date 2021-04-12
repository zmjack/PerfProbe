# PerfProbe

**PerfProbe** is a performance testing utility.



## How to use

### UseConsole

Just use **Perf.Set()** to mark the rows you want to test.

For example, write the following code and run it.

```c#
using PerfProbe;
using System.Threading;

namespace PerfProbeApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Perf.UseConsole();

            Perf.Set();
            Thread.Sleep(1000);
            Perf.Set();
            Thread.Sleep(2000);
            Perf.End();
        }
    }
}
```

Console output:

> PerfProbe       at  2020/1/7 8:05:55    (Thread:1)  
> File: Program.cs      Lines:[12,14)  
> Caller:       Main    ElapsedTime:    00:00:01  
> CarryObject:
>
> PerfProbe       at  2020/1/7 8:05:57    (Thread:1)  
> File: Program.cs      Lines:[14,16)  
> Caller:       Main    ElapsedTime:    00:00:02.0010000  
> CarryObject:



### UseFile

If you want save the output to a file, you only need call **Perf.UseFile()** before using **Perf.Set()**.

For example, write the following code and run it.

```c#
using PerfProbe;
using System.Threading;

namespace PerfProbeApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Perf.UseFile("PerfProbeOutput.txt");

            Perf.Set();
            Thread.Sleep(1000);
            Perf.Set();
            Thread.Sleep(2000);
            Perf.End();
        }
    }
}
```

