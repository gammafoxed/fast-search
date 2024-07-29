module fast_search.Program

open System
open System.Collections.Concurrent
open System.IO
open System.Threading


module AsyncExtensions =
    open Microsoft.FSharp.Control
    let ParallelThrottled<'T> (maxDegreeOfParallelism: int) (jobs: seq<Async<'T>>) : Async<'T[]> =
        async {
            let semaphore = new SemaphoreSlim(maxDegreeOfParallelism, maxDegreeOfParallelism)
            let resultBag = ConcurrentBag<'T>()

            let runJob job = async {
                do! semaphore.WaitAsync() |> Async.AwaitTask
                try
                    let! result = job
                    resultBag.Add(result)
                finally
                    semaphore.Release() |> ignore
            }

            let! tasks = 
                jobs 
                |> Seq.map runJob 
                |> Async.Parallel
            
            return resultBag.ToArray()
        }

let searchFilesAsync (rootDir: string) (pattern: string) (maxDegreeOfParallelism: int) (cancellationToken: CancellationToken) =
    let files = ConcurrentBag<string>()
    
    let rec searchDirAsync (dir: string) = async {
        try
            // Добавляем найденные файлы в список
            let fileEntries = Directory.GetFiles(dir, pattern)
            for file in fileEntries do
                files.Add(file)

            // Рекурсивно ищем в поддиректориях
            let subDirs = Directory.GetDirectories(dir)
            let subDirJobs = 
                subDirs 
                |> Array.map (fun subDir -> async {
                    do! searchDirAsync subDir
                })
            let! _ = AsyncExtensions.ParallelThrottled maxDegreeOfParallelism subDirJobs
            return ()
        with
        | :? UnauthorizedAccessException -> return ()
        | :? DirectoryNotFoundException -> return ()
    }

    async {
        try
            do! searchDirAsync rootDir
        with
        | :? OperationCanceledException -> ()
        return files |> Seq.toList
    }


[<EntryPoint>]
let main argv =
    let cts = new CancellationTokenSource()
    let token = cts.Token
    let rootDir = @"/var/home/aleksei"
    let pattern = "*.fs"
    let maxDegreeOfParallelism = 4

    let task = searchFilesAsync rootDir pattern maxDegreeOfParallelism token
    let result = Async.RunSynchronously task
    
    Console.WriteLine result
    0
