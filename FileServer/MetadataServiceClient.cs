using FileSocket;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FileServer
{
    public class MetadataServiceClient
    {
        private IPAddress ipAddress = IPAddress.Parse("127.0.0.1");
        private const int port = 10000;
        private const int bufferSize = 1024;
        private TcpClient clientSocket;
        private NetworkStream serverStream = default(NetworkStream);
        private Thread clientThread;

        public async Task StartClient()
        {
            clientSocket = new TcpClient();
            try
            {
                clientSocket.Connect(ipAddress, port);
                Console.WriteLine("Connected to master server");
                serverStream = clientSocket.GetStream();
                string message = await GetFileInfo();
                byte[] outStream = Encoding.ASCII.GetBytes(message);
                serverStream.Write(outStream, 0, outStream.Length);
                serverStream.Flush();
                clientThread = new Thread(GetMessage);
                clientThread.Start();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private void GetMessage()
        {
            try
            {
                while (true)
                {
                    serverStream = clientSocket.GetStream();
                    byte[] inStream = new byte[bufferSize];
                    serverStream.Read(inStream, 0, inStream.Length);
                    if (!IsSocketConnected(clientSocket))
                    {
                        Console.WriteLine("You have been disconnected from server");
                        #pragma warning disable SYSLIB0006 // Type or member is obsolete
                        clientThread.Abort();
                        #pragma warning restore SYSLIB0006 // Type or member is obsolete
                        clientSocket.Close();
                    }
                    string message = Encoding.ASCII.GetString(inStream);
                    Console.WriteLine(message);
                }
            }
            catch (Exception e)
            {
                #pragma warning disable SYSLIB0006 // Type or member is obsolete
                clientThread.Abort();
                #pragma warning restore SYSLIB0006 // Type or member is obsolete
                clientSocket.Close();
                Console.WriteLine(e);
            }
        }

        public void SendMessage(string message)
        {
            byte[] buffer = Encoding.ASCII.GetBytes(message);
            serverStream.Write(buffer);
            serverStream.Flush();
        }

        private bool IsSocketConnected(TcpClient s)
        {
            bool flag = false;
            try
            {
                bool part1 = s.Client.Poll(10, SelectMode.SelectRead);
                bool part2 = (s.Available == 0);
                if (part1 && part2)
                    flag = false;
                else
                    flag = true;
            }
            catch (Exception er)
            {
                Console.WriteLine(er);
            }
            return flag;
        }

        public bool SetIPAddress(string ip)
        {
            return IPAddress.TryParse(ip, out ipAddress);
        }

        private async Task<string> GetFileInfo()
        {
            SocketFileManager manager = new SocketFileManager();
            await manager.ScanFiles("data");
            return manager.Json();
        }
    }
}
