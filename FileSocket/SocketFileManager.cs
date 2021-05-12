using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading.Tasks;

namespace FileSocket
{
    public class SocketFileManager
    {
        public List<SocketFileInfo> FileList { get; set; }
        public string ServerAddress { get; set; }
        
        public SocketFileManager()
        {
            FileList = new List<SocketFileInfo>();
        }

        public SocketFileManager(List<SocketFileInfo> fileList)
        {
            FileList = fileList;
        }

        public SocketFileManager(List<SocketFileInfo> fileList, string serverAddress)
        {
            FileList = fileList;
            ServerAddress = serverAddress;
        }

        public static Task<SocketFileManager> ScanFiles(string path)
        {
            return Task.Run(() =>
            {
                List<SocketFileInfo> fileList = new List<SocketFileInfo>();
                SocketFileManager manager = new SocketFileManager(fileList);
                if (!Directory.Exists(path))
                {
                    Console.WriteLine($"Error: Directory {path} not found");
                    return manager;
                }
                string[] files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);
                int fileId = 0;
                foreach (string file in files)
                {
                    FileInfo fileInfo = new FileInfo(file);
                    using (FileStream stream = File.OpenRead(file))
                    {
                        MD5 md5 = MD5.Create();
                        string checksum = BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", string.Empty);
                        SocketFileInfo sfi = new SocketFileInfo(fileId, fileInfo.Name, file, fileInfo.Extension, fileInfo.Length, checksum);
                        fileList.Add(sfi);
                        fileId++;
                    }
                }
                return manager;
            });
        }

        public static SocketFileManager FromJson(string jsonString)
        {
            return new SocketFileManager
            {
                FileList = JsonSerializer.Deserialize<List<SocketFileInfo>>(jsonString)
            };
        }

        public void PrintAllFiles()
        {
            Console.WriteLine("Id\tName\t\t\t\tPath\t\t\t\t\t\tType\t\tSize\t\tMD5");
            foreach (SocketFileInfo sfi in FileList)
            {
                string shortPath = sfi.Path.Length > 45 ? sfi.Path.Substring(0, 45) : sfi.Path;
                Console.WriteLine($"{sfi.ID, -4}\t{sfi.Name, -25}\t{shortPath, -45}\t{sfi.Type, -10}\t{sfi.Size, -10}\t{sfi.MD5, -10}");
            }
        }

        public string Json()
        {
            return JsonSerializer.Serialize(FileList);
        }
    }
}
