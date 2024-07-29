module fast_search.Domain

open System.IO
open System.Threading

let rec findFilesAsync
    (directory: string)
    (pattern: string)
    (semaphore: SemaphoreSlim)
    (ct: CancellationToken)
    (updateList: string -> unit)
    =
    async {
        if ct.IsCancellationRequested then
            return ()

        do! Async.AwaitTask(semaphore.WaitAsync(ct))

        try
            let subDirs = Directory.EnumerateDirectories(directory)
            let files = Directory.EnumerateFiles(directory, pattern)

            for file in files do updateList (file)

            let! _ =
                subDirs
                |> Seq.map (fun subDir -> findFilesAsync subDir pattern semaphore ct updateList)
                |> Async.Parallel

            return ()
        finally
            semaphore.Release() |> ignore
    }

let parallelFind
    (directory: string)
    (pattern: string)
    (maxParallelism: int)
    (ct: CancellationToken)
    (updateList: string -> unit)
    =
    async {
        let semaphore = new SemaphoreSlim(maxParallelism)
        let subDirs = Directory.EnumerateDirectories(directory)

        let! _ = findFilesAsync directory pattern semaphore ct updateList

        let! _ =
            subDirs
            |> Seq.map (fun subDir -> findFilesAsync subDir pattern semaphore ct updateList)
            |> Async.Parallel

        return ()
    }
