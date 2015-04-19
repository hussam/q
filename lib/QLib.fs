namespace qlib

open System
open System.IO
open System.Collections.ObjectModel

exception QLibNotInitialized

type QLib private () =
    static let mutable qdb : QItemDB Option = None
    static let queues = Array.init 2 (fun _ -> null)

    static let mutable isLoaded = false

    static let loadData() =
        match isLoaded with
        | true -> ()
        | false ->
            match qdb with
            | None -> raise QLibNotInitialized
            | Some db ->
                let items = db.GetItems().Result    // this will block until items are loaded
                let uncompleted, completed = items |> List.ofSeq |> List.partition (fun i -> i.Completed = false)

                queues.[0] <- new ObservableCollection<_>(uncompleted)
                queues.[1] <- new ObservableCollection<_>(completed)
                isLoaded <- true

    static member Init(dbPath) =
        let db = new QItemDB(Path.Combine(dbPath, "q.sqlite"))
        db.CreateTable().Wait()
        qdb <- Some db

    static member Topics = QLogic.Topics
    static member QueueNames = [| "" ; "Completed" |]
    static member AllQueues = (loadData() ; queues)

    static member Uncompleted = 0
    static member Completed = 1

    static member SaveItem item =
        loadData()
        match qdb with
        | None -> ()
        | Some db ->
            db.SaveItem(item) |> ignore
            queues.[0].Add(item)

    static member MarkItemAsCompleted item =
        loadData()
        match qdb with
        | None -> ()
        | Some db ->
            queues.[0].Remove(item) |> ignore
            item.Completed <- true
            db.UpdateItem(item) |> ignore
            queues.[1].Add(item) |> ignore

    static member MarkItemAsUncompleted item =
        loadData()
        match qdb with
        | None -> ()
        | Some db ->
            queues.[1].Remove(item) |> ignore
            item.Completed <- false
            db.UpdateItem(item) |> ignore
            queues.[0].Add(item) |> ignore


    #if false
    static member ScheduleItemForToday item =
        loadData()
        match qdb with
        | None -> ()
        | Some db ->
            match QScheduling.schedule item with
            | None -> ()
            | Some dateTime -> 
                item.Schedule <- dateTime
                queues |> Array.iter (fun q -> q.Remove(item) |> ignore)
                queues.[0].Add(item)
                db.UpdateItem(item) |> ignore

    static member UnscheduleItem item =
        loadData()
        match qdb with
        | None -> ()
        | Some db ->
            queues.[0].Remove(item) |> ignore
            queues.[3].Add(item)
            item.Schedule <- new DateTime(1,1,1)
            db.UpdateItem(item) |> ignore

    static member DeleteItem item =
        loadData()
        match qdb with
        | None -> ()
        | Some db ->
            queues |> Array.iter (fun q -> q.Remove(item) |> ignore)
            db.DeleteItem(item) |> ignore
    #endif