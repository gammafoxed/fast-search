module fast_search.FindAsync

open System
open System.IO
open Microsoft.FSharp.Control
open Microsoft.FSharp.Core

module private domain =
    let isSymbolicLink (path: string) =
        let attributes = File.GetAttributes(path)
        attributes.HasFlag(FileAttributes.ReparsePoint)

    let isNotSymbolLink path = not (isSymbolicLink path)

    let rec search
        (
            directory: string,
            fileMask: string,
            maxDegreeOfParallelism: int,
            mailBox: MailboxProcessor<Result<string, string>>
        ) =
        async {
            try
                let files = Directory.GetFiles(directory, fileMask) |> Array.filter isNotSymbolLink

                for file in files do
                    Ok file |> mailBox.Post

                let seqAsync =
                    Directory.GetDirectories(directory)
                    |> Array.filter isNotSymbolLink
                    |> Array.map (fun dir -> search (dir, fileMask, maxDegreeOfParallelism, mailBox))
                    |> Array.toSeq

                Async.Parallel(seqAsync, maxDegreeOfParallelism) |> Async.StartAsTask |> ignore
            with
            | :? UnauthorizedAccessException -> Result.Error "Unauthorised acces exception" |> mailBox.Post
            | :? PathTooLongException -> Result.Error "Path too long exception" |> mailBox.Post
            | ex -> Result.Error $"Exception: %s{ex.Message}" |> mailBox.Post
        }


let search = domain.search

let searchCSharpWrapper directory fileMask maxDegreeOfParallelism mailBox =
    domain.search (directory, fileMask, maxDegreeOfParallelism, mailBox)
