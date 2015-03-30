#I "/Users/hussam/projects/q/lib/bin/Debug/"
#r "Xamarin.iOS.dll"
#r "SQLite.dll"
#load "DB.fs"

// NOTE: This project uses a local copy of Xamarin.iOS.dll in order to enable Script testing.
// I don't know if this is good or bad. Consider removing local copy.


open qlib
open System


let db = new QItemDB("/Users/hussam/tmp/testing.sqlite")

do db.CreateTable() |> ignore

let mutable q = new QItem()
db.SaveItem(q) |> ignore

q.Text <- "Some activitiy"
q.Hashtag <- "Testing"
db.SaveItem(q)

let x = nfloat 32.0f


