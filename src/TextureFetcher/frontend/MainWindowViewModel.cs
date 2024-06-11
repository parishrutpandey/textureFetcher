using System;
using Avalonia.Controls;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Collections.Generic;
using Avalonia.Media.Imaging;
using System.ComponentModel;
using System.Text;


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

    public Bitmap? Preview
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

    private string? logText;

    public string? LogText
    {
        get
        {
            return logText;
        }
        set
        {
            logText = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LogText)));
        }
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


    private void SetupLogging()
    {
        SubscribeableSink.Instance.LogEvent += (Serilog.Events.LogEvent ev) =>
            {
                var stringBuilder = new StringBuilder();
                new Serilog.Formatting.Display.MessageTemplateTextFormatter
                    ("[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
                    .Format(ev, new System.IO.StringWriter(stringBuilder));
                this.LogText += stringBuilder.ToString();
            };
    }


    private Task updateThumbnail(string id)
    {
        return Task.Run(() =>
            {
                var prov = new AmbientCGProvider();
                Preview = prov.GetThumbnail(id, 2, new Progress<float>()).Result;
            });
    }


    public MainWindowViewModel(in Model _model)
    {
        LoadCacheFromDisk();
        Model = _model;
        _SearchText = "";
        SearchResultList = new();
        SetupLogging();
    }


    public Task? OnSelectionChanged(SelectionChangedEventArgs args)
    {
        var dataGrid = args.Source as DataGrid;
        if (dataGrid == null)
            return null;
        var idOfSelection = ((MetadataSearchResultItem)dataGrid.SelectedItem).Name;
        Logger.Log("A new log", Serilog.Events.LogEventLevel.Fatal );
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
                Search(SearchText);
            });
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
