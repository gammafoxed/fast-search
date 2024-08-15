using System.Windows;
using Microsoft.FSharp.Core;
using Microsoft.FSharp.Control;

namespace fast_search_ui;

/// <summary>
///     Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void BrowseButton_Click(object sender, RoutedEventArgs e)
    {
        var browserWindow = new DirectoryBrowserWindow();
        if (browserWindow.ShowDialog() == true)
        {
            SearchPathTextBox.Text = browserWindow.SelectedPath;
        }
    }

    private void ClearMaskButton_Click(object sender, RoutedEventArgs e)
    {
        SearchMaskTextBox.Text = "";
    }

    private async void StartSearchButton_Click(object sender, RoutedEventArgs e)
    {
        var path = SearchPathTextBox.Text;
        var mask = SearchMaskTextBox.Text;

        if (string.IsNullOrWhiteSpace(path) || string.IsNullOrWhiteSpace(mask))
        {
            MessageBox.Show("Please enter both search path and mask.");
            return;
        }

        ProgressBar.IsIndeterminate = true;
        
        await Task.Run(() => SearchFiles(path, mask));
        ProgressBar.IsIndeterminate = false;
    }

    private static async Task SearchFiles(string path, string mask)
    {
    }
}

