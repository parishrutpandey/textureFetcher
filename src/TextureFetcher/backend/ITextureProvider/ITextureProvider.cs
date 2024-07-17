using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;

namespace TextureFetcher;




public class Material
{
    public Bitmap? Albedo;
    public Bitmap? Roughness;
    public Bitmap? Metalness;
    public Bitmap? Specular;
    public Bitmap? Height;
    public Bitmap? NormalDX;
    public Bitmap? NormalGL;
    public Bitmap? AmbientOcclusion;
    public Bitmap? BentNormal;
}




public interface ITextureProvider
{
    public string GetIdentifier();
    public string GetName();
    public string GetWebsiteUrl();
    public Task<List<TextureMetadata>> GetTextureMetadata(IProgress<float> progress);
    public Task<Bitmap> GetThumbnail(string identifier, int mip, IProgress<float> progress);

    /// <summary>
    /// Returns Decoded <see cref="Material"/> from <paramref name="downloadData"/>
    /// </summary>
    public Task<Material> GenerateMaterialFromDownload(Stream downloadData);
}




/// <summary>
/// Used by <see cref="ITextureProvider.GetTextureMetadata"/>
/// </summary>
public struct TextureMetadata
{
    /// <summary>
    /// Usually domain name of website. <br/>
    /// Example: ambientcg.com
    /// </summary>
    public string SourceIdentifier { get; set; }

    /// <summary>
    /// This identifier should be unique for the <see cref="SourceIdentifier"/>. <br/>
    /// Will be used to query further information about this texture.
    /// </summary>
    public string Identifier { get; set; }

    public string Name { get; set; }

    public List<string> Tags { get; set; }

    public int DimensionX { get; set; }

    public int DimensionY { get; set; }

    public float PhysicalDimensionsInMetresX;

    public float PhysicalDimensionsInMetresY;

    public List<DownloadOption> AvailableDownloadOptions { get; set; }

    public List<MapAvailability> mapAvailability { get; set; }




    public struct MapAvailability
    {
        public Map MapName { get; }

        public Availability Availability_ { get; }


        public MapAvailability(Map mapName, Availability mapAvailability)
        {
            MapName = mapName;
            Availability_ = mapAvailability;
        }

        public enum Availability
        {
            Available,
            NotAvailable,
            Unknown
        }
    }




    public struct DownloadOption
    {
        int sizeInBytes;
        FileType filetype;
        bool isCompressed;

        [Flags]
        public enum FileType
        {
            zip,
        }
    }

    [Flags]
    public enum Map
    {
        Albedo,
        Metalness,
        Roughness,
        Specular,
        NormalGL,
        NormalDX,
        Height,
        AmbientOcclusion,
        BentNormal,
    }
}
