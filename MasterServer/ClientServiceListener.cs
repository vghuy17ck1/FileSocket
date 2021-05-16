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
    class ClientServiceListener
    {
        private IPAddress ipAddress = IPAddress.Parse("127.0.0.1");
        private const int portForClient = 10001;
        private const int bufferSize = 1024;

        private TcpListener listener;
        private TcpListener clientListener;
        private CancellationTokenSource cancellation;
        private List<TcpClient> clientList;

        public ClientServiceListener()
        {
            cancellation = new CancellationTokenSource();
            clientList = new List<TcpClient>();
        }

        public async Task StartServerForClient()
        {
            clientListener = new TcpListener(ipAddress, portForClient);
            clientListener.Start();
            Console.WriteLine($"Server for Client(s) is listening at {clientListener.LocalEndpoint}");
            Console.WriteLine("Waiting for Client...");
            try
            {
                int counter = 0;
                while (true)
                {
                    counter++;
                    TcpClient client = await Task.Run(() => clientListener.AcceptTcpClientAsync(), cancellation.Token);
                    // Add to client list
                    clientList.Add(client);
                    Console.WriteLine($"\nA Client {counter} connected: {client.Client.RemoteEndPoint}");

                    // Create a new thread, then read the message by size
                    Thread c = new Thread(() => ServiceForClient(client, 999999));
                    c.Start();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                clientListener.Stop();
            }
        }

        public void ServiceForClient(TcpClient client, int messageSize)
        {
            byte[] buffer = new byte[messageSize];
            string message = "";
            while (true)
            {
                try
                {
                    // Receive file list
                    NetworkStream stream = client.GetStream();
                    stream.Read(buffer, 0, buffer.Length);
                    message = Encoding.ASCII.GetString(buffer);
                    //handle message from client here
                    Console.WriteLine("Client request: " + message);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Console.WriteLine($"Client Disconnected: {client.Client.RemoteEndPoint}");

                    clientList.Remove(client);
                    return;
                }
            }
        }

        public bool SetIPAddress(string ip)
        {
            return IPAddress.TryParse(ip, out ipAddress);
        }
    }
}
