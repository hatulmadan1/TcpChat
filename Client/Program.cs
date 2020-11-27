using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Net.Sockets;
using System.Text;
using ChatServer;

namespace ChatClient
{
    class Program
    {
        static string userName;
        private static string host;
        private const int port = 8888;
        static TcpClient client;
        static NetworkStream stream;
        static ServerObject server;
        static Thread listenThread;

        static void Main(string[] args)
        {
            try
            {
                server = new ServerObject();
                listenThread = new Thread(new ParameterizedThreadStart(server.Listen));
                listenThread.Start(port);
            }
            catch (Exception ex)
            {
                server.Disconnect();
                Console.WriteLine(ex.Message);
            }
            var ip = Dns.GetHostEntry(Dns.GetHostName())
                .AddressList
                .First(address => address.AddressFamily == AddressFamily.InterNetwork);
            //string ip = new WebClient().DownloadString("http://icanhazip.com");


            SocketPermission permisSocket = new SocketPermission(
                NetworkAccess.Connect, TransportType.Tcp, "localhost",
                port);

            permisSocket.Assert();

            Console.WriteLine($"Your IP is {ip}");

            Console.WriteLine("Enter ipv4 to connect");
            host = Console.ReadLine();
            Console.Write("Enter your name ");
            userName = Console.ReadLine();
            Console.WriteLine("Enter the connection port. Default is 8888");
            int connectionPort = int.Parse(Console.ReadLine());
            client = new TcpClient();
            try
            {
                client.Connect(host, connectionPort);
                stream = client.GetStream();

                string message = userName;
                byte[] data = Encoding.Unicode.GetBytes(message);
                stream.Write(data, 0, data.Length);
                
                Thread receiveThread = new Thread(new ThreadStart(ReceiveMessage));
                receiveThread.Start(); 
                Console.WriteLine("Welcome, {0}", userName);
                SendMessage();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                Disconnect();
            }
        }
        
        static void SendMessage()
        {
            Console.WriteLine("Enter a message: ");

            while (true)
            {
                string message = Console.ReadLine();
                byte[] data = Encoding.Unicode.GetBytes(message);
                stream.Write(data, 0, data.Length);
            }
        }
        
        static void ReceiveMessage()
        {
            while (true)
            {
                try
                {
                    byte[] data = new byte[64];
                    StringBuilder builder = new StringBuilder();
                    int bytes = 0;
                    do
                    {
                        bytes = stream.Read(data, 0, data.Length);
                        builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
                    }
                    while (stream.DataAvailable);

                    string message = builder.ToString();
                    Console.WriteLine(message);
                }
                catch
                {
                    Console.WriteLine("Connection broken!");
                    Console.ReadLine();
                    Disconnect();
                }
            }
        }

        static void Disconnect()
        {
            stream?.Close();
            client?.Close();
            Environment.Exit(0);
        }
    }
}