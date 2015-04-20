﻿namespace Q.iOS

open System
open System.Collections.Generic
open UIKit
open Foundation
open CoreGraphics

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


type TopicsCollectionSource(onSelected) =
    inherit UICollectionViewSource()

    let topicCategories = QLib.Topics.Keys |> Array.ofSeq
    let topicItems = QLib.Topics.Values |> Array.ofSeq
    let itemSelected = new Event<_>()

    override this.NumberOfSections(collectionView) = nint topicCategories.Length

    override this.GetItemsCount(collectionView, section) = nint topicItems.[int section].Length

    override this.ItemSelected(collectionView, indexPath) = onSelected(topicItems.[indexPath.Section].[indexPath.Row])

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
        prompt.Text <- " REMIND ME TO GO FOR "
        prompt.BackgroundColor <- this.highlightColor

        let dismiss = new UIButton()
        dismiss.SetTitle("X", UIControlState.Normal)
        dismiss.SetTitleColor(this.highlightColor.WithLightness(nfloat 0.2).WithHSLSaturation(nfloat 0.5), UIControlState.Normal)
        dismiss.Font <- UIFont.FromName(Settings.StyledFontNameBold, nfloat (float 24))
        dismiss.TouchUpInside.Add(fun _ -> this.View.EndEditing(true) |> ignore; this.DismissViewController(true, null) )
        dismiss.TranslatesAutoresizingMaskIntoConstraints <- false

        let topicsGrid = new TopicsCollectionView()
        topicsGrid.TranslatesAutoresizingMaskIntoConstraints <- false
        topicsGrid.Source <- new TopicsCollectionSource(fun selectedTitle ->
            topicsGrid.RemoveFromSuperview()

            let topic = new StyledLabel(Settings.StyledFontNameBoldItalic, 28)
            topic.Text <- selectedTitle.ToUpper()
            topic.BackgroundColor <- NewItemViewController.topicColor

            let at = new StyledLabel(Settings.StyledFontNameBold, 20, Text = "at")

            let maxLength = 20
            let counter = new StyledLabel(Settings.StyledFontNameBold, 24)
            counter.Text <- maxLength.ToString()

            let venue = new StyledTextField(24, this.highlightColor)
            venue.Placeholder <- "...(optional)"
            venue.AutocapitalizationType <- UITextAutocapitalizationType.AllCharacters
            venue.ReturnKeyType <- UIReturnKeyType.Done
            venue.BecomeFirstResponder() |> ignore
            venue.ShouldChangeCharacters <- new UITextFieldChange(fun x range str ->
                // prevent crashing undo bug
                if range.Length + range.Location > nint venue.Text.Length
                then
                    false
                else
                    let remainingCharacters = maxLength - (venue.Text.Length + str.Length - (int range.Length))   /// subtracting range length in case of replacement
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
                QLib.SaveItem(new QItem(Text = venue.Text, Topic = topic.Text)) |> ignore
                Xamarin.Insights.Track("CreatedTask", "topic", topic.Text)
                venue.ResignFirstResponder() |> ignore
                this.DismissViewController(true, null)
                )

            view.AddSubview(topic)
            view.AddSubview(at)
            view.AddSubview(venue)
            view.AddSubview(counter)
            view.AddSubview(save)

            view.AddConstraints [|
                topic.LayoutLeft == view.LayoutLeft + nfloat 10.0
                topic.LayoutTop == prompt.LayoutBottom + nfloat 10.0
                at.LayoutLeft == topic.LayoutLeft + nfloat 10.0
                at.LayoutCenterY == venue.LayoutCenterY
                venue.LayoutTop == topic.LayoutBottom + nfloat 5.0
                venue.LayoutLeft == at.LayoutRight + nfloat 10.0
                counter.LayoutRight == view.LayoutRight
                counter.LayoutCenterY == venue.LayoutCenterY
                venue.LayoutRight == counter.LayoutLeft - nfloat 5.0
                save.LayoutTop == venue.LayoutBottom + nfloat 100.0
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