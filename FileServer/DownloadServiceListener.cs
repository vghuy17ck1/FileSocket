using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FileServer
{
    public class DownloadServiceListener
    {
        public static int Port { get; set; } = 20000;
        private const int bufferSize = 10240;
        private IPAddress ipAddress = IPAddress.Parse("127.0.0.1");
        private UdpClient server;

        public void StartServer()
        {
            server = new UdpClient(Port);
            Console.WriteLine($"Download service is listening on port {Port}");
            while (true)
            {
                IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, Port);
                byte[] data = server.Receive(ref remoteEndPoint);
                Console.WriteLine($"Received data from {remoteEndPoint}");
                string path = Encoding.UTF8.GetString(data);
                Thread transferThread = new Thread(() => FileTransfer(path, remoteEndPoint));
                transferThread.Start();
            }
        }

        private void FileTransfer(string path, IPEndPoint remoteEndPoint)
        {
            if (File.Exists(path))
            {
                using (FileStream stream = File.OpenRead(path))
                {
                    byte[] data = new byte[bufferSize];
                    while (stream.Read(data, 0, data.Length) > 0)
                    {
                        server.Send(data, data.Length, remoteEndPoint);
                    }
                }
            }
            else
            {
                server.Send(new byte[0], 0, remoteEndPoint);
            }
        }

        public bool SetIPAddress(string ip)
        {
            return IPAddress.TryParse(ip, out ipAddress);
        }
    }
}
