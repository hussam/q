namespace qlib

open System
open Microsoft.FSharp.Reflection

open Utils

module QLogic =

    type Topic =
        | Coffee
        | Beer
        | Drinks
        | Lunch
        | Brunch
        | Dinner
        | Other

    let Topics = FSharpType.GetUnionCases typeof<Topic> |> Array.map (fun t -> t.Name)



