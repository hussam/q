namespace Q.iOS

open System

open UIKit
open Foundation

open Utilites
open qlib

open JASidePanels

type SideNavViewSource(switchTo) =
    inherit UITableViewSource()

    let nav = [|
        ("Tasks", [|
            ("Today's Agenda", new TaskListViewController(TodayAgenda) :> UIViewController);
            ("All Tasks", new TaskListViewController(AllTasks) :> UIViewController);
            ("Uncompleted", new TaskListViewController(Uncompleted) :> UIViewController);
            ("Completed", new TaskListViewController(Completed) :> UIViewController)
            |]);
        ("", [| ("Settings", new SettingsViewController()  :> UIViewController) |])
        |]

    let itemAt (indexPath : NSIndexPath) = (snd nav.[indexPath.Section]).[indexPath.Row]

    static member CellReuseId = "NavCellReuseId"

    override this.NumberOfSections(tableView) = nint nav.Length
    override this.RowsInSection(tableView, section) = nint (snd nav.[int section]).Length
    override this.TitleForHeader(tableView, section) = fst nav.[int section]

    override this.RowSelected(tableView, indexPath) =
        tableView.DeselectRow(indexPath, true)
        switchTo(new UINavigationController( snd (itemAt indexPath) ))

    override this.GetCell(tableView, indexPath) =
        let cell = tableView.DequeueReusableCell (SideNavViewSource.CellReuseId, indexPath)
        cell.TextLabel.Text <- fst (itemAt indexPath)
        cell.TextLabel.Font <- UIFont.FromName(Settings.StyledFontName, nfloat 18.0)
        cell.BackgroundColor <- UIColor.QBeige
        cell


type SideNavViewController() as this = 
    inherit UITableViewController(UITableViewStyle.Grouped)

    do
        this.Title <- "Queue v0.2"

    // Perform any additional setup after loading the view
    override this.ViewDidLoad() =
        base.ViewDidLoad()
        this.View.BackgroundColor <- UIColor.QBeige
        let tv = this.TableView
        tv.RegisterClassForCellReuse(Operators.typeof<UITableViewCell>, SideNavViewSource.CellReuseId)
        tv.SeparatorStyle <- UITableViewCellSeparatorStyle.None

        tv.Source <- new SideNavViewSource(fun newContentVC ->
            this.GetSidePanelController().CenterPanel <- newContentVC
        )


