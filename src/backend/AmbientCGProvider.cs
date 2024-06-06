using System.Text.Json;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System;
using NetVips;
using System.Threading.Tasks;

namespace TextureFetcher;


/// <summary>
/// HTTP Response deserialization target.
/// </summary>
public class AmbientCGJsonResponse
{
    public class ResponseElement
    {
        public string? assetId { get; set; }
        public int dimensionX { get; set; }
        public int dimensionY { get; set; }
        public List<string>? tags { get; set; }
        public string? dataType { get; set; }
        // Using Dictionary over struct here since deserialization to struct not working.
        public Dictionary<string, string>? previewImage { get; set; }
    }
    public List<ResponseElement>? foundAssets { get; set; }
}


class AmbientCGProvider : ITextureProvider
{
    public string GetWebsiteName()
    {
        return "ambientCG";
    }


    public string GetWebsiteUrl()
    {
        return "https://ambientcg.com/";
    }


    private TextureMetadata GenerateMetadataFromJsonElement(in AmbientCGJsonResponse.ResponseElement element)
    {
        var metadata = new TextureMetadata();

        metadata.name = element.assetId ?? "";
        metadata.tags = new(element.tags ?? new());
        metadata.dimensionX = element.dimensionX;
        metadata.dimensionY = element.dimensionY;
        metadata.identifier = element.assetId ?? "";

        return metadata;
    }


    public Task<List<TextureMetadata>> GetTextureMetadata(in IProgress<float> progress)
    {
        return Task.Run(() =>
            {
                List<TextureMetadata> returnValue = new();
                for (int iteration = 0; ; iteration++)
                {
                    int numberOfEntriesReadPerIteration = 250;
                    int offset = numberOfEntriesReadPerIteration * iteration;

                    AmbientCGJsonResponse? httpDeserializedResponse;
                    {
                        string requestString =
                            @$"https://ambientcg.com/api/v2/full_json?
                                type=Material&include=tagData,dimensionsData
                                &limit={numberOfEntriesReadPerIteration}
                                &offset={offset}";
                        httpDeserializedResponse =
                            JsonSerializer.Deserialize<AmbientCGJsonResponse>(
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

                    if(httpDeserializedResponse == null)
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
            });
    }


    public Task<Image> GetThumbnail(string identifier, int lod, in IProgress<float> progress)
    {
        return Task.Run(() =>
                {
                    var httpClient = new HttpClient();
                    var message = new HttpRequestMessage(HttpMethod.Get,
                            $"https://ambientcg.com/api/v2/full_json?q={identifier}&include=imageData");
                    var response = httpClient.Send(message);
                    using var reader = new StreamReader(response.Content.ReadAsStream());
                    string stringResponse = reader.ReadToEnd();
                    AmbientCGJsonResponse? jsonResponse = JsonSerializer.Deserialize<AmbientCGJsonResponse>(stringResponse);
                    if( jsonResponse == null)
                    {
                        throw new Exception("Unable to deserialize http response.");
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
                    var thumbnailTask = httpClient.GetAsync(thumbnailUrlToFetch).Result.Content.
                        ReadAsByteArrayAsync();
                    thumbnailTask.Wait();
                    var thumbNailByteArray = thumbnailTask.Result;
                    Image image = Image.NewFromBuffer(thumbNailByteArray);
                    return image;
                });
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
