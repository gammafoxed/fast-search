module fast_search.Domain

open System
open System.IO
open System.Threading
open System.Threading.Tasks

let rec findFilesAsync (directory: string) (pattern: string) (semaphore: SemaphoreSlim) (ct: CancellationToken) (updateList: Action<string>) : Task<string list> = task {
    if ct.IsCancellationRequested then return []

    do! semaphore.WaitAsync(ct) |> Async.AwaitTask

    try
        let subDirs = Directory.EnumerateDirectories(directory)
        let files = Directory.EnumerateFiles(directory, pattern)

        for file in files do
            updateList.Invoke(file)

        let! subDirResults =
            subDirs
            |> Seq.map (fun subDir -> findFilesAsync subDir pattern semaphore ct updateList)
            |> Task.WhenAll
        return
            files
            |> Seq.toList
            @ (subDirResults |> Array.concat |> List.ofArray)
    finally
        semaphore.Release() |> ignore
}

let parallelFind (directory: string) (pattern: string) (maxParallelism: int) (ct: CancellationToken) (updateList: Action<string>) : Task<string list> = task {
    let semaphore = new SemaphoreSlim(maxParallelism)
    let subDirs = Directory.EnumerateDirectories(directory)

    let! results =
        subDirs
        |> Seq.map (fun subDir -> findFilesAsync subDir pattern semaphore ct updateList)
        |> Task.WhenAll

    let! rootFiles = findFilesAsync directory pattern semaphore ct updateList

    return rootFiles @ (results |> Array.concat |> List.ofArray)
}

