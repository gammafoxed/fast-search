module fast_search.PathChooser

open System
open System.Drawing
open System.Windows.Forms
open System.IO

module private form =
    let mutable treeView: TreeView = null
    let mutable label: Label = null
    let mutable form: Form = null

    let createTreeNode (directoryInfo: DirectoryInfo) =
        let node = TreeNode(directoryInfo.Name)
        node.Tag <- directoryInfo.FullName
        node.Nodes.Add("Loading...") |> ignore
        node

    let populateNode (node: TreeNode) =
        node.Nodes.Clear()
        let path = node.Tag :?> string
        let directoryInfo = DirectoryInfo(path)

        try
            for directory in directoryInfo.GetDirectories() do
                node.Nodes.Add(createTreeNode directory) |> ignore
        with
            | :? UnauthorizedAccessException -> ()

    let onBeforeExpand (e: TreeViewCancelEventArgs) =
        let node = e.Node
        if node.Nodes[0].Text = "Loading..." then
            populateNode node

    let onAfterSelect (e: TreeViewEventArgs) =
        let node = e.Node
        label.Text <- node.Tag :?> string

    let initializeComponents (owner: Form) =
        form <- new Form(Text = "Custom Folder Browser", Width = 400, Height = 300, Owner = owner)
        treeView <- new TreeView(Dock = DockStyle.Fill)
        label <- new Label(Dock = DockStyle.Bottom, Height = 30, TextAlign = ContentAlignment.MiddleLeft)

        treeView.BeforeExpand.Add(fun e -> onBeforeExpand e)
        treeView.AfterSelect.Add(fun e -> onAfterSelect e)

        let root = TreeNode("Computer")
        root.Nodes.Add("Loading...") |> ignore
        treeView.Nodes.Add(root) |> ignore

        treeView.BeforeExpand.Add(fun e -> 
            if e.Node = root then
                root.Nodes.Clear()
                for drive in DriveInfo.GetDrives() do
                    if drive.DriveType = DriveType.Fixed then
                        root.Nodes.Add(createTreeNode drive.RootDirectory) |> ignore
        )

        form.Controls.Add(treeView)
        form.Controls.Add(label)

let showFolderBrowser(owner: Form) =
    form.initializeComponents(owner)
    form.form.ShowDialog() |> ignore

let clearSelection() =
    form.treeView.Nodes.Clear()
    let root = new TreeNode("Computer")
    root.Nodes.Add("Loading...") |> ignore
    form.treeView.Nodes.Add(root) |> ignore
    form.label.Text <- ""