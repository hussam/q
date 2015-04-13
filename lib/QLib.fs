namespace qlib

open System
open System.IO
open System.Collections.ObjectModel

exception QLibNotInitialized

type QLib private () =
    static let mutable qdb : QItemDB Option = None
    static let queues = Array.init 3 (fun _ -> null)
    static let agenda = Array.init 1 (fun _ -> null)

    static let mutable isLoaded = false

    static let loadData() =
        match isLoaded with
        | true -> ()
        | false ->
            match qdb with
            | None -> raise QLibNotInitialized
            | Some db ->
                let items = db.GetItems().Result    // this will block until items are loaded

                let (_, now) , (_, soon),  (_, someTime) = QLogic.sortToBuckets (List.ofSeq items)
                queues.[0] <- new ObservableCollection<_>(now)
                queues.[1] <- new ObservableCollection<_>(soon)
                queues.[2] <- new ObservableCollection<_>(someTime)

                // Add items to daily agenda
                let today = DateTime.Today
                let agendaItems = items
                                    |> List.ofSeq
                                    |> List.filter (fun qi -> qi.Schedule.Date = today)
                agenda.[0] <- new ObservableCollection<_>(agendaItems)

                isLoaded <- true

    static member Init(dbPath) =
        let db = new QItemDB(Path.Combine(dbPath, "q.sqlite"))
        db.CreateTable().Wait()
        qdb <- Some db

    static member Topics = QLogic.Topics
    static member Buckets = QLogic.Buckets
    static member AllQueues = (loadData() ; queues)
    static member TodaysAgenda = (loadData() ; agenda)

    static member SaveItem item =
        loadData()
        match qdb with
        | None -> ()
        | Some db ->
            db.SaveItem(item) |> ignore
            queues.[2].Add(item)

    static member ScheduleItemForToday item =
        loadData()
        match qdb with
        | None -> ()
        | Some db ->
            match QLogic.schedule item with
            | None -> ()
            | Some dateTime -> 
                item.Schedule <- dateTime
                agenda.[0].Add(item)
                db.UpdateItem(item) |> ignore

                queues |> Array.iter (fun q -> q.Remove(item) |> ignore)