using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Net.Sockets;
using System.Text;
using ChatServer;

namespace ChatClient
{
    class Program //client2
    {
        static string userName;
        private static string host;
        private const int port = 8889;
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

            Console.WriteLine($"Your IP is {ip}");

            SocketPermission permisSocket = new SocketPermission(
                NetworkAccess.Connect, TransportType.Tcp, "localhost",
                port);

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

                // запускаем новый поток для получения данных
                Thread receiveThread = new Thread(new ThreadStart(ReceiveMessage));
                receiveThread.Start(); //старт потока
                Console.WriteLine("Добро пожаловать, {0}", userName);
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
        // отправка сообщений
        static void SendMessage()
        {
            Console.WriteLine("Введите сообщение: ");

            while (true)
            {
                string message = Console.ReadLine();
                byte[] data = Encoding.Unicode.GetBytes(message);
                stream.Write(data, 0, data.Length);
            }
        }
        // получение сообщений
        static void ReceiveMessage()
        {
            while (true)
            {
                try
                {
                    byte[] data = new byte[64]; // буфер для получаемых данных
                    StringBuilder builder = new StringBuilder();
                    int bytes = 0;
                    do
                    {
                        bytes = stream.Read(data, 0, data.Length);
                        builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
                    }
                    while (stream.DataAvailable);

                    string message = builder.ToString();
                    Console.WriteLine(message);//вывод сообщения
                }
                catch
                {
                    Console.WriteLine("Подключение прервано!"); //соединение было прервано
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