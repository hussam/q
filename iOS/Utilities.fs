module Utilites

open System
open UIKit
open CoreGraphics

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
        let h, s, _, a = this.GetHSBA ()
        UIColor.FromHSBA(h, s, brightness, a)

    member this.WithHSBSaturation saturation = 
        let h, _s, b, a = this.GetHSBA ()
        UIColor.FromHSBA(h, saturation, b, a)

    member this.GetHSLA () =
        let h, s_hsb, b, a = this.GetHSBA()
        let l = (nfloat 0.5) * b * (nfloat 2.0 - s_hsb)
        let s_hsl = b * s_hsb / nfloat (1.0 - Math.Abs((2.0 * (float l)) - 1.0))
        (h, s_hsl, l, a)

    static member FromHSLA (hue, saturation, lightness, alpha) =
        let b = nfloat 2.0 * saturation * nfloat (1.0 - Math.Abs((2.0 * (float lightness)) - 1.0))
        let s_hsb = (nfloat 2.0) * (b - lightness) / b
        UIColor.FromHSBA(hue, s_hsb, b, alpha)

    member this.WithLightness lightness =
        let h, s, _, a = this.GetHSLA()
        UIColor.FromHSLA(h, s, lightness, a)

    member this.WithHSLSaturation saturation =
        let h, _, l, a = this.GetHSLA()
        UIColor.FromHSLA(h, saturation, l, a)


let QColors = [| UIColor.QBlue ; UIColor.QYellow ; UIColor.QSalmon ; UIColor.QMagenta ; UIColor.QGreen ; UIColor.QLime |]

let CGRectWithSize (width : float)  (height : float) =
    new CGRect(nfloat 0.0, nfloat 0.0, nfloat width, nfloat height)