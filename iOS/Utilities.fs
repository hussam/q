module Utilites

open UIKit

type UIColor with
    static member Aqua = UIColor.FromRGB(0, 255, 255)
    static member Fuchsia = UIColor.FromRGB(255, 0, 255)
    static member Lime = UIColor.FromRGB(0, 255, 0)
    static member Maroon = UIColor.FromRGB(128, 0, 0)
    static member Navy = UIColor.FromRGB(0, 0, 128)
    static member Olive = UIColor.FromRGB(128, 128, 0)
    static member Pink = UIColor.FromRGB(255, 102, 255)
    static member Teal = UIColor.FromRGB(0, 128, 128)

    static member MidGreen = UIColor.FromRGB(0, 128, 0)
    static member MidPurple = UIColor.FromRGB(128, 0, 128)


    static member QBlue = UIColor.FromRGB(0, 173, 238)
    static member QYellow = UIColor.FromRGB(247, 223, 47)
    static member QSalmon = UIColor.FromRGB(239, 95, 55)
    static member QMagenta = UIColor.FromRGB(237, 42, 123)
    static member QGreen = UIColor.FromRGB(139, 197, 63)
    static member QLime = UIColor.FromRGB(214, 223, 35)

    member this.WithBrightness brightness =
        let h, s, b, a = this.GetHSBA ()
        UIColor.FromHSBA(h, s, brightness, a)

    member this.WithSaturation saturation = 
        let h, s, b, a = this.GetHSBA ()
        UIColor.FromHSBA(h, saturation, b, a)


let QColors = [| UIColor.QBlue ; UIColor.QYellow ; UIColor.QSalmon ; UIColor.QMagenta ; UIColor.QGreen ; UIColor.QLime |]