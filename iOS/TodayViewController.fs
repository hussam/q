namespace Q.iOS

open System
open UIKit
open Foundation

open qlib

type TodayCell = 
    inherit UITableViewCell

    [<Export("initWithStyle:reuseIdentifier:")>]
    new (style : UITableViewCellStyle, reuseId : string) = {
        inherit UITableViewCell(UITableViewCellStyle.Subtitle, reuseId)
    }

    static member ReuseId : string = "TodayCell"

    member this.Bind (item : QItem) =
        this.TextLabel.Text <- item.Text
        this.DetailTextLabel.Text <- item.Topic


type TodayViewSource(tableView : UITableView) =
    inherit UITableViewSource()

    do
        QLib.TodaysAgenda |> Array.iter (fun agenda -> agenda.CollectionChanged.Add(fun _ -> tableView.ReloadData()))

    override this.NumberOfSections(tableView) = nint QLib.TodaysAgenda.Length
    override this.RowsInSection(tableView, section) = nint QLib.TodaysAgenda.[int section] .Count

    override this.GetCell(tableView, indexPath) =
        let cell = tableView.DequeueReusableCell (TodayCell.ReuseId, indexPath) :?> TodayCell
        cell.Bind(QLib.TodaysAgenda.[indexPath.Section].[indexPath.Row])
        cell :> UITableViewCell


type TodayViewController() as this =
    inherit UITableViewController(UITableViewStyle.Grouped)
    do
        this.Title <- "Today"
        let addBtn = new UIBarButtonItem(UIBarButtonSystemItem.Add, fun sender eventArgs ->
            let c = new NewItemViewController()
            this.PresentViewController(c, true, null))
        this.NavigationItem.SetRightBarButtonItem(addBtn, false)

        let queueBtn = new UIBarButtonItem("My Queue", UIBarButtonItemStyle.Plain, fun sender eventArgs ->
            let qvc = new UINavigationController( new HomeViewController() )
            qvc.ModalTransitionStyle <- UIModalTransitionStyle.FlipHorizontal
            this.PresentViewController(qvc, true, null))
        this.NavigationItem.SetLeftBarButtonItem(queueBtn, false)

    override this.ViewDidLoad() =
        base.ViewDidLoad()
        let tv = this.TableView
        tv.RegisterClassForCellReuse(Operators.typeof<TodayCell>, TodayCell.ReuseId)
        tv.Source <- new TodayViewSource(tv)
        tv.AllowsSelection <- false
