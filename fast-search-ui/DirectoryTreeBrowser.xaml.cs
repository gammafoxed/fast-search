using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace fast_search_ui;

public partial class DirectoryTreeBrowser
{
    public DirectoryTreeBrowser()
    {
        InitializeComponent();
        LoadDriveNodes();
    }

    public static string SelectedPath { get; private set; }

    private void LoadDriveNodes()
    {
        DirectoryTreeView.Items.Clear();
        foreach (var drive in Directory.GetLogicalDrives())
        {
            var driveItem = new TreeViewItem { Header = drive, Tag = drive };
            driveItem.Expanded += Folder_Expanded;
            DirectoryTreeView.Items.Add(driveItem);
            driveItem.Items.Add(null); // Dummy item
        }
    }

    private void Folder_Expanded(object sender, RoutedEventArgs e)
    {
        var item = (TreeViewItem)sender;
        if (item.Items is [null])
        {
            item.Items.Clear();
            try
            {
                var directories = Directory.GetDirectories((string)item.Tag);
                foreach (var directory in directories)
                {
                    var subItem = new TreeViewItem { Header = Path.GetFileName(directory), Tag = directory };
                    subItem.Expanded += Folder_Expanded;
                    item.Items.Add(subItem);
                    subItem.Items.Add(null); // Dummy item
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }
    }

    private void CollapseAllButton_Click(object sender, RoutedEventArgs e)
    {
        CollapseAllNodes(DirectoryTreeView.Items);
    }

    private void CollapseAllNodes(ItemCollection items)
    {
        foreach (TreeViewItem item in items)
        {
            item.IsExpanded = false;
            if (item.Items.Count > 0) CollapseAllNodes(item.Items);
        }
    }

    private void SelectPathButton_Click(object sender, RoutedEventArgs e)
    {
        if (DirectoryTreeView.SelectedItem is TreeViewItem selectedItem)
        {
            SelectedPath = selectedItem.Tag as string;
            Window.GetWindow(this)!.DialogResult = true;
            Window.GetWindow(this)!.Close();
        }
    }

    private void ExitButton_Click(object sender, RoutedEventArgs e)
    {
        Window.GetWindow(this)!.DialogResult = false;
        Window.GetWindow(this)!.Close();
    }
}