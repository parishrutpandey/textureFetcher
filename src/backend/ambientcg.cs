using System.Text.Json;
using System.Text.Json.Serialization;
using Avalonia;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Json;

namespace textureFetcher;

/// <summary>
/// To be used as deserialization target.
/// </summary>
public class AmbientCGJsonResponse
{
    public class ResponseElement
    {
        public int dimensionX { get; set; }
        public int dimensionY { get; set; }
        public PreviewImageElement previewImage { get; set; }


        public class PreviewImageElement
        {
            [JsonPropertyName("1024-PNG")]
            public string url { get; set; }
        }
    }
    public List<ResponseElement> foundAssets { get; set; }
}


/// WIP exploratory
class ambientcgtest
{
    [textureFetcher.DebugUsage]
    public static void get()
    {
        var httpClient = new HttpClient();
        var message = new HttpRequestMessage(HttpMethod.Get,
                "https://ambientcg.com/api/v2/full_json?q=floor&include=dimensionsData,include=tagData,imageData");
        var response = httpClient.Send(message);
        using var reader = new StreamReader(response.Content.ReadAsStream());
        string stringResponse = reader.ReadToEnd();
        var jsonResponse = JsonDocument.Parse(stringResponse);
        AmbientCGJsonResponse structResponse = JsonSerializer.Deserialize<AmbientCGJsonResponse>(jsonResponse);
        Console.WriteLine(stringResponse);
        Console.WriteLine(structResponse?.foundAssets[10].previewImage.url);
    }
}


interface ITextureProvider
{
    public string GetWebsiteName();
    public string GetWebsiteUrl();
    public List<SearchResponseElement> Search(string query);
    public Avalonia.Controls.Image GetThumbnail(string identifier, int maxSizeInBytes);
}


/// WIP exploratory
/// <summary>
/// Individial element in a texture search.
/// </summary>
struct SearchResponseElement
{
    /// <summary>
    /// This identifier should map to a single texture in its website.
    /// Will be used to search and find more information about
    /// this texture.
    /// </summary>
    string identifier;
    string name;
    List<string> tags;
    int dimensionX;
    int dimensionY;
    List<Format> availableFormats;


    struct Format
    {
        int pixelsX;
        int pixelsY;
        int bitDepthR;
        int bitDepthG;
        int bitDepthB;
        int bitDepthA;
        int sizeInBytes;
        float physicalDimensionsInMetresX;
        float physicalDimensionsInMetresY;
        bool isHDR;
        ImageFormat imageFormat;


        enum ImageFormat
        {
            tiff,
            jpg,
            png
        }
    }
}
