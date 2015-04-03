namespace Q.iOS

open System
open System.Drawing
open UIKit
open Foundation

open qlib

type QueueCell = 
    inherit UITableViewCell

    [<Export("initWithStyle:reuseIdentifier:")>]
    new (style : UITableViewCellStyle, reuseId : string) = {
        inherit UITableViewCell(UITableViewCellStyle.Subtitle, reuseId)
    }

    static member ReuseId : string = "QueueCell"

    member this.Bind (item : QItem) =
        this.TextLabel.Text <- item.Text
        this.DetailTextLabel.Text <- item.Topic


type QueueViewSource(tableView : UITableView) =
    inherit UITableViewSource()

    let queue = QLib.GetQueueView()
    let tv = tableView

    do
        queue.CollectionChanged.Add(fun _ -> tv.ReloadData())

    
    override this.RowsInSection(tableView, section) = nint queue.Count
    override this.GetCell(tableView, indexPath) =
        let cell = tableView.DequeueReusableCell (QueueCell.ReuseId, indexPath) :?> QueueCell
        cell.Bind(queue.[indexPath.Row])
        cell :> UITableViewCell


[<Register("MainViewController")>]
type MainViewController() as this = 
    inherit UITableViewController()
    do
        this.Title <- "My Queue"
        let addBtn = new UIBarButtonItem(UIBarButtonSystemItem.Add, fun sender eventArgs ->
            let c = new NewItemViewController()
            this.PresentViewController(c, true, null))
        this.NavigationItem.SetRightBarButtonItem(addBtn, false)

    // Release any cached data, images, etc that aren't in use.
    override this.DidReceiveMemoryWarning() = base.DidReceiveMemoryWarning()

    // Perform any additional setup after loading the view, typically from a nib.
    override this.ViewDidLoad() =
        base.ViewDidLoad()
        let tv = this.TableView
        tv.RegisterClassForCellReuse(Operators.typeof<QueueCell>, QueueCell.ReuseId)
        tv.Source <- new QueueViewSource(tv)

    // Return true for supported orientations
    override this.ShouldAutorotateToInterfaceOrientation(orientation) = 
        orientation <> UIInterfaceOrientation.PortraitUpsideDown

