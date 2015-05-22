namespace qlib

open System
open System.IO
open System.Collections.Generic
open System.Collections.ObjectModel

exception QLibNotInitialized

type QueueType = AllTasks | TodayAgenda | Completed | Uncompleted

type QLib private () =
    static let mutable qdb : QItemDB Option = None
    static let queues = new Dictionary<_,_>(HashIdentity.Structural)

    static let mutable isLoaded = false

    static let loadData() =
        match isLoaded with
        | true -> ()
        | false ->
            match qdb with
            | None -> raise QLibNotInitialized
            | Some db ->
                let allTasks = db.GetItems().Result |> List.ofSeq    // this will block until items are loaded
                let uncompleted, completed = allTasks |> List.partition (fun i -> i.Completed = false)

                queues.Add(Uncompleted, new ObservableCollection<_>(uncompleted))
                queues.Add(Completed, new ObservableCollection<_>(completed))
                queues.Add(TodayAgenda, new ObservableCollection<_>())

                isLoaded <- true

    static member Init(dbPath) =
        //Backend.Init()
        let db = new QItemDB(Path.Combine(dbPath, "q.sqlite"))
        db.CreateTable().Wait()
        qdb <- Some db

    static member QueueNames queueType =
        match queueType with
        | AllTasks -> [| "Uncompleted" ; "Completed" |]
        | _ -> [| "" |]

    static member GetTasks(queueType) =
        loadData()
        match queueType with
        | AllTasks -> [| queues.[Uncompleted] ; queues.[Completed] |]
        | _ -> [| queues.[queueType] |]

    static member SaveItem item =
        loadData()
        match qdb with
        | None -> ()
        | Some db ->
            db.SaveItem(item) |> ignore
            //Backend.SaveItem(item) |> ignore
            queues.[Uncompleted].Add(item)

    static member MarkItemAsCompleted item =
        loadData()
        match qdb with
        | None -> ()
        | Some db ->
            queues.[Completed].Add(item)
            queues.[Uncompleted].Remove(item) |> ignore
            item.Completed <- true
            item.CompletedOn <- DateTime.UtcNow
            db.UpdateItem(item) |> ignore

    static member MarkItemAsUncompleted item =
        loadData()
        match qdb with
        | None -> ()
        | Some db ->
            queues.[Completed].Remove(item) |> ignore
            queues.[Uncompleted].Add(item)
            item.Completed <- false
            db.UpdateItem(item) |> ignore

    static member DeleteItem item =
        loadData()
        match qdb with
        | None -> ()
        | Some db ->
            for queue in queues.Values do
                queue.Remove(item) |> ignore
            db.DeleteItem(item) |> ignore

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
    #endif