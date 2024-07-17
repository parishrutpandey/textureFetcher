using System.Collections.Generic;
using System.Text.Json.Serialization;


namespace TextureFetcher;


partial class AmbientCGProvider
{
    /// <summary>
    /// HTTP Response deserialization target.
    /// </summary>
    public class HttpJsonResponseTarget
    {
        public List<ResponseElement>? foundAssets { get; set; }

        public class ResponseElement
        {
            public string? assetId { get; set; }
            public int? dimensionX { get; set; }
            public int? dimensionY { get; set; }
            public List<string>? tags { get; set; }
            public string? dataType { get; set; }
            public DownloadFolders? downloadFolders { get; set; }
            // Using Dictionary over struct here since deserialization to struct not working.
            public Dictionary<string, string>? previewImage { get; set; }


            public class DownloadFolders
            {
                [JsonPropertyName("default")]
                public Default? _default { get; set; }


                public class Default
                {
                    public string? title;
                    public DownloadFiletypeCategories? downloadFiletypeCategories { get; set; }


                    public class DownloadFiletypeCategories
                    {
                        public Zip? zip { get; set; }

                        public class Zip
                        {
                            public string? title { get; set; }
                            public List<Download>? downloads { get; set; }


                            public class Download
                            {
                                public string? fullDownloadPath { get; set; }
                                public long size { get; set; }
                            }
                        }

                    }
                }
            }
        }

    }
}
