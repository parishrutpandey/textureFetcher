using System.Text.Json;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System;
using NetVips;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using NetVips.Extensions;

namespace TextureFetcher;



partial class AmbientCGProvider : ITextureProvider
{
    public string GetIdentifier()
    {
        return "ambientCG";
    }


    public string GetName()
    {
        return "ambientCG";
    }


    public string GetWebsiteUrl()
    {
        return "https://ambientcg.com/";
    }


    private TextureMetadata GenerateMetadataFromJsonElement(in HttpJsonResponseTarget.ResponseElement element)
    {
        var metadata = new TextureMetadata();
        metadata.name = element.assetId ?? "";
        metadata.tags = new(element.tags ?? new());
        metadata.dimensionX = element.dimensionX.Value;
        metadata.dimensionY = element.dimensionY.Value;
        metadata.identifier = element.assetId ?? "";
        return metadata;
    }


    public async Task<List<TextureMetadata>> GetTextureMetadata(IProgress<float> progress)
    {
        List<TextureMetadata> returnValue = new();
        for (int iteration = 0; ; iteration++)
        {
            int numberOfEntriesReadPerIteration = 250;
            int offset = numberOfEntriesReadPerIteration * iteration;

            HttpJsonResponseTarget? httpDeserializedResponse;
            {
                string requestString =
                    $"https://ambientcg.com/api/v2/full_json?"
                    + "type=Material&include=tagData,dimensionsData"
                    + $"&limit={numberOfEntriesReadPerIteration}"
                    + $"&offset={offset}";

                httpDeserializedResponse =
                    JsonSerializer.Deserialize<HttpJsonResponseTarget>(
                        JsonDocument.Parse(
                            new StreamReader(
                                new HttpClient().GetAsync(
                                    requestString
                                )
                                .Result.Content.ReadAsStream()
                            )
                                .ReadToEnd()
                        )
                    )
                ;
            }

            if (httpDeserializedResponse == null)
            {
                throw new Exception("Unable to deserialze json response.");
            }

            // TODO: Ensure null correctness.
            if (httpDeserializedResponse.foundAssets.Count == 0)
                break;

            foreach (var asset in httpDeserializedResponse.foundAssets)
            {
                var metadata = this.GenerateMetadataFromJsonElement(asset);
                returnValue.Add(metadata);
            }
        }
        return returnValue;
    }


    public async Task<Bitmap> GetThumbnail(string identifier, int lod, IProgress<float> progress)
    {
        var httpClient = new HttpClient();
        Console.WriteLine("1");
        var response = await httpClient.GetAsync(
                $"https://ambientcg.com/api/v2/full_json?q={identifier}&include=imageData");
        Console.WriteLine("2");
        HttpJsonResponseTarget? jsonResponse = await JsonSerializer.
            DeserializeAsync<HttpJsonResponseTarget>(response.Content.ReadAsStream());
        Console.WriteLine("3");
        if (jsonResponse == null)
        {
            throw new Exception("Unable to deserialize http response.");
        }
        if (jsonResponse.foundAssets == null)
        {
            throw new Exception($"Asset with id:{identifier} not found.");
        }
        var thumbnailUrls = jsonResponse.foundAssets[0].previewImage;
        string thumbnailUrlToFetch = "";
        switch (lod)
        {
            case 0:
                thumbnailUrlToFetch = thumbnailUrls["512-PNG"];
                break;
            case 1:
                thumbnailUrlToFetch = thumbnailUrls["256-PNG"];
                break;
            case 2:
                thumbnailUrlToFetch = thumbnailUrls["128-PNG"];
                break;
            case 3:
                thumbnailUrlToFetch = thumbnailUrls["64-PNG"];
                break;
            default:
                throw new Exception("lod must be 0, 1, 2 or 3");
        }
        var thumbnailResponse = await httpClient.GetAsync(thumbnailUrlToFetch);
        Console.WriteLine("4");
        var thumbNailByteArray = await thumbnailResponse.Content.ReadAsByteArrayAsync();
        Console.WriteLine("5");
        Image image = Image.NewFromBuffer(thumbNailByteArray);

        return new Bitmap(new MemoryStream(thumbNailByteArray));
    }


    [TextureFetcher.DebugUsage]
    public static void ambientcgprovider()
    {
        AmbientCGProvider prov = new();
        Progress<float> progress = new();
        var result = prov.GetTextureMetadata(progress).Result;

        JsonSerializerOptions opts = new() { WriteIndented = true };
        var json = JsonSerializer.Serialize(result, opts);
        File.WriteAllText("./testfile.json", json);
    }
}
