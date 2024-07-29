module fast_search.Program

open System
open System.Collections.Concurrent
open System.IO
open System.Threading

let searchFilesAsync (rootDir: string) (pattern: string) (maxDegreeOfParallelism: int) (cancellationToken: CancellationToken) =
    let files = ConcurrentBag<string>()
    let dirsToProcess = ConcurrentQueue<string>()
    let semaphore = new SemaphoreSlim(maxDegreeOfParallelism, maxDegreeOfParallelism)

    // Добавляем корневую директорию в очередь
    dirsToProcess.Enqueue(rootDir)

    let rec searchDirAsync (dir: string) = async {
        try
            try
                // Добавляем найденные файлы в список
                let fileEntries = Directory.GetFiles(dir, pattern)
                for file in fileEntries do
                    files.Add(file)

                // Добавляем поддиректории в очередь
                let subDirs = Directory.GetDirectories(dir)
                for subDir in subDirs do
                    dirsToProcess.Enqueue(subDir)
            with
            | :? UnauthorizedAccessException -> ()
            | :? DirectoryNotFoundException -> ()
        finally
            semaphore.Release() |> ignore
    }

    let processQueueAsync () = async {
        while not cancellationToken.IsCancellationRequested && not dirsToProcess.IsEmpty do
            let mutable dir = null
            if dirsToProcess.TryDequeue(&dir) then
                do! semaphore.WaitAsync(cancellationToken) |> Async.AwaitTask
                Async.Start(searchDirAsync dir, cancellationToken)
    }

    async {
        let queueProcessor = processQueueAsync ()
        do! Async.Ignore(queueProcessor)
        // Ожидание завершения всех задач
        while semaphore.CurrentCount <> maxDegreeOfParallelism do
            do! Async.Sleep(100)
        return files |> Seq.toList
    }
    
[<EntryPoint>]
let main argv =
    let cts = new CancellationTokenSource()
    let token = cts.Token
    let rootDir = @"/var/home/aleksei/"
    let pattern = "*.fs"
    let maxDegreeOfParallelism = 10

    let task = searchFilesAsync rootDir pattern maxDegreeOfParallelism token
    let result = Async.RunSynchronously task
    
    Console.WriteLine result
    0
