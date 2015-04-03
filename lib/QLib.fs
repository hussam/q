namespace qlib

open System.IO
open System.Collections.ObjectModel

module QLib =
    let mutable private qdb = None

    let Init dbPath = 
        let db = new QItemDB(Path.Combine(dbPath, "q.sqlite"))
        db.CreateTable () |> ignore
        qdb <- Some db
        true

    let SaveItem item =
        match qdb with
        | Some db ->
            db.SaveItem(item) |> ignore
            true
        | None -> false

    let GetQueueView () =
        match qdb with
        | Some db -> db.FetchItems() |> Async.RunSynchronously
        | None -> ObservableCollection<QItem>()

    let Topics = [| "Coffee"; "Beer"; "Drinks"; "Brunch"; "Lunch"; "Dinner"; "Movie"; "Walk"; |]
