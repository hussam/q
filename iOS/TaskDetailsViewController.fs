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

        let notesLbl = new StyledLabel(Text = "Notes:")
        let notes = new UITextView()
        notes.TranslatesAutoresizingMaskIntoConstraints <- false
        notes.Text <- task.Notes
        notes.Editable <- false

        let delete = new UIButton()
        let deleteBtnWidth = nfloat 160.0
        delete.SetTitle("DELETE", UIControlState.Normal)
        delete.SetTitleColor(UIColor.Black, UIControlState.Normal)
        delete.Font <- UIFont.FromName(Settings.StyledFontNameBoldItalic, nfloat 24.0)
        delete.BackgroundColor <- UIColor.QMagenta
        delete.TranslatesAutoresizingMaskIntoConstraints <- false
        delete.TouchUpInside.Add(fun _ ->
            QLib.DeleteItem(task)
            Xamarin.Insights.Track("DeletedTask")
            this.NavigationController.PopViewController(true) |> ignore
            )

        view.AddSubviews(name, createdOn, completedOn, completedLbl, completed, notesLbl, notes, delete)
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

                notesLbl.LayoutLeft == name.LayoutLeft
                notesLbl.LayoutTop == completed.LayoutBottom + nfloat 10.0
                notes.LayoutTop == notesLbl.LayoutBottom + nfloat 5.0
                notes.LayoutLeft == notesLbl.LayoutLeft
                notes.LayoutRight == completed.LayoutRight
                notes.LayoutBottom == delete.LayoutTop - nfloat 10.0

                delete.LayoutTop == view.LayoutBottom - nfloat 150.0
                delete.LayoutLeft == view.LayoutLeft
                delete.LayoutRight == view.LayoutLeft + deleteBtnWidth
            |]
