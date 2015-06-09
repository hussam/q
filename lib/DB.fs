namespace qlib

open System
open System.Threading.Tasks
open System.Collections.Generic
open SQLite

open Utils

[<AllowNullLiteral>]
type DBItem() = 
    [<PrimaryKey; AutoIncrement>]
    member val LocalId :int = 0 with get, set
    member val Id : string = null with get, set

[<AllowNullLiteral>]
type QItem() =
    inherit DBItem()
    member val Text : string = null with get, set
    member val Completed : bool = false with get, set
    member val CreatedOn : DateTime = DateTime.UtcNow with get, set
    member val CompletedOn : DateTime = DateTime.MinValue with get, set
    member val Notes : string = null with get, set


type QItemDB(path) =
    inherit SQLiteAsyncConnection(path)

    let locker : obj = new obj()

    member this.CreateTable() =
        lock locker (fun() -> this.CreateTableAsync<QItem>())

    member this.SaveItem(item : QItem) =
        lock locker (fun() -> this.InsertAsync(item))

    member this.GetItems() =
        (lock locker (fun() -> this.Table<QItem>())).ToListAsync()

    member this.UpdateItem(item : QItem) =
        lock locker (fun() -> this.UpdateAsync(item))

    member this.DeleteItem(item : QItem) =
        lock locker (fun() -> this.DeleteAsync(item))