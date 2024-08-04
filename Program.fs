module fast_search.Program

open System
open FindAsync

[<EntryPoint>]
let main argv =
    let mailBox = MailboxProcessor.Start(fun inbox ->
        async {
            while true do
                let! res = inbox.Receive()
                match res with
                | Ok ok -> printfn $"[FILE]: {ok}"
                | Error err -> printfn $"[ERROR]: {err}"
        })
    
    Form.setOnClickAction( fun path mask ->
        search(path, mask, 4,  mailBox) |> Async.RunSynchronously
        Form.startInfiniteProgressBar()
        )
    Form.runForm()
    0