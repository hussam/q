namespace Q.iOS

open System
open System.IO
open UIKit
open Foundation

open Xamarin
open JASidePanels

open qlib

[<Register("AppDelegate")>]
type AppDelegate() = 
    inherit UIApplicationDelegate()

    override val Window = null with get, set

    override this.WillFinishLaunching(app, options) =
        let appSupportDir = NSFileManager.DefaultManager.GetUrls(NSSearchPathDirectory.ApplicationSupportDirectory, NSSearchPathDomain.User).[0].Path

        #if DEBUG
        printfn "DB Path: %s" appSupportDir
        #endif

        Directory.CreateDirectory(appSupportDir) |> ignore
        QLib.Init(appSupportDir)
        true

    // This method is invoked when the application is ready to run.
    override this.FinishedLaunching(app, options) = 
        this.Window <- new UIWindow(UIScreen.MainScreen.Bounds)

        let root = new JASidePanelController()
        root.LeftPanel <- new SideNavViewController()
        root.CenterPanel <- new UINavigationController( new TaskListViewController(AllTasks) )

        this.Window.RootViewController <- root
        this.Window.MakeKeyAndVisible()
        true

module Main = 
    [<EntryPoint>]
    let main args =
        #if SIMULATOR
        Insights.Initialize(Insights.DebugModeKey)
        #else
        Insights.Initialize("f0f8669df33ff445f872a6c8a7704509eab6d773")
        #endif
        UIApplication.Main(args, null, "AppDelegate")
        0

