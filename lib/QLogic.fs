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


    let morning = TimeSpan.FromHours(8.0)
    let noon = TimeSpan.FromHours(13.0)
    let afternoon = TimeSpan.FromHours(16.0)
    let evening = TimeSpan.FromHours(20.0)
    let night = TimeSpan.FromHours(23.0)

    let defaultTime = function
        | Coffee -> [ morning; afternoon ]
        | Beer -> [ evening ]
        | Lunch -> [ noon ]
        | Brunch -> [ morning; noon ]
        | Dinner -> [ evening ]
        | Drinks -> [ evening; night ]
        | Other -> [ evening ]  // TODO: FIXME: This is just a stopgap

    type Bucket = Scheduled | Now | Soon | SomeTime

    let Buckets = FSharpType.GetUnionCases typeof<Bucket> |> Array.map (fun b -> b.Name)

    let bucket topic time =
        if topic = Other then SomeTime
        else
            let nextDay = TimeSpan.FromDays(1.0)
            let nearestTime =
                (defaultTime topic)
                |> List.map (fun t -> let diff = t.Subtract(time) in if diff > TimeSpan.Zero then diff else diff.Add(nextDay))
                |> List.filter (fun t -> t > TimeSpan.Zero)
                |> List.min

            if nearestTime.Hours <= 1 then Now
            elif nearestTime.Hours <= 3 then Soon
            else SomeTime

    let sortToBuckets list =
        let timeNow = DateTime.Now.TimeOfDay
        let (scheduled, now, soon, sometime) = list |> List.map (fun (item : QItem) ->
                                                        match strToDu item.Topic with
                                                        | Some topic -> Some (topic, item)
                                                        | None -> None)
                                                    |> List.fold (fun ((scheduled, now, soon, sometime) as acc) qiOpt ->
                                                        match qiOpt with
                                                        | None -> acc
                                                        | Some (topic, item) ->
                                                            if item.Schedule.Date = DateTime.Today
                                                            then ((item :: scheduled), now, soon, sometime)
                                                            else
                                                                match bucket topic timeNow with
                                                                | Now -> (scheduled, (item :: now) , soon, sometime)
                                                                | Soon -> (scheduled, now, (item :: soon), sometime)
                                                                | SomeTime -> (scheduled, now, soon, (item :: sometime))
                                                                | Scheduled -> raise ShouldNotGetHere
                                                        ) ([], [], [], [])
        ((Scheduled, scheduled) , (Now, now) , (Soon, soon), (SomeTime, sometime))

    let schedule (item : QItem) =
        match strToDu item.Topic with
        | None -> None
        | Some topic ->
            let timeNow = DateTime.Now.TimeOfDay
            let defaultTimes = defaultTime topic
            let time = match defaultTimes |> List.filter (fun t -> t > timeNow) with
                       | time :: _ -> time
                       | [] -> List.nth defaultTimes (defaultTimes.Length - 1)
            Some (DateTime.Today.Add(time))
