namespace qlib

open System
open System.Threading.Tasks
open System.Collections.Generic
open System.Collections.ObjectModel
open SQLite

type DBItem() = 
    [<PrimaryKey; AutoIncrement>]
    member val LocalId :int = 0 with get, set

type QItem() =
    inherit DBItem()
    member val Text : string = null with get, set
    member val Topic : string = null with get, set

[<AllowNullLiteral>]
type QItemDB(path) =
    inherit SQLiteAsyncConnection(path)

    let locker : obj = new obj()
    let mutable qitems = new ObservableCollection<QItem>(new List<QItem>())

    member this.CreateTable() =
        lock locker (fun() -> this.CreateTableAsync<QItem>()) |> Async.AwaitTask |> Async.RunSynchronously

    member this.FetchItems() =
        let table = lock locker ( fun() -> this.Table<QItem>() )
        table.ToListAsync() |> Async.AwaitTask |> Async.Map (fun list ->
            qitems <- new ObservableCollection<QItem>(list)
            qitems
        )

    member this.SaveItem(item : QItem) =
        qitems.Add(item)
        lock locker (fun() -> this.InsertAsync(item))