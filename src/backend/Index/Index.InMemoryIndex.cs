using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Xml.Serialization;

namespace TextureFetcher;


partial class Index
{
    /// <summary>
    /// Deserialization target.
    /// </summary>
    public class InMemoryIndex
    {
        public List<TextureMetadata> Data;
        public byte[]? Hash;


        private SHA256 CreateHashFromData(List<TextureMetadata> arg_data)
        {
            var R_Hash = SHA256.Create();

            var tempStream = new MemoryStream();
            new XmlSerializer(typeof(List<TextureMetadata>))
                .Serialize(tempStream, arg_data);
            R_Hash.ComputeHash(tempStream);

            return R_Hash;
        }


        /// <summary>
        /// Intialize with <paramref name="metadata"/> and compute <see cref="hash"/>.
        /// </summary>
        /// <returns> Serialized stream from <paramref name="metadata"/> </returns>
        public void InitializeFromMetadata(List<TextureMetadata> metadata)
        {
            Data = metadata;
            var outputStream = new MemoryStream();
            var hash = SHA256.Create();
            {
                var serializer = new XmlSerializer(typeof(List<TextureMetadata>));
                serializer.Serialize(outputStream, Data);
                hash.ComputeHash(outputStream);
            }
            Hash = hash.Hash;
        }


        public bool CheckIntegrity()
        {
            SHA256 computedHash = CreateHashFromData(Data);

            if (computedHash.Hash.Length != Hash.Length)
                return false;

            for (int i = 0; i < Hash.Length; i++)
            {
                if (Hash[i] != computedHash.Hash[i])
                    return false;
            }

            return true;
        }
    }
}
