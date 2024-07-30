module fast_search.Program

open System
open System.IO
open System.Threading.Tasks
open System.Collections.Concurrent

let maxDegreeOfParallelism = 4  // Максимальное количество параллельных задач
let memoryLimit = 1000000000L    // Ограничение по памяти в байтах (1 ГБ)

type MemoryLimitExceededException() = inherit Exception("Memory limit exceeded")

let searchFiles (rootDirectory: string) (fileMask: string) =
    let results = ConcurrentBag<string>()

    let rec search directory =
        let memoryUsage = System.Diagnostics.Process.GetCurrentProcess().PrivateMemorySize64
        if memoryUsage > memoryLimit then
            raise (MemoryLimitExceededException())

        try
            let files = Directory.GetFiles(directory, fileMask)
            for file in files do
                results.Add(file)
            
            let directories = Directory.GetDirectories(directory)
            let tasks = directories |> Array.map (fun dir -> Task.Factory.StartNew(fun () -> search dir) :> Task)
            Task.WaitAll(tasks)
        with
        | :? UnauthorizedAccessException -> ()
        | :? PathTooLongException -> ()
        | :? MemoryLimitExceededException -> raise (MemoryLimitExceededException())

    try
        let rootDirectories = Directory.GetDirectories(rootDirectory)
        let rootTasks = rootDirectories |> Array.map (fun dir -> Task.Factory.StartNew(fun () -> search dir) :> Task)
        Task.WaitAll(rootTasks)
    with
    | :? MemoryLimitExceededException ->
        printfn "Memory limit exceeded during file search."

    results
    
    
[<EntryPoint>]
let main argv =
    let rootDir = "//Pack2008/work"
    let mask = "*773*.pdf"
    let foundFiles = searchFiles rootDir mask

    for file in foundFiles do
        printfn $"%s{file}"

    0