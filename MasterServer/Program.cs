using System;
using System.Threading;

namespace MasterServer
{
    class Program
    {
        private static Thread threadForClients = null; //service 2
        private static Thread threadForFileServer = null; //service 1

        static void Main(string[] args)
        {
            MetadataServiceListener msl = new MetadataServiceListener();
            ClientServiceListener csl = new ClientServiceListener(msl);
            Console.Write("Set this server IP (default is 127.0.0.1 or localhost): ");
            string ip = Console.ReadLine();
            Console.WriteLine("Setting up metadata service between master server and file server");
            Console.Write("Port: ");
            int metaDataServicePort;
            if (int.TryParse(Console.ReadLine(), out metaDataServicePort))
            {
                MetadataServiceListener.Port = metaDataServicePort;
            }
            Console.WriteLine("Setting up index service between master server and client");
            Console.Write("Port: ");
            int indexServicePort;
            if (int.TryParse(Console.ReadLine(), out indexServicePort))
            {
                ClientServiceListener.Port = indexServicePort;
            }
            if (string.IsNullOrWhiteSpace(ip))
                ip = "127.0.0.1";
            if (msl.SetIPAddress(ip) && csl.SetIPAddress(ip))
            {
                threadForFileServer = new Thread(() => msl.StartServer());
                threadForClients = new Thread(()=> csl.StartServerForClient().GetAwaiter().GetResult());

                threadForFileServer.Start();
                threadForClients.Start();
            }
            else
                Console.WriteLine("Could not parse this ip address.");
            Console.WriteLine("\nSever started all services, ready to serve...");
            Console.Read();
        }

        // event when closing
        /*
        static void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            Console.WriteLine("exited");
            if(threadForClients.IsAlive == true)
            {
#pragma warning disable SYSLIB0006 // Type or member is obsolete
                threadForClients.Abort();
#pragma warning restore SYSLIB0006 // Type or member is obsolete
            }
            if(threadForFileServer.IsAlive == true)
            {
#pragma warning disable SYSLIB0006 // Type or member is obsolete
                threadForFileServer.Abort();
#pragma warning restore SYSLIB0006 // Type or member is obsolete
            }
        }
        */
    }
}
