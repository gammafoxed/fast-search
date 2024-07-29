module fast_search.Program

open System.Threading
open System.Threading.Tasks
open System.Windows.Forms
open ResourceTracker
open UserInterface
open Buttons
open TextBox
open ListBox
open Domain
open Labels


[<EntryPoint>]
let main argv =
    let mutable searchTask = Task.FromResult(())
    let mutable cts = new CancellationTokenSource()

    // Инициализация формы
    initForm ()

    // Настройка кнопок
    onClickDirButton (fun _ ->
        use folderDialog = new FolderBrowserDialog()

        if folderDialog.ShowDialog() = DialogResult.OK then
            setDirectoryText folderDialog.SelectedPath)

    onClickCancelButton (fun _ -> cts.Cancel())

    onClickSearchButton (fun _ ->
        let directory = getDirectoryText ()
        let pattern = getPatternText ()
        clearList ()
        cts.Cancel()
        cts.Dispose()
        cts <- new CancellationTokenSource()

        let asyncSearch = parallelFind directory pattern 4 cts.Token updateList

        Async.StartWithContinuations(
            asyncSearch,
            (fun () -> MessageBox.Show("Search completed.") |> ignore),
            (fun ex -> MessageBox.Show("Search encountered an error.") |> ignore),
            (fun _ -> MessageBox.Show("Search was canceled.") |> ignore)
        ))

    // Запуск мониторинга ресурсов
    let monitorTask =
        startTrackingAsync
            (fun cpuUsage -> updateCpuUsage cpuUsage)
            (fun memoryUsage -> updateMemoryUsage memoryUsage)
            (fun _ -> cts.Cancel())
            cts.Token
        |> Async.StartAsTask

    runApplication ()

    cts.Cancel()
    monitorTask.Wait()

    0
