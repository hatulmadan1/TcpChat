using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace TcpChat
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(Dns.GetHostEntry(Dns.GetHostName())
                .AddressList
                .First(address => address.AddressFamily == AddressFamily.InterNetwork)
                .ToString());

            var ip = Dns.GetHostEntry(Dns.GetHostName())
                .AddressList
                .First(address => address.AddressFamily == AddressFamily.InterNetwork);

            Console.WriteLine(SocketPermission.AllPorts);

            SocketPermission permisSocket = new SocketPermission(
                NetworkAccess.Connect, TransportType.Tcp, "localhost",
                8888);

            string externalip = new WebClient().DownloadString("http://icanhazip.com");
            Console.WriteLine(externalip);
        }
    }
}
