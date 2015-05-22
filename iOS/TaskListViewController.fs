namespace Q.iOS

open System

open UIKit
open Foundation

open SwipeableViewCell
open Xamarin

open Utilites
open qlib

type QueueCell = 
    inherit SwipeableViewCell

    val mutable private item : QItem

    [<Export("initWithStyle:reuseIdentifier:")>]
    new (style : UITableViewCellStyle, reuseId : string) =
        { inherit SwipeableViewCell(UITableViewCellStyle.Default, reuseId); item = null }


    static member ReuseId : string = "QueueCell"

    member this.Bind (item : QItem, swipeMode) =
        this.item <- item
        this.TextLabel.Text <- item.Text
        this.TextLabel.Enabled <- not item.Completed

        this.CellSwipeGestureRecognizer.ShortTrigger <- nfloat 0.30
        let frameWithWidth width = CGRectWithSize width 60.0

        if item.Completed = false then
            let completedActionView = new UILabel(frameWithWidth 100.0)
            completedActionView.Text <- "Completed"
            completedActionView.TextColor <- UIColor.White
            completedActionView.TextAlignment <- UITextAlignment.Center
            this.SetSwipeGestureWithView(
                completedActionView,
                UIColor.QGreen,
                swipeMode,
                SwipeTableViewCellState.StateRightShort,
                new SwipeCompletionBlock(fun view state mode ->
                    QLib.MarkItemAsCompleted(item)
                    this.Bind(item, swipeMode)
                    Insights.Track("CompletedTask")
                    )
                )
        else
            let uncompletedAction = new UILabel(frameWithWidth 110.0)
            uncompletedAction.Text <- "Uncomplete"
            uncompletedAction.TextColor <- UIColor.White
            uncompletedAction.TextAlignment <- UITextAlignment.Center
            this.SetSwipeGestureWithView(
                uncompletedAction,
                UIColor.QBlue,
                swipeMode,
                SwipeTableViewCellState.StateLeftShort,
                new SwipeCompletionBlock(fun view state mode ->
                    QLib.MarkItemAsUncompleted(item)
                    this.Bind(item, swipeMode)
                    Insights.Track("UnCompletedTask")
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



type QueueViewSource(queueType, tableView : UITableView, taskSelected) =
    inherit UITableViewSource()

    let tasks = QLib.GetTasks(queueType)

    do
        tasks.CollectionChanged.Add(fun _ -> tableView.ReloadData())

    let itemAt (indexPath : NSIndexPath) = tasks.[indexPath.Row]

    let tapAction (item : QItem) = ()

    //override this.NumberOfSections(tableView) = nint queues.Length
    override this.RowsInSection(tableView, section) = nint tasks.Count
    //override this.TitleForHeader(tableView, section) = QLib.QueueNames.[int section]

    override this.AccessoryButtonTapped(tableView, indexPath) =
        tapAction (itemAt indexPath)

    override this.RowSelected(tableView, indexPath) =
        tableView.DeselectRow(indexPath, true)
        taskSelected(itemAt indexPath)


    override this.GetCell(tableView, indexPath) =
        let item = itemAt indexPath
        let cell = tableView.DequeueReusableCell (QueueCell.ReuseId, indexPath) :?> QueueCell
        let swipeMode = match queueType with
                        | AllTasks -> SwipeTableCellMode.Switch
                        | _ -> SwipeTableCellMode.Exit
        cell.Bind(item, swipeMode)
        cell :> UITableViewCell


type TaskListViewController(queueType) as this = 
    inherit UITableViewController(UITableViewStyle.Grouped)
    do
        this.Title <- match queueType with
                      | AllTasks -> "All Tasks"
                      | Completed -> "Completed"
                      | TodayAgenda -> "Today's Agenda"
                      | Uncompleted -> "Uncompleted"
        let addBtn = new UIBarButtonItem(UIBarButtonSystemItem.Add, fun sender eventArgs ->
            let c = new SimpleNewItemViewController()
            this.PresentViewController(c, true, null))
        this.NavigationItem.SetRightBarButtonItem(addBtn, false)

    // Perform any additional setup after loading the view
    override this.ViewDidLoad() =
        base.ViewDidLoad()
        let tv = this.TableView
        tv.RegisterClassForCellReuse(Operators.typeof<QueueCell>, QueueCell.ReuseId)
        tv.Source <- new QueueViewSource(queueType, tv, fun task -> this.NavigationController.PushViewController(new TaskDetailsViewController(task), true))