using System.IO;
using System;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Logging;
using Avalonia.VisualTree;
using Avalonia.LogicalTree;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.CodeAnalysis.CSharp.Syntax;


namespace TextureFetcher;

public partial class MainWindow : Avalonia.Controls.Window
{
    public MainWindow()
    {
        this.InitializeComponent();
    }


    // HACK
    public void Initialize(MainWindowViewModel datacontext)
    {
        this.DataContext = datacontext;
//        datacontext.scrollLogToBottom = LogScrollView.ScrollToEnd;
    }


    public void ClickHandler(object sender, RoutedEventArgs args)
    {
        Console.WriteLine(SearchTerm.Text);
    }


    public void OnClickQuit(object sender, RoutedEventArgs args)
    {
        this.Close();
    }


    private void SelectionChanged(object sender, SelectionChangedEventArgs args)
    {
        // HACK: Need to provide base interface for MainWindowViewModel and cast this to that.
        (this.DataContext as MainWindowViewModel).OnSelectionChanged(args);
    }

    public string ButtonName { get; set; }
}
