namespace Q.iOS

open System
open UIKit
open Foundation
open CoreGraphics

open SwipeableViewCell
open Xamarin

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

        cell.CellSwipeGestureRecognizer.LongTrigger <- nfloat 0.55
        let frame = CGRect(nfloat 0.0, nfloat 0.0, nfloat 100.0, nfloat 60.0)

        let swipeActionView = new UILabel(frame)
        swipeActionView.TextColor <- UIColor.White
        swipeActionView.TextAlignment <- UITextAlignment.Center

        if indexPath.Section = 0 then
            swipeActionView.Text <- "Remove"
            cell.SetSwipeGestureWithView(
                swipeActionView,
                UIColor.QSalmon,
                SwipeTableCellMode.Exit,
                SwipeTableViewCellState.StateLeftShort,
                new SwipeCompletionBlock(fun view state mode ->
                    QLib.UnscheduleItem(item)
                    Insights.Track("RequeuedTask", "topic", item.Topic)
                    )
                )
        else
            swipeActionView.Text <- "Schedule"
            cell.SetSwipeGestureWithView(
                swipeActionView,
                UIColor.QBlue,
                SwipeTableCellMode.Exit,
                SwipeTableViewCellState.StateLeftShort,
                new SwipeCompletionBlock(fun view state mode ->
                    QLib.ScheduleItemForToday(item)
                    Insights.Track("ScheduledQueuedTask", "topic", item.Topic)
                    )
                )

        let completedActionView = new UILabel(frame)
        completedActionView.Text <- "Completed"
        completedActionView.TextColor <- UIColor.White
        completedActionView.TextAlignment <- UITextAlignment.Left
        cell.SetSwipeGestureWithView(
            completedActionView,
            UIColor.QGreen,
            SwipeTableCellMode.Exit,
            SwipeTableViewCellState.StateRightShort,
            new SwipeCompletionBlock(fun view state mode ->
                QLib.MarkItemAsCompleted(item)
                Insights.Track("CompletedTask", "topic", item.Topic)
                )
            )

        let deleteActionView = new UILabel(frame)
        deleteActionView.Text <- "Delete"
        deleteActionView.TextColor <- UIColor.White
        deleteActionView.TextAlignment <- UITextAlignment.Left
        cell.SetSwipeGestureWithView(
            deleteActionView,
            UIColor.QMagenta,
            SwipeTableCellMode.Exit,
            SwipeTableViewCellState.StateRightLong,
            new SwipeCompletionBlock(fun view state mode ->
                QLib.DeleteItem(item)
                Insights.Track("DeletedTask", "topic", item.Topic)
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
