module fast_search.Program

open System.Threading
open System.Threading.Tasks
open System.Windows.Forms
open UserInterface
open Buttons
open TextBox
open ListBox
open Domain
open ResourceTracker


[<EntryPoint>]
let main argv =
    let mutable searchTask = Task.FromResult(())
    let mutable cts = new CancellationTokenSource()
    
    onClickDirButton (fun _ ->
        use folderDialog = new FolderBrowserDialog()
        if folderDialog.ShowDialog() = DialogResult.OK then setDirectoryText folderDialog.SelectedPath)
    
    onClickCancelButton (fun _ -> cts.Cancel())
    
    onClickSearchButton (fun _ ->
    let directory = getDirectoryText()
    let pattern = getPatternText()
    clearList()
    cts.Cancel()
    cts.Dispose()
    cts <- new CancellationTokenSource()

    searchTask <- parallelFind directory pattern 4 cts.Token updateList

    searchTask.ContinueWith(fun t ->
        match t.Status with
        | TaskStatus.RanToCompletion ->
            MessageBox.Show("Search completed.") |> ignore
        | TaskStatus.Canceled -> MessageBox.Show("Search was canceled.") |> ignore
        | TaskStatus.Faulted -> MessageBox.Show("Search encountered an error.") |> ignore
    ) |> ignore
        )
    0