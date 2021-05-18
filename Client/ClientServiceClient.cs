using System;
using FileSocket;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;

namespace Client
{
    class ClientServiceClient
    {
        private IPAddress ipAddress = IPAddress.Parse("127.0.0.1");
        private int connectPort = 10001;
        private const int bufferSize = 1024;
        private TcpClient clientSocket = null;
        private NetworkStream serverStream = null;

        public List<SocketFileManager> socketFileManagers { get; set; }

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
            catch(Exception e)
            {
                return false;
            }
            return true;        
        }

        public string refreshSignalThrowBack()
        {
            if (clientSocket == null)
                return "";

            string message = "";

            //read the bytes length of message first
            NetworkStream ns = clientSocket.GetStream();
            byte[] buffer = new byte[bufferSize];
            ns = clientSocket.GetStream();
            ns.Read(buffer, 0, buffer.Length);

            //take the lenght of next message to read it
            string lengthMessage = Encoding.ASCII.GetString(buffer);
            int sizeOfNextMessage = int.Parse(lengthMessage);
            buffer = new byte[sizeOfNextMessage];

            // read next message with length already known
            ns.Read(buffer, 0, buffer.Length);
            message = Encoding.ASCII.GetString(buffer);

            readFromAllFileServer(message);
            return "OK";
        }

        public void readFromAllFileServer(string message)
        {
            try
            {
                socketFileManagers = SocketFileManager.FromJsonList(message);
            }
            catch (JsonException ex)
            {
                
            }
        }

        public bool SetIPAddress(string ip)
        {
            return IPAddress.TryParse(ip, out ipAddress);
        }
    }
}
