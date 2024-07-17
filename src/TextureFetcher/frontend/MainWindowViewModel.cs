using System;
using Avalonia.Controls;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Collections.Generic;
using Avalonia.Media.Imaging;
using System.ComponentModel;
using System.Text;
using System.IO;


namespace TextureFetcher;



public class MainWindowViewModel : INotifyPropertyChanged
{
    private Model Model;

    private string searchText;

    public string SearchText
    {
        get
        {
            return searchText;
        }
        set
        {
            searchText = value;
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

    private ObservableCollection<MetadataSearchResultItem> SearchResultList { get; }

    public event PropertyChangedEventHandler? PropertyChanged;

    public Action? scrollLogToBottom { get; set; }

    private ObservableCollection<string> logText;

    public ObservableCollection<string> LogText
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
        {
            throw new Exception("In Memory Cache Null");
        }

        List<TextureMetadata> searchResult = Searcher.Search(query, Model.inMemoryCache);
        Logger.Log(searchResult.Count + " Results found.");

        SearchResultList.Clear();
        foreach (var result in searchResult)
        {
            SearchResultList.Add(MetadataSearchResultItem.GenerateFromMetadataItem(result));
        }
        Logger.Log(SearchResultList.Count.ToString());
    }


    private void SetupLogging()
    {
        Model.logEvents.CollectionChanged += (sender, args) =>
        {
            var stringBuilder = new StringBuilder();
            foreach (var ev in args.NewItems!)
            {
                new Serilog.Formatting.Display.MessageTemplateTextFormatter
                    ("[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
                    .Format(ev as Serilog.Events.LogEvent,
                        new System.IO.StringWriter(stringBuilder));
                this.LogText.Add(stringBuilder.ToString());
                stringBuilder.Clear();
                scrollLogToBottom!();
            }
        };
    }


    private Task updateThumbnail(string id)
    {
        return Task.Run(() =>
            {
                var prov = new AmbientCGProvider();
                var tempStream = new MemoryStream();
                prov.GetThumbnail(id, 2, new Progress<float>()).Result
                       .Save(tempStream, System.Drawing.Imaging.ImageFormat.Png);
                Preview = new Bitmap(tempStream);
            });
    }


    public MainWindowViewModel(in Model _model)
    {
        Model = _model;
        searchText = "";
        SearchResultList = new();
        logText = new ObservableCollection<string>();
        SetupLogging();
        LoadCacheFromDisk();
    }


    public Task? OnSelectionChanged(SelectionChangedEventArgs args)
    {
        var dataGrid = args.Source as DataGrid;
        if (dataGrid == null)
            return null;
        var idOfSelection = ((MetadataSearchResultItem)dataGrid.SelectedItem).Name;
        Logger.Log("A new log", Serilog.Events.LogEventLevel.Fatal);
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
                Logger.Log("Loading Cache from Disk");
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


    public struct MetadataSearchResultItem
    {
        public string Name { get; set; }
        public int DimensionX { get; set; }
        public int DimensionY { get; set; }
        public string TagsCommaSeperated { get; set; }


        public static MetadataSearchResultItem
            GenerateFromMetadataItem(TextureMetadata metadata)
        {
            var R_Item = new MetadataSearchResultItem();
            R_Item.DimensionX = metadata.DimensionX;
            R_Item.DimensionY = metadata.DimensionY;
            R_Item.Name = metadata.Name;

            R_Item.TagsCommaSeperated = "";
            foreach (var tag in metadata.Tags)
            {
                R_Item.TagsCommaSeperated += " " + tag;
            }
            return R_Item;
        }
    }
}
