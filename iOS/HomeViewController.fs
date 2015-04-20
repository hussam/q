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
        this.TextLabel.Enabled <- not item.Completed
        this.DetailTextLabel.Text <- item.Topic
        this.DetailTextLabel.Enabled <- not item.Completed
        this.DetailTextLabel.Hidden <- item.Topic = "OTHER"


type QueueViewSource(tableView : UITableView) =
    inherit UITableViewSource()

    let tv = tableView
    let queues = QLib.AllQueues

    do
        queues |> Array.iter (fun q -> q.CollectionChanged.Add(fun _ -> tv.ReloadData()))

    override this.NumberOfSections(tableView) = nint queues.Length
    override this.RowsInSection(tableView, section) = nint queues.[int section].Count
    override this.TitleForHeader(tableView, section) = QLib.QueueNames.[int section]

    override this.GetCell(tableView, indexPath) =
        let item = queues.[indexPath.Section].[indexPath.Row]
        let cell = tableView.DequeueReusableCell (QueueCell.ReuseId, indexPath) :?> QueueCell
        cell.Bind(item)

        cell.CellSwipeGestureRecognizer.ShortTrigger <- nfloat 0.30
        let frameWithWidth width = CGRectWithSize width 60.0

        if indexPath.Section = QLib.Uncompleted then
            let completedActionView = new UILabel(frameWithWidth 100.0)
            completedActionView.Text <- "Completed"
            completedActionView.TextColor <- UIColor.White
            completedActionView.TextAlignment <- UITextAlignment.Center
            cell.SetSwipeGestureWithView(
                completedActionView,
                UIColor.QGreen,
                SwipeTableCellMode.Exit,
                SwipeTableViewCellState.StateLeftShort,
                new SwipeCompletionBlock(fun view state mode ->
                    QLib.MarkItemAsCompleted(item)
                    Insights.Track("CompletedTask", "topic", item.Topic)
                    )
                )
        else
            let uncompletedAction = new UILabel(frameWithWidth 110.0)
            uncompletedAction.Text <- "Uncomplete"
            uncompletedAction.TextColor <- UIColor.White
            uncompletedAction.TextAlignment <- UITextAlignment.Center
            cell.SetSwipeGestureWithView(
                uncompletedAction,
                UIColor.QBlue,
                SwipeTableCellMode.Exit,
                SwipeTableViewCellState.StateRightShort,
                new SwipeCompletionBlock(fun view state mode ->
                    QLib.MarkItemAsUncompleted(item)
                    Insights.Track("UnCompletedTask", "topic", item.Topic)
                    )
                )

        #if false
        let deleteActionView = new UILabel(frameWithWidth 80.0)
        deleteActionView.Text <- "Delete"
        deleteActionView.TextColor <- UIColor.White
        deleteActionView.TextAlignment <- UITextAlignment.Center
        cell.SetSwipeGestureWithView(
            deleteActionView,
            UIColor.QMagenta,
            SwipeTableCellMode.Exit,
            SwipeTableViewCellState.StateLeftShort,
            new SwipeCompletionBlock(fun view state mode ->
                QLib.DeleteItem(item)
                Insights.Track("DeletedTask", "topic", item.Topic)
                )
            )
        #endif

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
