namespace fast_search_ui;

public partial class DirectoryBrowserWindow
{
    public DirectoryBrowserWindow()
    {
        InitializeComponent();
    }

    public string SelectedPath => DirectoryTreeBrowser.SelectedPath;
}