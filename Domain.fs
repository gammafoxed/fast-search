module fast_search.Domain

open System.IO
open System.Threading
open System.Threading.Tasks

let rec findFilesAsync (directory: string) (pattern: string) (semaphore: SemaphoreSlim) (ct: CancellationToken) (updateList: string -> unit) : Task<unit> = task {
    if ct.IsCancellationRequested then return ()

    do! semaphore.WaitAsync(ct) |> Async.AwaitTask

    try
        let subDirs = Directory.EnumerateDirectories(directory)
        let files = Directory.EnumerateFiles(directory, pattern)

        for file in files do
            updateList(file)

        let! subDirResults =
            subDirs
            |> Seq.map (fun subDir -> findFilesAsync subDir pattern semaphore ct updateList)
            |> Task.WhenAll
        return ()
    finally
        semaphore.Release() |> ignore
}

let parallelFind (directory: string) (pattern: string) (maxParallelism: int) (ct: CancellationToken) (updateList: string -> unit) : Task<unit> = task {
    let semaphore = new SemaphoreSlim(maxParallelism)
    let subDirs = Directory.EnumerateDirectories(directory)
    
    findFilesAsync directory pattern semaphore ct updateList |> ignore
    
    subDirs
        |> Seq.map (fun subDir -> findFilesAsync subDir pattern semaphore ct updateList)
        |> Task.WhenAll
        |> ignore
    return ()
}

