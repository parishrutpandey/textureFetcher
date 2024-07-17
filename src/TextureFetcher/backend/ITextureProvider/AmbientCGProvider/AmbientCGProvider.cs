using System.Text.Json;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System;
using System.Threading.Tasks;
using System.Drawing;

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


    private TextureMetadata generateMetadataFromJsonElement(in HttpJsonResponseTarget.ResponseElement element)
    {
        var metadata = new TextureMetadata();
        metadata.Identifier = element.assetId ?? "";
        metadata.Name = element.assetId ?? "";
        metadata.Tags = new(element.tags ?? new());
        metadata.DimensionX = element.dimensionX!.Value;
        metadata.DimensionY = element.dimensionY!.Value;
        metadata.PhysicalDimensionsInMetresX = 0;
        metadata.PhysicalDimensionsInMetresY = 0;
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
                    + "type=Material&include=tagData,dimensionsData,downloadData"
                    + $"&limit={numberOfEntriesReadPerIteration}"
                    + $"&offset={offset}";

                try {
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
                catch (Exception e)
                {
                    Logger.Log(e.ToString());
                    throw e;
                }
            }

            if (httpDeserializedResponse == null)
            {
                Logger.Log("ERROR");
                throw new Exception("Unable to deserialze json response.");
            }

            if (httpDeserializedResponse.foundAssets.Count == 0)
                break;

            foreach (var asset in httpDeserializedResponse.foundAssets)
            {
                var metadata = this.generateMetadataFromJsonElement(asset);
                returnValue.Add(metadata);
            }
        }
        return returnValue;
    }


    public async Task<Bitmap> GetThumbnail(string identifier, int lod, IProgress<float> progress)
    {
        var httpClient = new HttpClient();
        var response = await httpClient.GetAsync(
                $"https://ambientcg.com/api/v2/full_json?q={identifier}&include=imageData");
        HttpJsonResponseTarget? jsonResponse = await JsonSerializer.
            DeserializeAsync<HttpJsonResponseTarget>(response.Content.ReadAsStream());
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
        var thumbNailByteArray = await thumbnailResponse.Content.ReadAsByteArrayAsync();

        return new Bitmap(new MemoryStream(thumbNailByteArray));
    }

    public Task<Material> GenerateMaterialFromDownload(Stream downloadData)
    {
        throw new NotImplementedException();
    }
}
