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

        private MetadataServiceListener msl = null;
        private NetworkStream serverStream = null;
        private TcpListener clientListener;
        private CancellationTokenSource cancellation;
        private List<TcpClient> clientList;

        public ClientServiceListener(MetadataServiceListener _inMsl)
        {
            msl = _inMsl;
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
                    Console.WriteLine($"\nClient {counter} connected: {client.Client.RemoteEndPoint}");

                    // This thread always ready to receive signal and serve this client till this client shutdown
                    Thread c = new Thread(() => ServiceForClient(client));
                    c.Start();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                clientListener.Stop();
            }
        }

        public void ServiceForClient(TcpClient client)
        {
            string message = "";
            while (true)
            {
                try
                {
                    // read the bytes length of message first
                    byte[] buffer = new byte[bufferSize];
                    NetworkStream ns = client.GetStream();
                    ns.Read(buffer, 0, buffer.Length);
                    string lengthMessage = Encoding.ASCII.GetString(buffer);

                    // get the length
                    int sizeOfNextMessage = int.Parse(lengthMessage);
                    buffer = new byte[sizeOfNextMessage];

                    // read message after know the bytes length
                    NetworkStream stream = client.GetStream();
                    stream.Read(buffer, 0, buffer.Length);
                    message = Encoding.ASCII.GetString(buffer);
 
                    // handle message from client here (signal request)
                    if (message == "refresh")
                    {
                        Console.WriteLine($"\nClient {client.Client.RemoteEndPoint} request to receive files list info!");

                        // send files list to client
                        serverStream = client.GetStream();
                        string fileListInfoAtJson = GetFileInfo();
                        
                        Thread.Sleep(1000);

                        if (SendMessageSignal(fileListInfoAtJson) == true)
                        {
                            Console.WriteLine($"Sent file list to {client.Client.RemoteEndPoint}!");
                        }
                        else
                        {
                            Console.WriteLine($"Failed to Send file list to {client.Client.RemoteEndPoint}!");
                        }
                    }
                    else
                    {
                        // futher upgrade here, base on signal
                    }
                    
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

        private string GetFileInfo()
        {
            List<SocketFileManager> manager = msl.socketFileManagers;
            //List<string> rs = new List<string>();
            //foreach(SocketFileManager item in manager)
            //{
            //    rs.Add(JsonSerializer.Serialize(manager[0].FileList));
            //}
            return JsonSerializer.Serialize(manager);
        }

        public bool SetIPAddress(string ip)
        {
            return IPAddress.TryParse(ip, out ipAddress);
        }

        public bool SendMessageSignal(string message)
        {
            if (serverStream == null)
                return false;

            try
            {
                //send bytes size of message first
                byte[] outStream = Encoding.ASCII.GetBytes(message.Length.ToString());
                serverStream.Write(outStream, 0, outStream.Length);
                serverStream.Flush();
                //then send message
                outStream = Encoding.ASCII.GetBytes(message);
                serverStream.Write(outStream, 0, outStream.Length);
                serverStream.Flush();
            }
            catch (Exception e)
            {
                return false;
            }
            return true;
        }
    }
}
