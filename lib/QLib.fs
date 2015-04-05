namespace qlib

open System
open System.IO
open System.Collections.ObjectModel

module QLib =
    let mutable private qdb = None
    let mutable private queues = None

    let private sometime (qs) =
        match qs with
        | None -> None
        | Some (_, _, sometimeQueue) -> Some sometimeQueue



    let Topics = QLogic.Topics
    let Buckets = QLogic.Buckets

    let Init dbPath = 
        let db = new QItemDB(Path.Combine(dbPath, "q.sqlite"))
        db.CreateTable () |> ignore
        qdb <- Some db
        true

    let GetAllQueues() =
        match qdb with
        | None -> Array.empty
        | Some db ->
            let items = db.GetItems() |> Async.RunSynchronously
            let (_, now) , (_, soon),  (_, someTime) = QLogic.sortToBuckets (List.ofSeq items)
            let n = new ObservableCollection<QItem>(List.toSeq(now))
            let s = new ObservableCollection<QItem>(List.toSeq(soon))
            let st = new ObservableCollection<QItem>(List.toSeq(someTime))
            queues <- Some (n, s, st)
            [| n ; s ; st |]

    let SaveItem item =
        match qdb with
        | None -> false
        | Some db ->
            db.SaveItem(item) |> ignore
            match sometime(queues) with
            | None -> false
            | Some queue -> queue.Add(item); true