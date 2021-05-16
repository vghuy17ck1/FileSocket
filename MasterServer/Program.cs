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
            ClientServiceListener csl = new ClientServiceListener();
            Console.WriteLine("THIS IS MASTER SERVER, PORT 10000 FOR FILE SERVER, 10001 FOR CLIENTS");
            Console.Write("Set this server IP (default is 127.0.0.1 or localhost): ");
            string ip = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(ip))
                ip = "127.0.0.1";
            if (msl.SetIPAddress(ip))
            {
                threadForFileServer = new Thread(() => msl.StartServer().GetAwaiter().GetResult());
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
    }
}
