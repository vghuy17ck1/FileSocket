using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    class ClientServiceClient
    {
        private IPAddress ipAddress = IPAddress.Parse("127.0.0.1");
        private int connectPort = 10001;
        private TcpClient clientSocket;
        private NetworkStream serverStream;

        public async Task StartClient(string _IP, string _port)
        {
            clientSocket = new TcpClient();
            try
            {
                ipAddress = IPAddress.Parse(_IP);
                connectPort = int.Parse(_port);
                clientSocket.Connect(ipAddress, connectPort);
                serverStream = clientSocket.GetStream();
            }
            catch (Exception e)
            {
                throw e;
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
    }
}
