namespace qlib

open System.IO

module QLib =
    let mutable private qdb = None

    let Init dbPath = 
        let db = new QItemDB(Path.Combine(dbPath, "q.sqlite"))
        db.CreateTable () |> Async.RunSynchronously |> ignore
        qdb <- Some db
        true

    let SaveItem item =
        match qdb with
        | Some db ->
            db.SaveItem(item) |> Async.RunSynchronously |> ignore
            true
        | None -> false