using System.Threading;
using System;
using Avalonia.Controls;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Avalonia.Threading;
using Fastenshtein;
using System.Collections.Generic;
namespace TextureFetcher;


public class MainWindowViewModel
{
    private Model Model;
    private string _SearchText;
    private string SearchText
    {
        get
        {
            return _SearchText;
        }
        set
        {
            _SearchText = value;
            Search(value);
        }
    }


    private ObservableCollection<MetadataSearchResultItem> SearchResultList { get; }


    public MainWindowViewModel(in Model _model)
    {
        LoadIndexFromDisk();
        Model = _model;
        _SearchText = "";
        SearchResultList = new();
    }


    public Task FetchAmbientCG()
    {
        return Task.Run(() =>
            {
                Model.SyncAmbientCG().Wait();
            });
    }


    public Task LoadIndexFromDisk()
    {
        return Task.Run(() =>
            {
                Model.LoadFromDisk().Wait();
                if (Model.inMemoryCache == null)
                {
                    throw new Exception("Index loading failed.");
                }
                Search(SearchText);
            });
    }


    private void Search(string query)
    {
        if(Model.inMemoryCache == null)
            throw new Exception("In Memory Cache Null");

        List<TextureMetadata> searchResult = Searcher.Search(query, Model.inMemoryCache);
    }


    public Task SearchAsync(string query)
    {
        return Task.Run(() =>
            {
                Search(query);
            });
    }


    private struct MetadataSearchResultItem
    {
        public string Name { get; set; }
        public int DimensionX { get; set; }
        public int DimensionY { get; set; }
        public string TagsCommaSeperated { get; set; }


        public static MetadataSearchResultItem
            GenerateFromMetadataItem(TextureMetadata metadata)
        {
            var R_Item = new MetadataSearchResultItem();
            R_Item.DimensionX = metadata.dimensionX;
            R_Item.DimensionY = metadata.dimensionY;
            R_Item.Name = metadata.name;

            R_Item.TagsCommaSeperated = "";
            foreach (var tag in metadata.tags)
            {
                R_Item.TagsCommaSeperated += " " + tag;
            }
            return R_Item;
        }
    }
}
