namespace Q.iOS

open System
open UIKit

open Praeclarum.AutoLayout
open Utilites

open qlib


type SimpleNewItemViewController = 
    inherit UIViewController

    static member private colors = [| UIColor.Aqua; UIColor.Blue; UIColor.Fuchsia; UIColor.MidGreen; UIColor.Lime; UIColor.Maroon; UIColor.Navy; UIColor.Olive; UIColor.Pink; UIColor.MidPurple; UIColor.Red; UIColor.Teal; UIColor.Yellow |]
    //static member private colors = QColors
    static member private topicColor = UIColor.FromRGB(255, 211, 0)

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

        let details = new StyledTextField(24, this.highlightColor)
        details.Placeholder <- "..."
        details.AutocapitalizationType <- UITextAutocapitalizationType.AllCharacters
        details.ReturnKeyType <- UIReturnKeyType.Done
        details.BecomeFirstResponder() |> ignore
        details.ShouldChangeCharacters <- new UITextFieldChange(fun x range str ->
            // prevent crashing undo bug
            if range.Length + range.Location > nint details.Text.Length
            then
                false
            else
                let remainingCharacters = maxLength - (details.Text.Length + str.Length - (int range.Length))   /// subtracting range length in case of replacement
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

        let save = new UIButton()
        let saveBtnWidth = nfloat 160.0
        save.SetTitle("SAVE IT!", UIControlState.Normal)
        save.SetTitleColor(UIColor.Black, UIControlState.Normal)
        save.Font <- UIFont.FromName(Settings.StyledFontNameBoldItalic, nfloat 24.0)
        save.BackgroundColor <- this.highlightColor
        save.TranslatesAutoresizingMaskIntoConstraints <- false
        save.TouchUpInside.Add(fun _ ->
            let qItem = new QItem(Text = details.Text)
            QLib.SaveItem(qItem) |> ignore
            Xamarin.Insights.Track("CreatedTask")
            details.ResignFirstResponder() |> ignore
            this.DismissViewController(true, null)
            )

        view.AddSubview(prompt)
        view.AddSubview(dismiss)
        view.AddSubview(details)
        view.AddSubview(counter)
        view.AddSubview(save)

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

            save.LayoutTop == details.LayoutBottom + nfloat 100.0
            save.LayoutRight == view.LayoutRight
            save.LayoutLeft == view.LayoutRight - saveBtnWidth
        |]