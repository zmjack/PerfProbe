using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace PerfProbe.Server
{
    class Program
    {
        private static readonly UdpClient UdpClient = new(26778);
        private static IPEndPoint Remote = new(IPAddress.Any, 0);

        static void Main(string[] args)
        {
            var semaphore = new Semaphore(0, 1);
            UdpClient.BeginReceive(new AsyncCallback(ReceiveCallback), null);
            semaphore.WaitOne();
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
