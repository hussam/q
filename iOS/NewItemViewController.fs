namespace Q.iOS

open System
open System.Collections.Generic
open UIKit
open Foundation
open CoreGraphics

open AddressBook
open AddressBookUI

open Utilites
open Praeclarum.AutoLayout

open qlib

type TopicHeader =
    inherit UICollectionReusableView

    val private lbl : StyledLabel
    member this.Title with get() = this.lbl.Text and set(title) = this.lbl.Text <- title

    static member ReuseId = "topicHeader"

    [<Export("initWithFrame:")>]
    new (frame : CGRect) as this =
        { inherit UICollectionReusableView(frame) ; lbl = new StyledLabel(Settings.StyledFontNameItalic, 18) } then
        this.AddSubview(this.lbl)
        this.AddConstraints [|
            this.lbl.LayoutWidth == this.LayoutWidth
            this.lbl.LayoutHeight == this.LayoutHeight
        |]


type TopicCell =
    inherit UICollectionViewCell

    val private lbl : StyledLabel
    member this.Title with get() = this.lbl.Text and set(title) = this.lbl.Text <- title

    static member ReuseId = "topicCell"

    [<Export("initWithFrame:")>]
    new (frame : CGRect) as this =
        { inherit UICollectionViewCell(frame) ; lbl = new StyledLabel(Settings.StyledFontName, 20) } then
        this.lbl.TextAlignment <- UITextAlignment.Left
        this.lbl.TranslatesAutoresizingMaskIntoConstraints <- false
        this.ContentView.AddSubview(this.lbl)

        this.ContentView.AddConstraints [|
            this.lbl.LayoutWidth == this.ContentView.LayoutWidth
            this.lbl.LayoutHeight == this.ContentView.LayoutHeight
        |]


type TopicsCollectionSource(_vc : UIViewController, onSelected) =
    inherit UICollectionViewSource()

    let vc = _vc
    let topicCategories = QLib.Topics.Keys |> Array.ofSeq
    let topicItems = QLib.Topics.Values |> Array.ofSeq
    let itemSelected = new Event<_>()

    override this.NumberOfSections(collectionView) = nint topicCategories.Length

    override this.GetItemsCount(collectionView, section) = nint topicItems.[int section].Length

    override this.ItemSelected(collectionView, indexPath) =
        let topic = topicItems.[indexPath.Section].[indexPath.Row]
        let qi = new QItem(Topic = topic)
        match topicCategories.[indexPath.Section] with
        | "Follow-up" ->
            // XXX TODO: Display Alert about asking for access to addressbook. Handle case when user denies it.
            let ab, _ = ABAddressBook.Create()
            ab.RequestAccess(Action<bool, NSError> (fun granted errr ->
                if granted then
                    let contactPicker = new ABPeoplePickerNavigationController()
                    contactPicker.Cancelled.Add(fun _ -> vc.DismissViewController(true, null))
                    contactPicker.SelectPerson2.Add(fun e ->
                        qi.Text <- e.Person.FirstName + " " + e.Person.LastName
                        qi.InternalDetails <- e.Person.Id.ToString()
                        onSelected(qi)
                        )
                    vc.PresentViewController(contactPicker, true, null)
                ) )
        | _ ->
            onSelected(qi)

    override this.GetViewForSupplementaryElement(collectionView, elementKind, indexPath) =
        let header = collectionView.DequeueReusableSupplementaryView(elementKind, TopicHeader.ReuseId, indexPath) :?> TopicHeader
        header.Title <- topicCategories.[indexPath.Section]
        header :> UICollectionReusableView

    override this.GetCell(collectionView, indexPath) =
        let cell = collectionView.DequeueReusableCell(TopicCell.ReuseId, indexPath) :?> TopicCell
        cell.Title <- topicItems.[indexPath.Section].[indexPath.Row]
        cell :> UICollectionViewCell


type TopicsCollectionView() as this =
    inherit UICollectionView(Unchecked.defaultof<CGRect>, new UICollectionViewFlowLayout())

    do
        this.BackgroundColor <- UIColor.White
        this.RegisterClassForCell(Operators.typeof<TopicCell>, TopicCell.ReuseId)
        this.RegisterClassForSupplementaryView(Operators.typeof<TopicHeader>, UICollectionElementKindSection.Header, TopicHeader.ReuseId)
        let layout = this.CollectionViewLayout :?> UICollectionViewFlowLayout
        layout.ItemSize <- new CGSize(nfloat 80.0, nfloat 30.0)
        layout.SectionInset <- new UIEdgeInsets(nfloat 0.0, nfloat 10.0, nfloat 0.0, nfloat 10.0)
        layout.HeaderReferenceSize <- new CGSize(nfloat 200.0 , nfloat 60.0)

    override this.CellForItem(indexPath) =
        match indexPath with
        | null -> null
        | _ -> base.CellForItem(indexPath)




type NewItemViewController = 
    inherit UIViewController

    static member private colors = [| UIColor.Aqua; UIColor.Blue; UIColor.Fuchsia; UIColor.MidGreen; UIColor.Lime; UIColor.Maroon; UIColor.Navy; UIColor.Olive; UIColor.Pink; UIColor.MidPurple; UIColor.Red; UIColor.Teal; UIColor.Yellow |]
    //static member private colors = QColors
    static member private topicColor = UIColor.FromRGB(255, 211, 0)

    val private highlightColor : UIColor

    new() = 
        let random = new Random()
        let colors = NewItemViewController.colors

        {
            inherit UIViewController()
            highlightColor = colors.[random.Next colors.Length].WithLightness(nfloat 0.75)
        }    

    override this.ViewDidLoad() =
        base.ViewDidLoad()

        let view = this.View
        view.BackgroundColor <- UIColor.White

        let prompt = new StyledLabel(Settings.StyledFontNameItalic, 24)
        prompt.Text <- " REMIND ME TO " //" REMIND ME TO GO FOR "
        prompt.BackgroundColor <- this.highlightColor

        let dismiss = new UIButton()
        dismiss.SetTitle("X", UIControlState.Normal)
        dismiss.SetTitleColor(this.highlightColor.WithLightness(nfloat 0.2).WithHSLSaturation(nfloat 0.5), UIControlState.Normal)
        dismiss.Font <- UIFont.FromName(Settings.StyledFontNameBold, nfloat (float 24))
        dismiss.TouchUpInside.Add(fun _ -> this.View.EndEditing(true) |> ignore; this.DismissViewController(true, null) )
        dismiss.TranslatesAutoresizingMaskIntoConstraints <- false

        let topicsGrid = new TopicsCollectionView()
        topicsGrid.TranslatesAutoresizingMaskIntoConstraints <- false
        topicsGrid.Source <- new TopicsCollectionSource(this, fun qItem ->
            topicsGrid.RemoveFromSuperview()

            let topic = new StyledLabel(Settings.StyledFontNameBoldItalic, 28)
            topic.Text <- qItem.Topic.ToUpper()
            topic.BackgroundColor <- NewItemViewController.topicColor

            let at = new StyledLabel(Settings.StyledFontNameBold, 20, Text = "@")

            let maxLength = 20
            let counter = new StyledLabel(Settings.StyledFontNameBold, 24)
            counter.Text <- maxLength.ToString()

            let details = new StyledTextField(24, this.highlightColor)
            details.Placeholder <- "...(optional)"
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
            match qItem.Text with
            | null -> ()
            | text -> details.Text <- text


            let save = new UIButton()
            let saveBtnWidth = nfloat 160.0
            save.SetTitle("SAVE IT!", UIControlState.Normal)
            save.SetTitleColor(UIColor.Black, UIControlState.Normal)
            save.Font <- UIFont.FromName(Settings.StyledFontNameBoldItalic, nfloat 24.0)
            save.BackgroundColor <- this.highlightColor
            save.TranslatesAutoresizingMaskIntoConstraints <- false
            save.TouchUpInside.Add(fun _ ->
                qItem.Text <- details.Text
                QLib.SaveItem(qItem) |> ignore
                Xamarin.Insights.Track("CreatedTask", "topic", topic.Text)
                details.ResignFirstResponder() |> ignore
                this.DismissViewController(true, null)
                )

            view.AddSubview(topic)
            view.AddSubview(at)
            view.AddSubview(details)
            view.AddSubview(counter)
            view.AddSubview(save)

            view.AddConstraints [|
                topic.LayoutLeft == view.LayoutLeft + nfloat 10.0
                topic.LayoutTop == prompt.LayoutBottom + nfloat 10.0
                at.LayoutLeft == topic.LayoutLeft + nfloat 10.0
                at.LayoutCenterY == details.LayoutCenterY
                details.LayoutTop == topic.LayoutBottom + nfloat 5.0
                details.LayoutLeft == at.LayoutRight + nfloat 10.0
                counter.LayoutRight == view.LayoutRight
                counter.LayoutCenterY == details.LayoutCenterY
                details.LayoutRight == counter.LayoutLeft - nfloat 5.0
                save.LayoutTop == details.LayoutBottom + nfloat 100.0
                save.LayoutRight == view.LayoutRight
                save.LayoutLeft == view.LayoutRight - saveBtnWidth
            |]
        )

        view.AddSubview(prompt)
        view.AddSubview(dismiss)
        view.AddSubview(topicsGrid)

        view.AddConstraints [|
            prompt.LayoutTop == view.LayoutTop + nfloat 20.0
            prompt.LayoutLeft == view.LayoutLeft
            dismiss.LayoutTop == view.LayoutTop + nfloat 20.0
            dismiss.LayoutRight == view.LayoutRight
            topicsGrid.LayoutLeft == view.LayoutLeft
            topicsGrid.LayoutRight == view.LayoutRight
            topicsGrid.LayoutTop == prompt.LayoutBottom + nfloat 20.0
            topicsGrid.LayoutBottom == view.LayoutBottom
        |]