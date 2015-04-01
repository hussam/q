namespace Q.iOS

open System
open UIKit
open Foundation
open CoreGraphics

open Utilites
open Praeclarum.AutoLayout


type HashtagCell =
    inherit UICollectionViewCell

    val private lbl : StyledLabel
    member this.Title with get() = this.lbl.Text and set(title) = this.lbl.Text <- title

    [<Export("initWithFrame:")>]
    new (frame : CGRect) as this =
        { inherit UICollectionViewCell(frame) ; lbl = new StyledLabel(Settings.StyledFontName, 20) } then
        this.lbl.TextAlignment <- UITextAlignment.Center
        this.lbl.TranslatesAutoresizingMaskIntoConstraints <- false
        this.ContentView.AddSubview(this.lbl)

        this.ContentView.AddConstraints [|
            this.lbl.LayoutWidth == this.ContentView.LayoutWidth
            this.lbl.LayoutHeight == this.ContentView.LayoutHeight
        |]


type HashtagSource(hashtagTitles, onSelected) =
    inherit UICollectionViewSource()

    let titles : string[] = hashtagTitles
    let itemSelected = new Event<_>()

    override this.GetItemsCount(collectionView, section) = nint titles.Length

    override this.ItemSelected(collectionView, indexPath) = onSelected(titles.[indexPath.Row])

    override this.GetCell(collectionView, indexPath) =
        let cell = collectionView.DequeueReusableCell("hashtagCell", indexPath) :?> HashtagCell
        cell.Title <- titles.[indexPath.Row]
        cell :> UICollectionViewCell


type HashtagCollectionView() as this =
    inherit UICollectionView(Unchecked.defaultof<CGRect>, new UICollectionViewFlowLayout())

    do
        this.BackgroundColor <- UIColor.White
        this.RegisterClassForCell(Operators.typeof<HashtagCell>, "hashtagCell")
        let layout = this.CollectionViewLayout :?> UICollectionViewFlowLayout
        layout.ItemSize <- new CGSize(nfloat 80.0, nfloat 30.0)
        layout.SectionInset <- new UIEdgeInsets(nfloat 0.0, nfloat 10.0, nfloat 0.0, nfloat 10.0)

    override this.CellForItem(indexPath) =
        match indexPath with
        | null -> null
        | _ -> base.CellForItem(indexPath)




type NewItemViewController = 
    inherit UIViewController

    //static member private colors = [ UIColor.Aqua; UIColor.Blue; UIColor.Fuchsia; UIColor.MidGreen; UIColor.Lime; UIColor.Maroon; UIColor.Navy; UIColor.Olive; UIColor.Pink; UIColor.MidPurple; UIColor.Red; UIColor.Teal; UIColor.Yellow ]
    static member private colors = QColors
    static member private hashtagColor = UIColor.FromRGB(255, 211, 0)

    val private highlightColor : UIColor

    new() = 
        let random = new Random()
        let colors = NewItemViewController.colors

        {
            inherit UIViewController()
            highlightColor = colors.[random.Next colors.Length].WithSaturation(nfloat 0.8).ColorWithAlpha(nfloat 0.5)
        }    

    override this.ViewDidLoad() =
        base.ViewDidLoad()

        let view = this.View
        view.BackgroundColor <- UIColor.White

        let prompt = new StyledLabel(Settings.StyledFontNameItalic, 24)
        prompt.Text <- " REMIND ME TO GO FOR A "
        prompt.BackgroundColor <- this.highlightColor

        let dismiss = new UIButton()
        dismiss.SetTitle("X", UIControlState.Normal)
        dismiss.SetTitleColor(this.highlightColor, UIControlState.Normal)
        dismiss.Font <- UIFont.FromName(Settings.StyledFontNameBold, nfloat (float 24))
        dismiss.TouchUpInside.Add(fun x -> this.DismissViewController(true, null) )
        dismiss.TranslatesAutoresizingMaskIntoConstraints <- false

        let hashtags = [| "Coffee"; "Beer"; "Drinks"; "Brunch"; "Lunch"; "Dinner"; "Movie"; "Walk"; |]
        let hashtagsGrid = new HashtagCollectionView()
        hashtagsGrid.TranslatesAutoresizingMaskIntoConstraints <- false
        hashtagsGrid.Source <- new HashtagSource(hashtags, fun selectedTitle ->
            hashtagsGrid.RemoveFromSuperview()

            let hashtag = new StyledLabel(Settings.StyledFontNameBoldItalic, 28)
            hashtag.Text <- selectedTitle.ToUpper()
            hashtag.BackgroundColor <- NewItemViewController.hashtagColor

            let at = new StyledLabel(Settings.StyledFontNameBold, 20, Text = "at")

            let venue = new StyledTextField(24, this.highlightColor)
            venue.Placeholder <- "...(optional)"
            venue.AutocapitalizationType <- UITextAutocapitalizationType.AllCharacters
            venue.BecomeFirstResponder() |> ignore

            let save = new UIButton()
            let saveBtnWidth = nfloat 160.0
            save.SetTitle("SAVE IT!", UIControlState.Normal)
            save.SetTitleColor(UIColor.Black, UIControlState.Normal)
            save.Font <- UIFont.FromName(Settings.StyledFontNameBoldItalic, nfloat 24.0)
            save.BackgroundColor <- this.highlightColor
            save.TranslatesAutoresizingMaskIntoConstraints <- false
            save.TouchUpInside.Add(fun x -> let alert = new UIAlertView("Clicked", "Clicked", null, "OK") in alert.Show())

            view.AddSubview(hashtag)
            view.AddSubview(at)
            view.AddSubview(venue)
            view.AddSubview(save)

            view.AddConstraints [|
                hashtag.LayoutLeft == view.LayoutLeft + nfloat 10.0
                hashtag.LayoutTop == prompt.LayoutBottom + nfloat 10.0
                at.LayoutLeft == hashtag.LayoutLeft + nfloat 10.0
                at.LayoutBottom == venue.LayoutBottom
                venue.LayoutTop == hashtag.LayoutBottom + nfloat 5.0
                venue.LayoutLeft == at.LayoutRight + nfloat 5.0
                venue.LayoutRight == view.LayoutRight
                save.LayoutTop == venue.LayoutBottom + nfloat 100.0
                save.LayoutRight == view.LayoutRight
                save.LayoutLeft == view.LayoutRight - saveBtnWidth
            |]
        )

        view.AddSubview(prompt)
        view.AddSubview(dismiss)
        view.AddSubview(hashtagsGrid)

        view.AddConstraints [|
            prompt.LayoutTop == view.LayoutTop + nfloat 20.0
            prompt.LayoutLeft == view.LayoutLeft
            dismiss.LayoutTop == view.LayoutTop + nfloat 20.0
            dismiss.LayoutRight == view.LayoutRight
            hashtagsGrid.LayoutLeft == view.LayoutLeft
            hashtagsGrid.LayoutRight == view.LayoutRight
            hashtagsGrid.LayoutTop == prompt.LayoutBottom + nfloat 20.0
            hashtagsGrid.LayoutBottom == view.LayoutBottom
        |]