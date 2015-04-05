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

    let Topics = FSharpType.GetUnionCases typeof<Topic> |> Array.map (fun t -> t.Name)


    let morning = TimeSpan.FromHours(7.0)
    let noon = TimeSpan.FromHours(12.0)
    let afternoon = TimeSpan.FromHours(15.0)
    let evening = TimeSpan.FromHours(19.0)
    let night = TimeSpan.FromHours(22.0)

    let defaultTime = function
        | Coffee -> [ morning; afternoon ]
        | Beer -> [ evening ]
        | Lunch -> [ noon ]
        | Brunch -> [ morning; noon ]
        | Dinner -> [ evening ]
        | Drinks -> [ evening; night ]

    type Bucket = Now | Soon | SomeTime

    let Buckets = FSharpType.GetUnionCases typeof<Bucket> |> Array.map (fun b -> b.Name)

    let bucket topic time =
        let nearestTime =
            defaultTime topic
            |> List.map (fun t -> t.Subtract(time).Duration())
            |> List.min

        if nearestTime.Hours <= 2 then Now
        elif nearestTime.Hours <= 5 then Soon
        else SomeTime

    let sortToBuckets list =
        let timeNow = DateTime.Now.TimeOfDay
        let (now, soon, sometime) = list |> List.map (fun (item : QItem) ->
                                            match strToDu item.Topic with
                                            | Some topic -> Some (topic, item)
                                            | None -> None)
                                         |> List.fold (fun ((now, soon, sometime) as acc) qiOpt ->
                                            match qiOpt with
                                            | None -> acc
                                            | Some (topic, item) ->
                                                match bucket topic timeNow with
                                                | Now -> ((item :: now) , soon, sometime)
                                                | Soon -> (now, (item :: soon), sometime)
                                                | SomeTime -> (now, soon, (item :: sometime))
                                            ) ([], [], [])
        ((Now, now) , (Soon, soon), (SomeTime, sometime))

    let bucketChooser = function
        | Now -> fun (now, _, _) -> now
        | Soon -> fun (_, soon, _) -> soon
        | SomeTime -> fun (_, _, sometime) -> sometime

