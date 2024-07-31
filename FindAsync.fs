module fast_search.FindAsync

open System
open System.IO
open System.Threading.Tasks
open System.Collections.Concurrent
open FSharpx.Control

module private domain =
    let mutable maxDegreeOfParallelism = 4  // Maximum number of parallel tasks
    let mutable memoryLimit = 1000000000L   // Memory limit in bytes (1 GB)

    // Custom exception for exceeding memory limit
    type MemoryLimitExceededException() = inherit Exception("Memory limit exceeded")

    // Function to check if a file or directory is a symbolic link
    let isSymbolicLink (path: string) =
        let attributes = File.GetAttributes(path)
        attributes.HasFlag(FileAttributes.ReparsePoint)

    // Function to search files in the given directory and its subdirectories
    let searchFiles (rootDirectory: string) (fileMask: string) : AsyncSeq<Result<string, string>> =
        let results = ConcurrentQueue<Result<string, string>>()

        let rec search directory =
            // Check current memory usage and raise an exception if it exceeds the limit
            let memoryUsage = System.Diagnostics.Process.GetCurrentProcess().PrivateMemorySize64
            if memoryUsage > memoryLimit then
                raise (MemoryLimitExceededException())

            try
                // Add matching files in the current directory to the results
                let files = Directory.GetFiles(directory, fileMask)
                for file in files do
                    if not (isSymbolicLink file) then  // Ignore symbolic links
                        results.Enqueue(Result.Ok(file))
                
                // Recursively search in subdirectories
                let directories = Directory.GetDirectories(directory)
                Parallel.ForEach(directories,
                                 ParallelOptions(MaxDegreeOfParallelism = maxDegreeOfParallelism),
                                 (fun dir -> if not (isSymbolicLink dir) then search dir)) |> ignore
            with
            | :? UnauthorizedAccessException -> results.Enqueue(Result.Error "Unauthorised acces exception")
            | :? PathTooLongException -> results.Enqueue(Result.Error "Path too long exception")
            | :? MemoryLimitExceededException -> raise (MemoryLimitExceededException())  // Re-throw memory limit exception
            | ex -> results.Enqueue (Result.Error $"Exception: %s{ex.Message}")  // Log other exceptions

        try
            // Start the search in the root directories
            let rootDirectories = Directory.GetDirectories(rootDirectory)
            Parallel.ForEach(rootDirectories,
                             ParallelOptions(MaxDegreeOfParallelism = maxDegreeOfParallelism),
                             (fun dir -> if not (isSymbolicLink dir) then search dir)) |> ignore
        with
        | :? MemoryLimitExceededException -> results.Enqueue(Result.Error "Memory limit exceeded")
        | ex -> results.Enqueue(Result.Error $"Exception: %s{ex.Message}")

        AsyncSeq.ofSeq results  // Return the async sequence of found files


open domain
let startSearch (maxDegreeOfParallelism, memoryLimit) (rootDirectory: string) (fileMask: string) =
    domain.memoryLimit <- memoryLimit
    domain.maxDegreeOfParallelism <- maxDegreeOfParallelism
    searchFiles rootDirectory fileMask