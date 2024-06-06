using System;
using System.Collections.Generic;
using NetVips;
using System.Threading.Tasks;

namespace TextureFetcher;


/// WIP exploratory
class ambientcgtest
{
    [TextureFetcher.DebugUsage]
    public static void get()
    {

    }
}

public interface ITextureProvider
{
    public string GetWebsiteName();
    public string GetWebsiteUrl();
    public Task<List<TextureMetadata>> GetTextureMetadata(in IProgress<float> progress);

    /// <summary>
    /// Image returned should be as large as the <param name="mip"> allows.
    /// Should be less than 1 MB
    /// Should be less than 250 KB
    /// Should be less than 50 KB
    /// Should be less than 10 KB
    /// </summary>
    public Task<Image> GetThumbnail(string identifier, int mip, in IProgress<float> progress);


}

/// <summary>
/// Used by <see cref="GetTextureMetadata"/>
/// </summary>
public struct TextureMetadata
{
    /// <summary>
    /// Usually domain name of website. <br/>
    /// Example: ambientcg.com
    /// </summary>
    public string sourceIdentifier;
    /// <summary>
    /// This identifier should be unique for the <see cref="sourceIdentifier"/>. <br/>
    /// Will be used to query further information about this texture.
    /// </summary>
    public string identifier { get; set; }
    public string name { get; set; }
    public List<string> tags { get; set; }
    public int dimensionX { get; set; }
    public int dimensionY { get; set; }
    public float physicalDimensionsInMetresX;
    public float physicalDimensionsInMetresY;
    public List<DownloadOption> availableDownloadOptions { get; set; }


    public struct DownloadOption
    {
        int sizeInBytes;
        FileType filetype;
        bool isCompressed;

        public enum FileType
        {
            zip,
        }
    }
}
