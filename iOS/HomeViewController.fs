namespace Q.iOS

open System
open UIKit
open Foundation
open CoreGraphics

open SwipeableViewCell

open Utilites
open qlib

type QueueCell = 
    inherit SwipeableViewCell

    val mutable private item : QItem

    [<Export("initWithStyle:reuseIdentifier:")>]
    new (style : UITableViewCellStyle, reuseId : string) =
        { inherit SwipeableViewCell(UITableViewCellStyle.Subtitle, reuseId); item = null }


    static member ReuseId : string = "QueueCell"

    member this.Bind (item : QItem) =
        this.item <- item
        this.TextLabel.Text <- item.Text
        this.DetailTextLabel.Text <- item.Topic


type QueueViewSource(tableView : UITableView) =
    inherit UITableViewSource()

    let tv = tableView
    let queues = QLib.AllQueues

    do
        queues |> Array.iter (fun q -> q.CollectionChanged.Add(fun _ -> tv.ReloadData()))

    override this.NumberOfSections(tableView) = nint queues.Length
    override this.RowsInSection(tableView, section) = nint queues.[int section].Count
    override this.TitleForHeader(tableView, section) = QLogic.Buckets.[int section]

    override this.GetCell(tableView, indexPath) =
        let item = queues.[indexPath.Section].[indexPath.Row]
        let cell = tableView.DequeueReusableCell (QueueCell.ReuseId, indexPath) :?> QueueCell
        cell.Bind(item)

        let scheduleView = new UILabel(CGRect(nfloat 0.0, nfloat 0.0, nfloat 100.0, nfloat 60.0))
        scheduleView.Text <- "Schedule"
        scheduleView.TextColor <- UIColor.White
        scheduleView.TextAlignment <- UITextAlignment.Center
        cell.SetSwipeGestureWithView(
            scheduleView,
            UIColor.QBlue,
            SwipeTableCellMode.Exit,
            SwipeTableViewCellState.StateLeftShort,
            new SwipeCompletionBlock(fun view state mode ->
                QLib.ScheduleItemForToday(item) |> ignore
                )
            )

        cell :> UITableViewCell


type HomeViewController() as this = 
    inherit UITableViewController(UITableViewStyle.Grouped)
    do
        this.Title <- "My Queue"
        let addBtn = new UIBarButtonItem(UIBarButtonSystemItem.Add, fun sender eventArgs ->
            let c = new NewItemViewController()
            this.PresentViewController(c, true, null))
        this.NavigationItem.SetRightBarButtonItem(addBtn, false)

    // Perform any additional setup after loading the view
    override this.ViewDidLoad() =
        base.ViewDidLoad()
        let tv = this.TableView
        tv.RegisterClassForCellReuse(Operators.typeof<QueueCell>, QueueCell.ReuseId)
        tv.Source <- new QueueViewSource(tv)
        tv.AllowsSelection <- false
