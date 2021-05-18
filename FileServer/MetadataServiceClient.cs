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
        private IPAddress ipAddress = IPAddress.Parse("127.0.0.1");
        private const int connectPort = 10000;
        private TcpClient clientSocket;
        private NetworkStream serverStream;

        public async Task StartClient()
        {
            clientSocket = new TcpClient();
            try
            {
                clientSocket.Connect(ipAddress, connectPort);
                Console.WriteLine("Connected to master server");
                serverStream = clientSocket.GetStream();
                string message = await GetFileInfo();
                
                byte[] outStream = Encoding.ASCII.GetBytes(message.Length.ToString());
                serverStream.Write(outStream, 0, outStream.Length);
                serverStream.Flush();
                outStream = Encoding.ASCII.GetBytes(message);
                serverStream.Write(outStream, 0, outStream.Length);
                serverStream.Flush();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public void SendMessage(string message)
        {
            byte[] buffer = Encoding.ASCII.GetBytes(message);
            serverStream.Write(buffer);
            serverStream.Flush();
        }

        public bool SetIPAddress(string ip)
        {
            return IPAddress.TryParse(ip, out ipAddress);
        }

        private async Task<string> GetFileInfo()
        {
            SocketFileManager manager = await SocketFileManager.ScanFiles("data");
            return manager.Json();
        }
    }
}
