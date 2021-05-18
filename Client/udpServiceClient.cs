using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    class udpServiceClient
    {
        private IPAddress ipAddress = IPAddress.Parse("127.0.0.1");
        private int connectPort = 10001;
        private const int bufferSize = 1024;
        private UdpClient clientUdp = null;
        private NetworkStream serverStream = null ;

        public bool StartUDPClient(string _IP, string _port)
        {
            clientUdp = new UdpClient();
            try
            {
                ipAddress = IPAddress.Parse(_IP);
                connectPort = int.Parse(_port);
                clientUdp.Connect(ipAddress, connectPort);
            }
            catch (Exception e)
            {
                return false;
            }
            return true;
        }

        public void shutDown()
        {
            if(clientUdp != null)
            {
                clientUdp.Close();
            }
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
