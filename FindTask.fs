module fast_search.FindTask

open System
open System.IO
open System.Threading.Tasks
open System.Collections.Concurrent

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
    let searchFiles (rootDirectory: string) (fileMask: string) =
        let results = ConcurrentBag<string>()  // Thread-safe collection to store found files

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
                        results.Add(file)
                
                // Recursively search in subdirectories
                let directories = Directory.GetDirectories(directory)
                Parallel.ForEach(directories,
                                 new ParallelOptions(MaxDegreeOfParallelism = maxDegreeOfParallelism),
                                 (fun dir -> if not (isSymbolicLink dir) then search dir)) |> ignore
            with
            | :? UnauthorizedAccessException -> ()  // Ignore directories we don't have access to
            | :? PathTooLongException -> ()         // Ignore directories with too long paths
            | :? MemoryLimitExceededException -> raise (MemoryLimitExceededException())  // Re-throw memory limit exception
            | ex -> printfn $"Exception: %s{ex.Message}"  // Log other exceptions

        try
            // Start the search in the root directories
            (let rootDirectories = Directory.GetDirectories(rootDirectory)
             Parallel.ForEach(rootDirectories,
                              new ParallelOptions(MaxDegreeOfParallelism = maxDegreeOfParallelism),
                              (fun dir -> if not (isSymbolicLink dir) then search dir))) |> ignore
        with
        | :? MemoryLimitExceededException -> printfn "Memory limit exceeded during file search."
        | ex -> printfn $"Exception: %s{ex.Message}"  // Log other exceptions

        results  // Return the collection of found files

open domain
let startSearch (maxDegreeOfParallelism, memoryLimit) (rootDirectory: string) (fileMask: string) =
    domain.memoryLimit <- memoryLimit
    domain.maxDegreeOfParallelism <- maxDegreeOfParallelism
    searchFiles rootDirectory fileMask