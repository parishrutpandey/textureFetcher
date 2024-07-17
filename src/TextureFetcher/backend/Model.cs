using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.ObjectModel;


namespace TextureFetcher;


public class Model
{
    TextureFetcher.Index defaultIndex;
    public List<TextureMetadata>? inMemoryCache;
    public ObservableCollection<Serilog.Events.LogEvent> logEvents;

    public Model()
    {
        defaultIndex = new("~/AppData/Local/TextureFetcher/", "index.idx");
        logEvents = new ObservableCollection<Serilog.Events.LogEvent>();
        SetupLogging();
    }


    private void SetupLogging()
    {
        SubscribeableSink.Instance.LogEvent += (Serilog.Events.LogEvent ev) =>
            {
                logEvents.Add(ev);
            };
    }


    public async Task SyncAmbientCG()
    {
        AmbientCGProvider prov = new();
        var metadataList = prov.GetTextureMetadata(new Progress<float>()).Result;
        defaultIndex.WriteToIndex(new Progress<float>(), metadataList).Wait();
    }


    public async Task LoadFromDisk()
    {
        var data = await this.defaultIndex.ReadFromIndex(new Progress<float>());
        if (data == null || data.Data == null)
            throw new Exception("Failed to Read Index File");
        this.inMemoryCache = data.Data;
    }
}
