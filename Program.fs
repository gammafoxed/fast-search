﻿module fast_search.Program

open FindAsync

[<EntryPoint>]
let main argv =
    let mailBox =
        MailboxProcessor.Start(fun inbox ->
            async {
                while true do
                    let! res = inbox.Receive()

                    match res with
                    | Ok ok -> printfn $"[FILE]: {ok}"
                    | Error err -> printfn $"[ERROR]: {err}"
            })

    search ("C:/", "*pdf", 4, mailBox) |> Async.RunSynchronously
    0
