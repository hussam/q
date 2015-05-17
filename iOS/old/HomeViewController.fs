namespace Q.iOS

open System
open System.Text.RegularExpressions

open UIKit
open Foundation
open CoreGraphics
open AddressBook
open MessageUI

open SwipeableViewCell
open Xamarin

open Utilites
open qlib
open qlib.QLogic

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


type QueueViewSource(_vc : UITableViewController,  tableView : UITableView) =
    inherit UITableViewSource()

    let vc = _vc
    let queues = QLib.AllQueues

    do
        queues |> Array.iter (fun q -> q.CollectionChanged.Add(fun _ -> tableView.ReloadData()))

    let itemAt (indexPath : NSIndexPath) = queues.[indexPath.Section].[indexPath.Row]

    let classify (item : QItem) =
        match item.Topic with
        | "Contact" ->
            match Int32.TryParse(item.InternalDetails) with
            | (true, id) when id > -1 -> Contact id
            | _ -> Other item.Topic
        | _ -> Other item.Topic

    let tapAction (item : QItem) =
        if not item.Completed then
            match classify item with
            | Contact id ->
                let ab, _ = ABAddressBook.Create()
                let person = ab.GetPerson( id )
                let name = person.FirstName + " " + person.LastName
                let alert = UIAlertController.Create("Choose Contact Method", name, UIAlertControllerStyle.ActionSheet)

                let numbers = person.GetPhones()
                if int numbers.Count > 0 then
                    let regex = new Regex(@"[^\d]")
                    numbers.GetValues() |> Array.iter (fun n ->
                        alert.AddAction( UIAlertAction.Create("Call: " + n, UIAlertActionStyle.Default, Action<_> (fun action ->
                            UIApplication.SharedApplication.OpenUrl(NSUrl.FromString("tel:" + regex.Replace(n, ""))) |> ignore
                            ) ) )
                        alert.AddAction( UIAlertAction.Create("Text: " + n, UIAlertActionStyle.Default, Action<_> (fun action ->
                            UIApplication.SharedApplication.OpenUrl(NSUrl.FromString("sms:" + regex.Replace(n, ""))) |> ignore
                            ) ) )
                        )

                let emails = person.GetEmails()
                if int emails.Count > 0 then
                    emails.GetValues() |> Array.iter (fun e ->
                        alert.AddAction( UIAlertAction.Create("Email: " + e, UIAlertActionStyle.Default, Action<_> (fun action ->
                            if MFMailComposeViewController.CanSendMail then
                                let mc = new MFMailComposeViewController()
                                mc.SetToRecipients([| e |])
                                mc.SetSubject("Follow-up")
                                mc.Finished.Add(fun e -> e.Controller.DismissViewController(true, null))
                                vc.PresentViewController(mc, true, null)
                            ) ) )
                        )

                alert.AddAction( UIAlertAction.Create("Cancel", UIAlertActionStyle.Cancel, null) )
                vc.PresentViewController(alert, true, null)
            | _ -> ()

    override this.NumberOfSections(tableView) = nint queues.Length
    override this.RowsInSection(tableView, section) = nint queues.[int section].Count
    override this.TitleForHeader(tableView, section) = QLib.QueueNames.[int section]

    override this.AccessoryButtonTapped(tableView, indexPath) =
        tapAction (itemAt indexPath)

    override this.RowSelected(tableView, indexPath) =
        tableView.DeselectRow(indexPath, true)
        tapAction (itemAt indexPath)

    override this.GetCell(tableView, indexPath) =
        let item = itemAt indexPath
        let cell = tableView.DequeueReusableCell (QueueCell.ReuseId, indexPath) :?> QueueCell
        cell.Bind(item)

        cell.CellSwipeGestureRecognizer.ShortTrigger <- nfloat 0.30
        let frameWithWidth width = CGRectWithSize width 60.0

        if indexPath.Section = QLib.Uncompleted then
            cell.Accessory <- match classify item with
                              | Contact _ -> UITableViewCellAccessory.DetailButton
                              | _ -> UITableViewCellAccessory.None
            let completedActionView = new UILabel(frameWithWidth 100.0)
            completedActionView.Text <- "Completed"
            completedActionView.TextColor <- UIColor.White
            completedActionView.TextAlignment <- UITextAlignment.Center
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
        else
            cell.Accessory <- UITableViewCellAccessory.None
            let uncompletedAction = new UILabel(frameWithWidth 110.0)
            uncompletedAction.Text <- "Uncomplete"
            uncompletedAction.TextColor <- UIColor.White
            uncompletedAction.TextAlignment <- UITextAlignment.Center
            cell.SetSwipeGestureWithView(
                uncompletedAction,
                UIColor.QBlue,
                SwipeTableCellMode.Exit,
                SwipeTableViewCellState.StateLeftShort,
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
        tv.Source <- new QueueViewSource(this, tv)
