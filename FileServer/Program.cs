using System;

namespace FileServer
{
    class Program
    {
        static void Main(string[] args)
        {
            MetadataServiceClient msl = new MetadataServiceClient();
            Console.WriteLine("THIS IS FILE SERVER");
            Console.Write("Enter Server IP to connect (127.0.0.1 for localhost): ");
            string ip = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(ip))
                ip = "127.0.0.1";
            if (msl.SetIPAddress(ip))
                msl.StartClient().GetAwaiter().GetResult();
            else
                Console.WriteLine("Could not parse this ip address.");
            Console.WriteLine("\nPress ENTER to continue...");
            Console.Read();
        }
    }
}
