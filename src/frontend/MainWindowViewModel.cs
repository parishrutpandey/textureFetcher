using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace TextureFetcher;


public class MainWindowViewModel
{
    public Model Model;
    public ObservableCollection<Model.MetadataSearchResultItem> SearchResultList { get; set; }
    public MainWindowViewModel(Model _model)
    {
        Model = _model;
        Model.Initialize();
        SearchResultList = Model.metadataSearchResults;
    }

    public void FetchAmbientCG()
    {
        Model.SyncAmbientCG().Wait();
    }
}
