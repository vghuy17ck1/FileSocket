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
        private CancellationTokenSource cancellation;
        private List<TcpClient> clientList;
        public List<SocketFileManager> socketFileManagers { get; set; }

        public MetadataServiceListener()
        {
            cancellation = new CancellationTokenSource();
            clientList = new List<TcpClient>();
            socketFileManagers = new List<SocketFileManager>();
        }

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
                    Console.WriteLine($"File server {counter} connected: {client.Client.RemoteEndPoint}");
                    // Read size of file list
                    byte[] buffer = new byte[bufferSize];
                    NetworkStream ns = client.GetStream();
                    ns.Read(buffer, 0, buffer.Length);
                    string lengthMessage = Encoding.ASCII.GetString(buffer);
                    int sizeOfNextMessage = int.Parse(lengthMessage);
                    // Create a new thread, then read the message by size
                    Thread c = new Thread(() => ServerReceive(client, sizeOfNextMessage));
                    c.Start();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                listener.Stop();
            }
        }

        public void ServerReceive(TcpClient client, int messageSize)
        {
            // Read message by size
            byte[] buffer = new byte[messageSize];
            string message;
            while (true)
            {
                try
                {
                    // Receive file list
                    NetworkStream stream = client.GetStream();
                    stream.Read(buffer, 0, buffer.Length);
                    message = Encoding.ASCII.GetString(buffer);
                    // Convert file list from json to object
                    ReadMessage(message, client.Client.RemoteEndPoint.ToString());
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Console.WriteLine($"Client Disconnected: {client.Client.RemoteEndPoint}");
                    // Remove from master server file list when disconnected
                    socketFileManagers.Remove(socketFileManagers.Find(m => m.ServerAddress == client.Client.RemoteEndPoint.ToString()));
                    clientList.Remove(client);
                    break;
                }
            }
        }

        public void ReadMessage(string message, string serverAddress)
        {
            try
            {
                SocketFileManager fileManager = SocketFileManager.FromJson(message);
                fileManager.ServerAddress = serverAddress;
                // Add to master server file list
                Console.WriteLine($"Received files from client {fileManager.ServerAddress}");
                fileManager.PrintAllFiles();
                socketFileManagers.Add(fileManager);
            }
            catch (JsonException ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        public bool SetIPAddress(string ip)
        {
            return IPAddress.TryParse(ip, out ipAddress);
        }
    }
}
