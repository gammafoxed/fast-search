using System.IO;
using System.Windows;

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

    private void SearchFiles(string path, string mask)
    {
        try
        {
            var files = Directory.GetFiles(path, mask, SearchOption.AllDirectories);
            foreach (var file in files)
                // Here you can handle each file as you want
                // For example, print the file path to the console
                Console.WriteLine(file);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error: {ex.Message}");
        }
    }
}