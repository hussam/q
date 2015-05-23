namespace Q.iOS

open System
open UIKit
open Foundation

open Praeclarum.AutoLayout

type SettingsViewController() as this =
    inherit UIViewController()
    do
        this.EdgesForExtendedLayout <- UIRectEdge.None
        this.Title <- "Settings"

    override this.ViewDidLoad() =
        base.ViewDidLoad()

        let name = new StyledTextField()
        name.Placeholder <- "Name"

        let email = new StyledTextField()
        email.Placeholder <- "Email"

        let view = this.View
        view.BackgroundColor <- UIColor.White
        view.AddSubviews(name, email)
        view.AddConstraints [|
                name.LayoutTop == view.LayoutTop + nfloat 20.0
                name.LayoutLeft == view.LayoutLeft + nfloat 20.0
                name.LayoutWidth == view.LayoutWidth - nfloat 40.0

                email.LayoutTop == name.LayoutBottom + nfloat 20.0
                email.LayoutLeft == name.LayoutLeft
                email.LayoutWidth == name.LayoutWidth
            |]