﻿namespace Q.iOS

open System
open System.IO
open UIKit
open Foundation

open qlib

[<Register("AppDelegate")>]
type AppDelegate() = 
    inherit UIApplicationDelegate()

    override val Window = null with get, set

    override this.WillFinishLaunching(app, options) =
        let appSupportDir = NSFileManager.DefaultManager.GetUrls(NSSearchPathDirectory.ApplicationSupportDirectory, NSSearchPathDomain.User).[0].Path

        #if __IOS__
        printfn "DB Path: %s" appSupportDir
        #endif

        Directory.CreateDirectory(appSupportDir) |> ignore
        QLib.Init(appSupportDir) |> ignore
        true

    // This method is invoked when the application is ready to run.
    override this.FinishedLaunching(app, options) = 
        this.Window <- new UIWindow(UIScreen.MainScreen.Bounds)
        this.Window.RootViewController <- new UINavigationController(new TodayViewController())
        this.Window.MakeKeyAndVisible()
        true

module Main = 
    [<EntryPoint>]
    let main args = 
        UIApplication.Main(args, null, "AppDelegate")
        0

