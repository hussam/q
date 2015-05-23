namespace Q.iOS

open System
open UIKit

open Praeclarum.AutoLayout
open Utilites

open qlib


type SimpleNewItemViewController = 
    inherit UIViewController

    static member private colors = [| UIColor.Aqua; UIColor.Blue; UIColor.Fuchsia; UIColor.MidGreen; UIColor.Lime; UIColor.Maroon; UIColor.Navy; UIColor.Olive; UIColor.Pink; UIColor.MidPurple; UIColor.Red; UIColor.Teal; UIColor.Yellow |]

    val private highlightColor : UIColor

    new() = 
        let random = new Random()
        let colors = SimpleNewItemViewController.colors

        {
            inherit UIViewController()
            highlightColor = colors.[random.Next colors.Length].WithLightness(nfloat 0.75)
        }    

    override this.ViewDidLoad() =
        base.ViewDidLoad()

        let view = this.View
        view.BackgroundColor <- UIColor.White

        let prompt = new StyledLabel(Settings.StyledFontNameItalic, 24)
        prompt.Text <- " REMIND ME TO "
        prompt.BackgroundColor <- this.highlightColor

        let dismiss = new UIButton()
        dismiss.SetTitle("X", UIControlState.Normal)
        dismiss.SetTitleColor(this.highlightColor.WithLightness(nfloat 0.2).WithHSLSaturation(nfloat 0.5), UIControlState.Normal)
        dismiss.Font <- UIFont.FromName(Settings.StyledFontNameBold, nfloat (float 24))
        dismiss.TouchUpInside.Add(fun _ -> this.View.EndEditing(true) |> ignore; this.DismissViewController(true, null) )
        dismiss.TranslatesAutoresizingMaskIntoConstraints <- false

        let maxLength = 40
        let counter = new StyledLabel(Settings.StyledFontNameBold, 24)
        counter.Text <- maxLength.ToString()

        let saveBtn = new UIButton()
        let saveBtnWidth = nfloat 160.0
        saveBtn.SetTitle("SAVE IT!", UIControlState.Normal)
        saveBtn.SetTitleColor(UIColor.Black, UIControlState.Normal)
        saveBtn.Font <- UIFont.FromName(Settings.StyledFontNameBoldItalic, nfloat 24.0)
        saveBtn.BackgroundColor <- this.highlightColor
        saveBtn.TranslatesAutoresizingMaskIntoConstraints <- false
        saveBtn.Enabled <- false

        let details = new StyledTextField(24, this.highlightColor)
        details.Placeholder <- "..."
        details.ReturnKeyType <- UIReturnKeyType.Done
        details.BecomeFirstResponder() |> ignore
        details.ShouldChangeCharacters <- new UITextFieldChange(fun textField range str ->
            // prevent crashing undo bug
            if range.Length + range.Location > nint textField.Text.Length
            then
                false
            else
                let textLength = (textField.Text.Length + str.Length - (int range.Length))   /// subtracting range length in case of replacement
                saveBtn.Enabled <- textLength > 0
                let remainingCharacters = maxLength - textLength
                if remainingCharacters < 0
                then
                    false
                else
                    if remainingCharacters > 5
                    then
                        counter.TextColor <- UIColor.Black
                        counter.BackgroundColor <- UIColor.White
                    else
                        counter.TextColor <- UIColor.White
                        counter.BackgroundColor <- UIColor.Red.WithBrightness(nfloat 0.65)

                    counter.Text <- remainingCharacters.ToString()
                    true
            )

        let save itemText =
            let qItem = new QItem(Text = itemText)
            QLib.SaveItem(qItem) |> ignore
            Xamarin.Insights.Track("CreatedTask")
            details.ResignFirstResponder() |> ignore
            this.DismissViewController(true, null)

        saveBtn.TouchUpInside.Add(fun _ -> save details.Text)
        details.ShouldReturn <- new UITextFieldCondition(fun textField ->
            if (textField.Text <> null && textField.Text.Length > 0) then
                save textField.Text
                true
            else
                false
        )

        view.AddSubview(prompt)
        view.AddSubview(dismiss)
        view.AddSubview(details)
        view.AddSubview(counter)
        view.AddSubview(saveBtn)

        view.AddConstraints [|
            prompt.LayoutTop == view.LayoutTop + nfloat 20.0
            prompt.LayoutLeft == view.LayoutLeft

            dismiss.LayoutTop == view.LayoutTop + nfloat 20.0
            dismiss.LayoutRight == view.LayoutRight

            details.LayoutTop == prompt.LayoutBottom + nfloat 10.0
            details.LayoutLeft == view.LayoutLeft + nfloat 10.0
            details.LayoutRight == view.LayoutRight - nfloat 10.0

            counter.LayoutTop == details.LayoutBottom + nfloat 5.0
            counter.LayoutRight == view.LayoutRight

            saveBtn.LayoutTop == details.LayoutBottom + nfloat 100.0
            saveBtn.LayoutRight == view.LayoutRight
            saveBtn.LayoutLeft == view.LayoutRight - saveBtnWidth
        |]