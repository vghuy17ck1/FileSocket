using FileSocket;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace MasterServer
{
    public class MetadataServiceListener
    {
        private IPAddress ipAddress = IPAddress.Parse("127.0.0.1");
        private const int port = 10000;
        private const int bufferSize = 1024;
        private TcpListener listener;
        private CancellationTokenSource cancellation = new CancellationTokenSource();
        private List<TcpClient> clientList = new List<TcpClient>();
        private List<string> fileList = new List<string>();

        public async Task StartServer()
        {
            listener = new TcpListener(ipAddress, port);
            listener.Start();
            Console.WriteLine($"Server is listening at {listener.LocalEndpoint}");
            Console.WriteLine("Waiting for connection...");
            try
            {
                int counter = 0;
                while (true)
                {
                    counter++;
                    TcpClient client = await Task.Run(() => listener.AcceptTcpClientAsync(), cancellation.Token);
                    // Add to client list
                    clientList.Add(client);
                    Console.WriteLine($"File server connected: {client.Client.RemoteEndPoint}");
                    // Read message first time from client
                    byte[] buffer = new byte[bufferSize];
                    NetworkStream ns = client.GetStream();
                    ns.Read(buffer, 0, buffer.Length);
                    string message = Encoding.ASCII.GetString(buffer);
                    Console.WriteLine(message);
                    // Create a new thread
                    Thread c = new Thread(() => ServerReceive(client));
                    c.Start();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                listener.Stop();
            }
        }

        public void ServerReceive(TcpClient client)
        {
            byte[] buffer = new byte[bufferSize];
            string message;
            while (true)
            {
                try
                {
                    NetworkStream stream = client.GetStream();
                    stream.Read(buffer, 0, buffer.Length);
                    message = Encoding.ASCII.GetString(buffer);
                    Console.WriteLine(message);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Console.WriteLine($"Client Disconnected: {client.Client.RemoteEndPoint}");
                    clientList.Remove(client);
                    break;
                }
            }
        }

        public void BoardCastMessage(string message)
        {
            try
            {
                byte[] buffer = new byte[1024];
                foreach (TcpClient client in clientList)
                {
                    NetworkStream broadcastStream = client.GetStream();
                    broadcastStream.Write(buffer, 0, buffer.Length);
                    broadcastStream.Flush();
                }
            }
            catch (SocketException ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public void ReadMessage(string message)
        {
            SocketFileManager manager = new SocketFileManager();
            try
            {
                manager.ParseJson(message);
            }
            catch (JsonException ex)
            {
                Console.WriteLine(ex.ToString());
            }
            manager.PrintAllFiles();
        }

        public bool SetIPAddress(string ip)
        {
            return IPAddress.TryParse(ip, out ipAddress);
        }
    }
}
