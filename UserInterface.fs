module fast_search.UserInterface

open System.Diagnostics
open System.Windows.Forms

module private Form =
    let form = new Form(Text = "File Search", Width = 800, Height = 600)
    let directoryLabel = new Label(Text = "Directory:", Top = 20, Left = 10)
    let directoryTextBox = new TextBox(Top = 20, Left = 100, Width = 400)
    let directoryButton = new Button(Text = "Browse", Top = 20, Left = 510)
    let patternLabel = new Label(Text = "Pattern:", Top = 60, Left = 10)
    let patternTextBox = new TextBox(Top = 60, Left = 100, Width = 400)
    let searchButton = new Button(Text = "Search", Top = 100, Left = 100)
    let cancelButton = new Button(Text = "Cancel", Top = 100, Left = 200)
    let resultListBox = new ListBox(Top = 140, Left = 10, Width = 760, Height = 300)
    let cpuUsageLabel = new Label(Text = "CPU Usage: 0%", Top = 460, Left = 10)
    let memoryUsageLabel = new Label(Text = "Memory Usage: 0 MB", Top = 490, Left = 10)
    let cpuThresholdTextBox = new TextBox(Top = 460, Left = 200, Width = 50)
    let memoryThresholdTextBox = new TextBox(Top = 490, Left = 200, Width = 50)

    form.Controls.Add(directoryLabel)
    form.Controls.Add(directoryTextBox)
    form.Controls.Add(directoryButton)
    form.Controls.Add(patternLabel)
    form.Controls.Add(patternTextBox)
    form.Controls.Add(searchButton)
    form.Controls.Add(cancelButton)
    form.Controls.Add(resultListBox)
    form.Controls.Add(cpuUsageLabel)
    form.Controls.Add(memoryUsageLabel)
    form.Controls.Add(cpuThresholdTextBox)
    form.Controls.Add(memoryThresholdTextBox)

    directoryButton.Click.Add(fun _ ->
    use folderDialog = new FolderBrowserDialog()
    if folderDialog.ShowDialog() = DialogResult.OK then
        directoryTextBox.Text <- folderDialog.SelectedPath)
    
    

open Form
let initForm() =
    cpuThresholdTextBox.Text <- "80"
    memoryThresholdTextBox.Text <- "1024"

let updateList(file: string) =
    form.Invoke(fun () -> resultListBox.Items.Add(file |> ignore))