using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;

namespace BlazorFileShare.Client.Domain
{
    public class FileMetadata
    {
        public string Name { get; set; }

        public long Size { get; set; }
        public DateTimeOffset LastModified { get; set; }
        public string ContentType { get; set; }


        public int TotalChunks
        {
            get
            {
                var chunks = (int)((double)Size / (16 * 1024));
                if(Size % 16*1024 != 0)
                {
                    return chunks + 1;
                }
                return chunks;
            }
        }

        public byte[] Serialize()
        {
            using MemoryStream m = new();
            using (BinaryWriter writer = new(m))
            {

                writer.Write(Name);
                writer.Write(Size);
                writer.Write(LastModified.ToUnixTimeSeconds());
                writer.Write(ContentType);

            }
            return m.ToArray();
        }


        public static FileMetadata Desserialize(byte[] data)
        {
            FileMetadata result = new();
            using (MemoryStream m = new(data))
            {
                using BinaryReader reader = new(m);
                Console.WriteLine(data.Length);
                result.Name = reader.ReadString();
                Console.WriteLine(result.Name);

                result.Size = reader.ReadInt64();
                Console.WriteLine(result.Size);
                result.LastModified.AddSeconds(reader.ReadInt64());
                Console.WriteLine(result.LastModified);

                result.ContentType = reader.ReadString();
                Console.WriteLine(result.ContentType);
            }
            
            return result;
        }

    }
}
