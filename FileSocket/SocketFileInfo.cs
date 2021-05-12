using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FileSocket
{
    public class SocketFileInfo
    {
        [JsonPropertyName("id")]
        public int ID { get; set; }
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("path")]
        public string Path { get; set; }
        [JsonPropertyName("type")]
        public string Type { get; set; }
        [JsonPropertyName("size")]
        public long Size { get; set; }
        [JsonPropertyName("md5")]
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
