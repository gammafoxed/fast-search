module fast_search.Program

open FindAsync

[<EntryPoint>]
let main argv =
    let rootDir = "C:/scan"
    let mask = "*.pdf"
    
    let mailBox = MailboxProcessor.Start(fun inbox ->
        async{
            while true do
                let! res = inbox.Receive()
                match res with
                | Ok ok -> printf $"[FILE]: {ok}"
                | Error err -> printf $"[ERROR]: {err}"
        })
    
    search(rootDir, mask, 4,  mailBox) |> Async.RunSynchronously
        
    0