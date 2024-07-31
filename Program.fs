module fast_search.Program

open FSharpx.Control

let findTask rootDir mask =
    let foundFiles = FindTask.startSearch (4, 1000000000L) rootDir mask
    // Print the found files
    for file in foundFiles do
        printfn $"%s{file}"

let findAsync rootDir mask =
    let foundFiles = FindAsync.startSearch (4, 1000000000L) rootDir mask

    // Asynchronously print the search results
    foundFiles
    |> AsyncSeq.iterAsync (fun result -> async {
        match result with
        | Result.Ok file -> printfn $"Found: %s{file}"
        | Result.Error ex -> printfn $"Error: %s{ex}"
    })
    |> Async.RunSynchronously

// Entry point of the program
[<EntryPoint>]
let main argv =
    let rootDir = "//Pack2008/work"  // Root directory to start the search
    let mask = "*.pdf"           // File mask to search for
    
    findAsync rootDir mask

    0  // Return 0 to indicate successful execution