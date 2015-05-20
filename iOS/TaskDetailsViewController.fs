namespace Q.iOS

open System
open UIKit
open Foundation
open Utilites
open qlib

open Praeclarum.AutoLayout

type TaskDetailsViewController(task : QItem) as this =
    inherit UIViewController()
    do
        this.EdgesForExtendedLayout <- UIRectEdge.None
        this.Title <- "Task Details"

    override this.ViewDidLoad() =
        base.ViewDidLoad()

        let view = this.View
        view.BackgroundColor <- UIColor.White

        let name = new StyledLabel(Settings.StyledFontNameBoldItalic, 24, Text = " " + task.Text + " ")
        name.BackgroundColor <- UIColor.QHighlight

        let createdOn = new StyledLabel()
        createdOn.Text <- String.Format("Created On: {0: ddd, MM/d a\\t h:mm tt}", task.CreatedOn.ToLocalTime())

        let completedOn = new StyledLabel()
        let displayCompletionDate () =
            completedOn.Text <- String.Format("Completed On: {0: ddd, MM/d a\\t h:mm tt}", task.CompletedOn.ToLocalTime())
            completedOn.Hidden <- not task.Completed
        displayCompletionDate()

        let completedLbl = new StyledLabel(Text = "Completed:")
        let completed = new UISwitch(On = task.Completed)
        completed.TranslatesAutoresizingMaskIntoConstraints <- false
        completed.ValueChanged.Add(fun _ ->
            match completed.On with
            | true -> QLib.MarkItemAsCompleted(task)
            | false -> QLib.MarkItemAsUncompleted(task)
            displayCompletionDate()
            )

        view.AddSubviews(name, createdOn, completedOn, completedLbl, completed)
        view.AddConstraints [|
                name.LayoutTop == view.LayoutTop + nfloat 20.0
                name.LayoutLeft == view.LayoutLeft + nfloat 10.0

                createdOn.LayoutTop == name.LayoutBottom + nfloat 20.0
                createdOn.LayoutRight == view.LayoutRight - nfloat 10.0

                completedOn.LayoutTop == createdOn.LayoutBottom + nfloat 10.0
                completedOn.LayoutRight == createdOn.LayoutRight

                completed.LayoutTop == completedOn.LayoutBottom + nfloat 10.0
                completed.LayoutRight == createdOn.LayoutRight
                completedLbl.LayoutCenterY == completed.LayoutCenterY
                completedLbl.LayoutRight == completed.LayoutLeft - nfloat 5.0
            |]
