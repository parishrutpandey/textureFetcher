using System.Collections.Generic;


namespace TextureFetcher;


partial class AmbientCGProvider
{
    /// <summary>
    /// HTTP Response deserialization target.
    /// </summary>
    public class HttpJsonResponseTarget
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
}
