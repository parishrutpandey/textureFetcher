using Avalonia.Markup.Declarative;
using Avalonia.Controls;
using Avalonia.Styling;


namespace TextureFetcher;


public class TestDeclarative : ViewBase<MainWindowViewModel>
{
    public TestDeclarative(MainWindowViewModel vm) : base(vm)
    {
        this.ViewModel = vm;
    }


    protected override void OnCreated()
    {
    }


    protected override object Build(MainWindowViewModel vm) =>
        new Grid()
            .Name("Menu_and_Content_and_Status")
            .Rows("40, Auto, 400")
            .Children(

                new Grid()
                    .Name("Menu")
                    .Row(0)
                    .Children(

                        new Grid()
                            .Children(

                                new Menu()

                                    .Items(
                                        new MenuItem()
                                            .Header("Fetch AmbientCG")
                                            .OnClick((e) => @vm.FetchAmbientCG()),
                                        new MenuItem()
                                            .Header("Load Index")
                                            .OnClick((e) => @vm.LoadCacheFromDisk())
                                    )
                            )
                    ),
                new Grid()
                    .Name("Content")
                    .Row(1)
                    .Cols("*,2,*")
                    .Children(

                        new Grid()
                            .Name("SearchAndResults")
                            .Col(0)
                            .Rows("30,*")
                            .Children(

                                new Grid()
                                    .Name("SearchBar")
                                    .Row(0)
                                    .Cols("*,100")
                                    .Children(

                                        new TextBox()
                                            .Name("SearchTerm")
                                            .Row(0)
                                            .AcceptsReturn(true)
                                            .Text(@vm.SearchText)
                                    ),
                                new Grid()
                                    .Name("SearchResults")
                                    .Row(1)
                                    .Children(

                                                //                                        new ScrollViewer()
                                                //                                            .Content(

                                                new DataGrid()
                                                {
                                                    SelectionMode = DataGridSelectionMode.Single,
                                                    AutoGenerateColumns = true,
                                                    CanUserSortColumns = true,
                                                    CanUserResizeColumns = true,
                                                    CanUserReorderColumns = true,
                                                    IsReadOnly = true,
                                                }
                                                    .Name("SearchResultlist")
                                    //.Bind(DataGrid.ItemsSourceProperty, new Binding() {
                                    //    Source = vm,
                                    //    Path = nameof(vm.SearchResultList)
                                    //})
                                    //                                        )
                                    )
                            ),
                            new GridSplitter()
                                .Col(1)
                                .ResizeDirection(GridResizeDirection.Columns),
                            new Panel()
                                .Name("PreviewImage")
                                .Col(2)
                                .Children(
                                    new Grid()
                                        .Name("Information")
                                        .Rows("*,*")
                                        .Children(

                                            new Image()
                                            {
                                                Source = @vm.Preview
                                            }
                                                .Name("PreviewImageImage")
                                                .Row(0),
                                                new Grid()
                                                .Name("DownloadOptionsContainer")
                                                .Row(1)
                                                .Cols("*,*")
                                                .Children(
                                                    new DataGrid()
                                                    {
                                                        Name = "DownloadOptions",
                                                        SelectionMode = DataGridSelectionMode.Single,
                                                        AutoGenerateColumns = true,
                                                        CanUserReorderColumns = true,
                                                        CanUserResizeColumns = true,
                                                        CanUserSortColumns = true,
                                                    }
                                                        .Col(1)
                                                )
                                        )
                                )
                    ),
                    new Grid()
                        .Name("StatusBar")
                        .Row(2)
                        .Children(

                            new DockPanel()
                                .Children(

                                    new Border()
                                        .BorderThickness(2)
                                )
                        )
            );


    private void ButtonStyle(Button b) => b
        .FontSize(12);
}

