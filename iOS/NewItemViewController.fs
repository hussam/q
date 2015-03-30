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


type HashtagSource(hashtagTitles) =
    inherit UICollectionViewSource()

    let titles : string[] = hashtagTitles

    override this.GetItemsCount(collectionView, section) = nint titles.Length

    override this.GetCell(collectionView, indexPath) =
        let cell = collectionView.DequeueReusableCell("hashtagCell", indexPath) :?> HashtagCell
        cell.Title <- titles.[indexPath.Row]
        cell :> UICollectionViewCell

    override this.ItemSelected(collectionView, indexPath) =
        Console.WriteLine(titles.[indexPath.Row])


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

        let prompt = new StyledLabel(Settings.StyledFontNameItalic, 24)
        prompt.Text <- " REMIND ME TO GO FOR A "
        prompt.BackgroundColor <- this.highlightColor
        prompt.TranslatesAutoresizingMaskIntoConstraints <- false

        let dismiss = new UIButton()
        dismiss.SetTitle("X", UIControlState.Normal)
        dismiss.SetTitleColor(this.highlightColor, UIControlState.Normal)
        dismiss.Font <- UIFont.FromName(Settings.StyledFontNameBold, nfloat (float 24))
        dismiss.TouchUpInside.Add(fun x -> this.DismissViewController(true, null) )
        dismiss.TranslatesAutoresizingMaskIntoConstraints <- false

        let hashtags = [| "Coffee"; "Beer"; "Drinks"; "Brunch"; "Lunch"; "Dinner"; "Movie"; "Walk"; |]
        let hashtagsGrid = new HashtagCollectionView()
        hashtagsGrid.Source <- new HashtagSource(hashtags)
        hashtagsGrid.TranslatesAutoresizingMaskIntoConstraints <- false


        let view = this.View
        view.BackgroundColor <- UIColor.White
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


