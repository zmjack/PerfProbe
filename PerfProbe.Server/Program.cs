using Ink;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace PerfProbe.Server
{
    class Program
    {
        private static UdpClient UdpClient;
        private static IPEndPoint Remote = new(IPAddress.Any, 0);

        static void Main(string[] args)
        {
            Echo.Ask("Input PerfProbe port (Default: 26778):", out int port, 26778);

            try
            {
                UdpClient = new UdpClient(port);
                Echo.Line($"PerfProbe port: {port,-5}")
                    .Line($"  - Press [Enter] to clear.")
                    .Line($"  - Press [Esc] to exit.");
                Console.WriteLine();
                UdpClient.BeginReceive(new AsyncCallback(ReceiveCallback), null);

                while (true)
                {
                    var key = Console.ReadKey(true).Key;
                    if (key == ConsoleKey.Escape)
                    {
                        Echo.AskYN("Exit?", out var exit);
                        if (exit) return;
                    }
                    else if (key == ConsoleKey.Enter) Console.Clear();
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"PerfProbe error: {ex.Message}");
                Echo.PressContinue();
            }
        }

        public static void ReceiveCallback(IAsyncResult ar)
        {
            var bytes = UdpClient.EndReceive(ar, ref Remote);
            var str = Encoding.UTF8.GetString(bytes);
            Console.WriteLine(str);
            UdpClient.BeginReceive(new AsyncCallback(ReceiveCallback), null);
        }

    }
}
