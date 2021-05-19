using FileSocket;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace FileServer
{
    public class MetadataServiceClient
    {
        public static int Port { get; set; } = 10000;
        private IPAddress ipAddress = IPAddress.Parse("127.0.0.1");
        private TcpClient client;
        private NetworkStream stream;

        public void StartClient()
        {
            client = new TcpClient();
            try
            {
                client.Connect(ipAddress, Port);
                Console.WriteLine("Connected to master server");
                stream = client.GetStream();
                string message = GetFileInfo();
                byte[] outStream = Encoding.UTF8.GetBytes(message.Length.ToString());
                stream.Write(outStream, 0, outStream.Length);
                stream.Flush();
                outStream = Encoding.UTF8.GetBytes(message);
                stream.Write(outStream, 0, outStream.Length);
                stream.Flush();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public void SendMessage(string message)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(message);
            stream.Write(buffer);
            stream.Flush();
        }

        public bool SetIPAddress(string ip)
        {
            return IPAddress.TryParse(ip, out ipAddress);
        }

        private string GetFileInfo()
        {
            SocketFileManager manager = SocketFileManager.ScanFiles("data");
            manager.ServerAddress = $"{ipAddress}:{DownloadServiceListener.Port}";
            return manager.Json();
        }
    }
}
