#I "/Users/hussam/projects/q/lib/bin/Debug/"
#r "SQLite.dll"
#load "DB.fs"

open qlib

let db = new QItemDB("/Users/hussam/tmp/testing.sqlite")

do db.CreateTable() |> ignore

let mutable q = new QItem()
db.SaveItem(q) |> ignore

q.Text <- "Some activitiy"
q.Hashtag <- "Testing"
db.SaveItem(q)
