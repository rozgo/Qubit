namespace Cortex

open System
open MonoTouch.UIKit
open MonoTouch.Foundation

type AppControl (frame:Drawing.RectangleF, eagl:EAGLControl, web:WebFrontControl) as this =
    inherit UIViewController ()

    do
        this.View.Frame <- frame

        this.View.AddSubview eagl.View
        this.View.AddSubview web.View

    override this.ViewDidLoad () =
        base.ViewDidLoad ()


[<Register ("AppDelegate")>]
type AppDelegate () =
    inherit UIApplicationDelegate ()

    let window = new UIWindow (UIScreen.MainScreen.Bounds)

    let eaglControl = new EAGLControl (window.Frame)
    let webControl = new WebFrontControl (window.Frame)
    let appControl = new AppControl (window.Frame, eaglControl, webControl)

    override this.FinishedLaunching (app, options) =
        window.RootViewController <- appControl
        //window.Add eaglControl.View
        //window.BackgroundColor <- UIColor.FromRGB(1.0f,0.0f,0.0f)
        window.MakeKeyAndVisible ()
        //React.Test
        //React.AsyncTest
        true

module Main =
    [<EntryPoint>]
    let main args =
        UIApplication.Main (args, null, "AppDelegate")
        0

