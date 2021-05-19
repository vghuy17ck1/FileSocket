using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    public class DownloadServiceClient
    {
        private int port;
        private IPAddress ipAddress;
        private UdpClient server;

        public DownloadServiceClient()
        {

        }

        public DownloadServiceClient(string ipAddress, int port)
        {
            this.ipAddress = IPAddress.Parse(ipAddress);
            this.port = port;
        }

        public DownloadServiceClient(string ipAddress, string port)
        {
            this.ipAddress = IPAddress.Parse(ipAddress);
            this.port = int.Parse(port);
        }

        public DownloadServiceClient(string endpoint)
        {
            string[] ep = endpoint.Split(":");
            ipAddress = IPAddress.Parse(ep[0]);
            port = int.Parse(ep[1]);
        }

        public async Task<byte[]> BeginFileReceiver(string path, long size)
        {
            return await Task.Run(() =>
            {
                IPEndPoint remoteEndPoint = new IPEndPoint(ipAddress, port);
                server = new UdpClient();
                server.Connect(remoteEndPoint);
                byte[] message = Encoding.UTF8.GetBytes(path);
                server.Send(message, message.Length);
                List<byte> data = new List<byte>();
                while (data.Count < size)
                {
                    byte[] received = server.Receive(ref remoteEndPoint);
                    data.AddRange(received);
                }
                data.RemoveRange((int)size, data.Count - (int)size);
                return data.ToArray();
            });
        }

        public bool SetIPAddress(string ip)
        {
            return IPAddress.TryParse(ip, out ipAddress);
        }
    }
}