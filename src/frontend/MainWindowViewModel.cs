using System.Threading;
using System;
using Avalonia.Controls;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Avalonia.Threading;
using Fastenshtein;
using System.Collections.Generic;
using Avalonia.Media.Imaging;
using System.ComponentModel;
namespace TextureFetcher;


public class MainWindowViewModel : INotifyPropertyChanged
{
    private Model Model;
    private string _SearchText;
    public string SearchText
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

    private Bitmap? preview;

    public Bitmap Preview
    {
        get
        {
            return preview;
        }
        set
        {
            preview = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Preview)));
        }
    }

    // TODO: Make public. Or should we?
    private ObservableCollection<MetadataSearchResultItem> SearchResultList { get; }

    public event PropertyChangedEventHandler? PropertyChanged;

    public MainWindowViewModel(in Model _model)
    {
        LoadCacheFromDisk();
        Model = _model;
        _SearchText = "";
        SearchResultList = new();
        Preview = new("./512.png");
    }


    private Task updateThumbnail(string id)
    {
        return Task.Run(() =>
            {
                var prov = new AmbientCGProvider();
                Preview = prov.GetThumbnail(id, 2, new Progress<float>()).Result;
            });
    }


    public Task OnSelectionChanged(SelectionChangedEventArgs args)
    {
        var dataGrid = args.Source as DataGrid;
        var idOfSelection = ((MetadataSearchResultItem)dataGrid.SelectedItem).Name;
        return updateThumbnail(idOfSelection);
    }


    public Task FetchAmbientCG()
    {
        return Task.Run(() =>
            {
                Model.SyncAmbientCG().Wait();
            });
    }


    public Task LoadCacheFromDisk()
    {
        return Task.Run(() =>
            {
                Model.LoadFromDisk().Wait();
                if (Model.inMemoryCache == null)
                {
                    throw new Exception("Index loading failed.");
                }
                Console.WriteLine("LOADED");
                Search(SearchText);
            });
    }


    private void Search(string query)
    {
        if (Model.inMemoryCache == null)
            throw new Exception("In Memory Cache Null");

        List<TextureMetadata> searchResult = Searcher.Search(query, Model.inMemoryCache);

        SearchResultList.Clear();
        foreach (var result in searchResult)
        {
            SearchResultList.Add(MetadataSearchResultItem.GenerateFromMetadataItem(result));
        }
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
