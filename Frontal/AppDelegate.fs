namespace Cortex

open System
open MonoTouch.UIKit
open MonoTouch.Foundation

//type AppControl (frame:Drawing.RectangleF, eagl:QGLController) as this =
//    inherit UIViewController ()
//
//    do
//        this.View.Frame <- frame
//
//        this.View.MultipleTouchEnabled <- true
//        this.View.UserInteractionEnabled <- true
//
//        this.View.AddSubview eagl.View
////        this.View.AddSubview web.View
//        this.AddChildViewController eagl
//
//    override this.ViewDidLoad () =
//        base.ViewDidLoad ()


//type AppControl (frame:Drawing.RectangleF, eagl:QGLController) as this =
//    inherit UIViewController ()


[<Register ("AppDelegate")>]
type AppDelegate () =
    inherit UIApplicationDelegate ()

    let window = new UIWindow (UIScreen.MainScreen.Bounds)

    let eaglControl = new QGLController (window.Frame)
//    let webControl = new WebFrontControl (window.Frame)
//    let appControl = new AppControl (window.Frame, eaglControl)

    override this.FinishedLaunching (app, options) =
//        window.Frame <- UIScreen.MainScreen.Bounds
        window.RootViewController <- eaglControl
        window.BackgroundColor <- UIColor.Red
        window.UserInteractionEnabled <- true
//        this.la
        window.MakeKeyAndVisible ()
        true

module Main =
    [<EntryPoint>]
    let main args =
        UIApplication.Main (args, null, "AppDelegate")
        0

