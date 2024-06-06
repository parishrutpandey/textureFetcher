using System;
using Avalonia.Threading;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using TextureFetcher;


public class Model
{
    public ObservableCollection<MetadataSearchResultItem> metadataSearchResults;

    public Model()
    {
        metadataSearchResults = new();
    }


    public Task Initialize()
    {
        return Task.Run(() =>
        {
            AmbientCGProvider provider = new();
            var metadataList = provider.GetTextureMetadata(new Progress<float>()).Result;
            // Affects UI. Needs to be called on the UI Thread.
            // Fails and throws Exception otherwise.
            Dispatcher.UIThread.Post(() =>
                {
                    foreach (var elem in metadataList)
                    {
                        MetadataSearchResultItem itemToAdd = new();
                        itemToAdd.Name = elem.identifier;
                        itemToAdd.DimensionX = elem.dimensionX;
                        itemToAdd.DimensionY = elem.dimensionY;
                        foreach (var tag in elem.tags)
                        {
                            itemToAdd.TagsCommaSeperated += tag;
                            itemToAdd.TagsCommaSeperated += " ";
                        }
                        metadataSearchResults.Add(itemToAdd);
                    }
                });
        });
    }


    public Task SyncAmbientCG()
    {
        return Task.Run(() =>
            {
                AmbientCGProvider prov = new();
                var metadataList = prov.GetTextureMetadata(new Progress<float>()).Result;
                TextureFetcher.Index index = new("~/AppData/Local/TextureFetcher/", "idx.csv");
                index.WriteToIndex(new Progress<float>(), metadataList).Wait();
            });
    }


    public struct MetadataSearchResultItem
    {
        public string Name { get; set; }
        public int DimensionX { get; set; }
        public int DimensionY { get; set; }
        public string TagsCommaSeperated { get; set; }
    }
}
