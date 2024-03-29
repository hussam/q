﻿namespace qlib

open System
open Microsoft.FSharp.Reflection

module Utils =
    exception ShouldNotGetHere

    let asyncMap f workflow = async {
        let! res = workflow
        return f res
    }

    let duToStr (x : 'a) = 
        match FSharpValue.GetUnionFields(x, typeof<'a>) with
        | case, _ -> case.Name

    let strToDu<'a> (x:string) =
        match FSharpType.GetUnionCases typeof<'a> |> Array.filter (fun case -> case.Name.ToLower() = x.ToLower()) with
        |[|case|] -> Some(FSharpValue.MakeUnion(case,[||]) :?> 'a)
        |_ -> None

    let unionCasesToStrArray (unionType : Type) =
        FSharpType.GetUnionCases unionType |> Array.map (fun t -> t.Name)