namespace Q.iOS

open System
open UIKit
open Foundation

type SettingsViewController() as this =
    inherit UIViewController()
    do
        this.Title <- "Settings"

    override this.ViewDidLoad() =
        base.ViewDidLoad()
        let view = this.View
        view.BackgroundColor <- UIColor.White
        let label = new UILabel(view.Bounds)
        label.Text <- "TBD"
        view.AddSubview(label)