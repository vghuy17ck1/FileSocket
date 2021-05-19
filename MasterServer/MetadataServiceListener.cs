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
        public static int Port { get; set; } = 10000;
        private IPAddress ipAddress = IPAddress.Parse("127.0.0.1");
        private const int bufferSize = 1024;

        private TcpListener listener;
        private CancellationTokenSource cancellation;
        private List<TcpClient> fileServerList;
        public List<SocketFileManager> socketFileManagers { get; set; }

        public MetadataServiceListener()
        {
            cancellation = new CancellationTokenSource();
            fileServerList = new List<TcpClient>();
            socketFileManagers = new List<SocketFileManager>();
        }

        public void StartServer()
        {
            listener = new TcpListener(ipAddress, Port);
            listener.Start();
            Console.WriteLine($"Server for File Server(s) is listening at {listener.LocalEndpoint}");
            Console.WriteLine("Waiting for File Server...");
            try
            {
                int counter = 0;
                while (true)
                {
                    counter++;
                    TcpClient client = listener.AcceptTcpClient();
                    // Add to File server list
                    fileServerList.Add(client);
                    Console.WriteLine($"\nFile server {counter} connected: {client.Client.RemoteEndPoint}");
                    // Read size of file list
                    byte[] buffer = new byte[bufferSize];
                    NetworkStream ns = client.GetStream();
                    ns.Read(buffer, 0, buffer.Length);
                    string lengthMessage = Encoding.UTF8.GetString(buffer);
                    int sizeOfNextMessage = int.Parse(lengthMessage);
                    // Create a new thread, then read the message by size
                    Thread c = new Thread(() => ServiceForFileServer(client, sizeOfNextMessage));
                    c.Start();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                listener.Stop();
            }
        }

        public void ServiceForFileServer(TcpClient client, int messageSize)
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
                    message = Encoding.UTF8.GetString(buffer);
                    // Convert file list from json to object
                    ReadMessage(message, client.Client.RemoteEndPoint.ToString());
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Console.WriteLine($"File Server Disconnected: {client.Client.RemoteEndPoint}");
                    // Remove from master server file list when disconnected
                    socketFileManagers.Remove(socketFileManagers.Find(m => m.ServerAddress == client.Client.RemoteEndPoint.ToString()));
                    fileServerList.Remove(client);
                    return;
                }
            }
        }

        public void ReadMessage(string message, string serverAddress)
        {
            try
            {
                SocketFileManager fileManager = SocketFileManager.FromJson(message);
                // Add to master server file list
                Console.WriteLine($"Received files from File server {fileManager.ServerAddress}");
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
