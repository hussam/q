namespace qlib

open System
open System.Collections.Generic

open Utils

module QLogic =

    type ActivityTopic =
        | Breakfast
        | Lunch
        | Brunch
        | Dinner
        | Coffee
        | Tea
        | Beer
        | Drinks

    type Topic =
        | Activity of ActivityTopic
        | Other of string

    let Topics =
        let dict = new Dictionary<_,_>()
        dict.Add("Activity", unionCasesToStrArray typeof<ActivityTopic>)
        dict.Add("Follow-up", [| "Contact" |])
        dict.Add("Other", [| "Other" |])
        dict




