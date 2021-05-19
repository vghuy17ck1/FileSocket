using System;
using System.Threading;

namespace FileServer
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Write("Enter Server IP: ");
            string serviceIp = Console.ReadLine();
            Console.WriteLine("Setting up download service between file server and client");
            DownloadServiceListener dsl = new DownloadServiceListener();
            Console.Write("Port: ");
            int downloadServicePort;
            if (int.TryParse(Console.ReadLine(), out downloadServicePort))
            {
                DownloadServiceListener.Port = downloadServicePort;
            }
            if (string.IsNullOrWhiteSpace(serviceIp))
            {
                serviceIp = "127.0.0.1";
            }
            if (!dsl.SetIPAddress(serviceIp))
            {
                Console.WriteLine("Could not parse this ip address.");
                Console.Read();
                return;
            }
            Console.WriteLine("Setting up metadata service between file server and master server");
            MetadataServiceClient msl = new MetadataServiceClient();
            Console.Write("Port: ");
            int metadataServicePort;
            if (int.TryParse(Console.ReadLine(), out metadataServicePort))
            {
                MetadataServiceClient.Port = metadataServicePort;
            }
            if (!msl.SetIPAddress(serviceIp))
            {
                Console.WriteLine("Could not parse this ip address.");
                Console.Read();
                return;
            }
            Thread downloadServiceThead = new Thread(() => dsl.StartServer());
            downloadServiceThead.Start();
            Thread metadataServiceThead = new Thread(() => msl.StartClient());
            metadataServiceThead.Start();
        }
    }
}
