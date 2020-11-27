using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;

namespace ChatServer
{
    public class ServerObject
    {
        static TcpListener tcpListener;
        List<ClientObject> clients = new List<ClientObject>();

        protected internal void AddConnection(ClientObject clientObject)
        {
            clients.Add(clientObject);
        }
        protected internal void RemoveConnection(string id)
        {
            ClientObject client = clients.FirstOrDefault(c => c.Id == id);
            if (client != null)
                clients.Remove(client);
        }
        public void Listen(object port)
        {
            try
            {
                tcpListener = new TcpListener(IPAddress.Any, (int)port);
                tcpListener.Start();
                Console.WriteLine("Ready to get connections. Waiting for them...");

                while (true)
                {
                    TcpClient tcpClient = tcpListener.AcceptTcpClient();

                    ClientObject clientObject = new ClientObject(tcpClient, this);
                    Thread clientThread = new Thread(new ThreadStart(clientObject.Process));
                    clientThread.Start();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Disconnect();
            }
        }

        public void BroadcastMessage(string message, string id)
        {
            byte[] data = Encoding.Unicode.GetBytes(message);
            foreach (var t in clients)
            {
                if (t.Id != id)
                {
                    t.Stream.Write(data, 0, data.Length);
                }
            }
        }
        public void Disconnect()
        {
            tcpListener.Stop();

            foreach (var t in clients)
            {
                t.Close();
            }
            Environment.Exit(0);
        }
    }
}