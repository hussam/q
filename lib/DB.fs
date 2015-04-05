namespace qlib

open System
open System.Threading.Tasks
open System.Collections.Generic
open System.Collections.ObjectModel
open SQLite

open Utils

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

    member this.CreateTable() =
        lock locker (fun() -> this.CreateTableAsync<QItem>()) |> Async.AwaitTask |> Async.RunSynchronously

    member this.SaveItem(item : QItem) =
        lock locker (fun() -> this.InsertAsync(item))

    member this.GetItems() =
        (lock locker (fun() -> this.Table<QItem>())).ToListAsync() |> Async.AwaitTask