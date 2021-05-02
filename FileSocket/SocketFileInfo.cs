using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileSocket
{
    public class SocketFileInfo
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public string Type { get; set; }
        public long Size { get; set; }
        public string MD5 { get; set; }

        public SocketFileInfo(int id, string name, string path, string type, long size, string md5)
        {
            ID = id;
            Name = name;
            Path = path;
            Type = type;
            Size = size;
            MD5 = md5;
        }
    }
}
