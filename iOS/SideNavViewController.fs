namespace Q.iOS

open System

open UIKit
open Foundation

open Utilites
open qlib

open JASidePanels

open CoreGraphics
open AnimatedCharts

type MetricsCell =
    inherit UITableViewCell

    val private chart : CircleChart

    [<Export("initWithStyle:reuseIdentifier:")>]
    new (style : UITableViewCellStyle, reuseId : string) as this =
        { inherit UITableViewCell(UITableViewCellStyle.Default, reuseId); chart = new CircleChart(new CGRect(30.0, 50.0, 150.0, 150.0), true, 20.0) }
        then
            let lbl = new UILabel(new CGRect(18.0, 0.0, 150.0, 30.0))
            lbl.Text <- "Today's Productivity"
            lbl.Font <- UIFont.FromName(Settings.StyledFontName, nfloat 18.0)
            this.chart.StrokeColor <- UIColor.QBlue
            this.AddSubviews(lbl, this.chart)

    member this.UpdateMetrics(productivity, animate) =
        this.chart.ChangeValue(productivity, animate)

type SideNavViewSource(switchTo) =
    inherit UITableViewSource()

    let nav = [|
        ("Tasks", [|
            ("Today's Agenda", new TaskListViewController(TodayAgenda) :> UIViewController);
            ("All Tasks", new TaskListViewController(AllTasks) :> UIViewController);
            ("Uncompleted", new TaskListViewController(Uncompleted) :> UIViewController);
            ("Completed", new TaskListViewController(Completed) :> UIViewController)
            |]);
        ("", [| ("Settings", new SettingsViewController()  :> UIViewController) |]);
        ("", [| ("Metrics", null) |])
        |]

    let itemAt (indexPath : NSIndexPath) = (snd nav.[indexPath.Section]).[indexPath.Row]

    static member MenuCellReuseId = "NavCellReuseId"
    static member MetricsCellReuseId = "MetricsCellReuseId"

    override this.NumberOfSections(tableView) = nint nav.Length
    override this.RowsInSection(tableView, section) = nint (snd nav.[int section]).Length
    override this.TitleForHeader(tableView, section) = fst nav.[int section]

    override this.GetHeightForRow(tableView, indexPath) =
        match fst (itemAt indexPath) with
        | "Metrics" -> nfloat 200.0
        | _ -> nfloat 40.0

    override this.RowSelected(tableView, indexPath) =
        tableView.DeselectRow(indexPath, true)
        match fst (itemAt indexPath) with
        | "Metrics" -> ()
        | _ -> switchTo(new UINavigationController( snd (itemAt indexPath) ))

    override this.GetCell(tableView, indexPath) =
        let menuItem = fst (itemAt indexPath)
        let cell = match menuItem with
                    | "Metrics" ->
                        let cell = tableView.DequeueReusableCell (SideNavViewSource.MetricsCellReuseId, indexPath) :?> MetricsCell
                        cell.UpdateMetrics(QLib.Productivity, true)
                        cell :> UITableViewCell
                    | _ ->
                        let cell = tableView.DequeueReusableCell (SideNavViewSource.MenuCellReuseId, indexPath)
                        cell.TextLabel.Text <- menuItem
                        cell.TextLabel.Font <- UIFont.FromName(Settings.StyledFontName, nfloat 18.0)
                        cell
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
        tv.RegisterClassForCellReuse(Operators.typeof<UITableViewCell>, SideNavViewSource.MenuCellReuseId)
        tv.RegisterClassForCellReuse(Operators.typeof<MetricsCell>, SideNavViewSource.MetricsCellReuseId)
        tv.SeparatorStyle <- UITableViewCellSeparatorStyle.None

        tv.Source <- new SideNavViewSource(fun newContentVC ->
            this.GetSidePanelController().CenterPanel <- newContentVC
        )


