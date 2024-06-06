using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace TextureFetcher;


class Index
{
    string Directory { get; }
    public string FileName { get; }
    public string FilePath
    {
        get
        {
            return Directory + FileName;
        }
    }


    /// <summary>
    ///  Invalid paths are:
    /// - Paths with invalid characters such as '<'.
    /// - Paths that doen't terminate with '/'
    /// </summary>
    /// <param name="path"></param>
    /// <exception cref="Exception"></exception>
    private static void IsValidFilePath(string path)
    {
        if (path.IndexOfAny(Path.GetInvalidPathChars()) >= 0 || path[path.Length - 1] != '/')
            throw new Exception("Invalid File Path");
    }


    /// <summary>
    /// Throws Exception if Invalid. Invalid filenames are:
    /// - Names with invalid characters such as '<'.
    /// </summary>
    /// <param name="name"></param>
    /// <exception cref="Exception"></exception>
    private static void IsValidFileName(string name)
    {

        if (name.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            throw new Exception("Invalid File Name");
    }


    public Index(string arg_Directory, string arg_FileName)
    {
        IsValidFilePath(arg_Directory);
        IsValidFileName(arg_FileName);
        Directory = arg_Directory;
        FileName = arg_FileName;
    }


    /// <summary>
    /// </summary>
    public Task WriteToIndex(IProgress<float> progressRatio,
        List<TextureMetadata> data)
    {
        return Task.Run(() =>
            {
                Console.WriteLine(FilePath);
                var fileStream = File.Open(FilePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);
                {
                    IndexData index = new();
                    index.InitializeFromMetadata(data);
                    new XmlSerializer(typeof(IndexData)).Serialize(fileStream, index);
                }
            }
        );
    }


    public Task<IndexData?> ReadFromIndex(IProgress<float> progressRatio)
    {
        return Task.Run(() =>
            {
                IndexData? R_IndexData = new IndexData();

                var fileStream = File.Open(FilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                XmlSerializer serializer = new(typeof(IndexData));
                R_IndexData = (IndexData?)serializer.Deserialize(fileStream);
                progressRatio.Report(0.5f);

                return R_IndexData;
            });
    }


}


/// <summary>
/// Ser/DeSerialization target.
/// </summary>
public class IndexData
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
