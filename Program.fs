module fast_search.Program

open System
open FindAsync

[<EntryPoint>]
let main argv =
    let rootDir = "C:\scan"
    let mask = "*.pdf"
    
    let mailBox = MailboxProcessor.Start(fun inbox ->
        async{
            while true do
                let! res = inbox.Receive()
                match res with
                | Ok ok -> printfn $"[FILE]: {ok}"
                | Error err -> printfn $"[ERROR]: {err}"
        })
    
    search(rootDir, mask, 4,  mailBox) |> Async.RunSynchronously
    Console.ReadLine() |> ignore
    0