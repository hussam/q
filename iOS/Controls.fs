namespace Q.iOS

open System
open UIKit

type StyledLabel(?fontName : string, ?fontSize : int) as this =
    inherit UILabel()

    do
        let size = match fontSize with
                    | Some fsize -> nfloat (float fsize)
                    | None -> this.Font.PointSize
        let name = match fontName with
                    | Some fname -> fname
                    | None -> Settings.StyledFontName

        this.Font <- UIFont.FromName(name, size)


type StyledTextField(?fontSize : nfloat, ?tintColor : UIColor) as this =
    inherit UITextField()

    do
        this.BorderStyle <- UITextBorderStyle.None
        this.KeyboardType <- UIKeyboardType.ASCIICapable

        let fsize = match fontSize with
                    | Some size -> size
                    | None -> this.Font.PointSize
        this.Font <- UIFont.FromName(Settings.StyledFontName, fsize)

        match tintColor with
        | Some tint -> this.TintColor <- tint
        | None -> ()
