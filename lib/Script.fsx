#I "/Users/hussam/projects/q/lib/bin/Debug/"
#r "Xamarin.iOS.dll"
#r "SQLite.dll"
#load "DB.fs"

open qlib

#load "QLib.fs"

open qlib

// NOTE: This project uses a local copy of Xamarin.iOS.dll in order to enable Script testing.
// I don't know if this is good or bad. Consider removing local copy.


do QLib.Init "/Users/hussam/tmp/" |> ignore