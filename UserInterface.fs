module fast_search.UserInterface

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


module Buttons =

let onClickDirButton (onClick: unit -> unit) =
    directoryButton.Click.Add(fun _ -> onClick ())

    let onClickCancelButton (onClick: unit -> unit) =
        cancelButton.Click.Add(fun _ -> onClick ())

    let onClickSearchButton (onClick: unit -> unit) =
        searchButton.Click.Add(fun _ -> onClick ())

module TextBox =
    open Form
    let setDirectoryText text = directoryTextBox.Text <- text
    let getDirectoryText () = directoryTextBox.Text


    let setPatternText text = directoryTextBox.Text <- text
    let getPatternText () = patternTextBox.Text


    let setCpuThresholdText text = cpuThresholdTextBox.Text <- text
    let getCpuThresholdText () = cpuThresholdTextBox.Text


    let setMemoryThresholdText text = memoryThresholdTextBox.Text <- text
    let getMemoryThresholdText () = memoryThresholdTextBox.Text

module ListBox =
    open Form

    let updateList (file: string) =
        form.Invoke(fun () -> resultListBox.Items.Add(file |> ignore)) |> ignore

    let clearList () =
        form.Invoke(fun () -> resultListBox.Items.Clear()) |> ignore

module Labels =
    open Form

    let updateCpuUsage (usage: float) =
        form.Invoke(fun () -> cpuUsageLabel.Text <- $"CPU Usage: %.2f{usage}") |> ignore

    let updateMemoryUsage (usage: float) =
        form.Invoke(fun () -> memoryUsageLabel.Text <- $"Memory Usage: %.2f{usage} MB")
        |> ignore


open Form

let initForm () =
    cpuThresholdTextBox.Text <- "80"
    memoryThresholdTextBox.Text <- "1024"

let runApplication () = Application.Run(form)
