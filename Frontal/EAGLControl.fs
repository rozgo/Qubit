namespace Cortex

open System;
open OpenTK;
open OpenTK.Graphics.ES20
open OpenTK.Platform.iPhoneOS
open MonoTouch.Foundation
open MonoTouch.CoreAnimation
open MonoTouch.ObjCRuntime
open MonoTouch.OpenGLES
open MonoTouch.UIKit
open System.Drawing

open Cortex.Generators

type EAGLControl (frame:RectangleF) as this =
    inherit UIViewController ()

    let fingers = [| 0; 0; 0; 0; 0; |]
    let fs = Seq.ofArray fingers

//    let viewport = RectangleF(0.0f, 0.0f, frame.Width, frame.Width)
//    let viewrect = RectangleF((frame.Width - frame.Width*0.75f)/2.0f, 200.0f, frame.Width*0.75f, frame.Width *0.75f * 0.5625f)

    let eaglView = new EAGLView (frame)
    do
        this.View.AddSubview eaglView
        this.View.Frame <- frame
        printfn "Size %A" frame

    override this.ViewDidLoad () =
        base.ViewDidLoad ()
        eaglView.MultipleTouchEnabled <- true
        NSNotificationCenter.DefaultCenter.AddObserver (UIApplication.WillResignActiveNotification,
            (fun notification -> if this.IsViewLoaded && eaglView.Window <> null then eaglView.StopAnimating),
            this) |> ignore
        NSNotificationCenter.DefaultCenter.AddObserver (UIApplication.DidBecomeActiveNotification,
            (fun notification -> if this.IsViewLoaded && eaglView.Window <> null then eaglView.StartAnimating),
            this) |> ignore
        NSNotificationCenter.DefaultCenter.AddObserver (UIApplication.WillTerminateNotification,
            (fun notification -> if this.IsViewLoaded && eaglView.Window <> null then eaglView.StopAnimating),
            this) |> ignore

    override this.Dispose disposing =
        base.Dispose disposing
        NSNotificationCenter.DefaultCenter.RemoveObserver (this)

    override this.DidReceiveMemoryWarning () =
        base.DidReceiveMemoryWarning ()

    override this.ViewWillAppear animated =
        base.ViewWillAppear animated
        eaglView.StartAnimating

    override this.ViewWillDisappear animated =
        base.ViewWillDisappear animated
        eaglView.StopAnimating

    member this.PushTouches (touches:UITouch[]) phase =
        for touch in touches do
            let mutable fingerIdx = -1
            if phase = Touch.Began then
                fingerIdx <- Seq.findIndex (fun i -> i = 0) fs
                fingers.[fingerIdx] <- touch.Handle.GetHashCode ()
            else
                fingerIdx <- Seq.findIndex (fun i -> i = touch.Handle.GetHashCode ()) fs
                if phase = Touch.Ended || phase = Touch.Cancelled then
                    fingers.[fingerIdx] <- 0
            let loc = touch.LocationInView eaglView
            Touch.Touches.Next {
                finger = Touch.Finger fingerIdx
                phase = phase
                position = Vector3 (loc.X, loc.Y, 0.0f) }

    override this.TouchesBegan (touches, evt) =
        base.TouchesBegan (touches, evt)
        this.PushTouches (touches.ToArray<UITouch>()) Touch.Began

    override this.TouchesMoved (touches, evt) =
        base.TouchesMoved (touches, evt)
        this.PushTouches (touches.ToArray<UITouch>()) Touch.Moved

    override this.TouchesEnded (touches, evt) =
        base.TouchesEnded (touches, evt)
        this.PushTouches (touches.ToArray<UITouch>()) Touch.Ended

    override this.TouchesCancelled (touches, evt) =
        base.TouchesCancelled (touches, evt)
        this.PushTouches (touches.ToArray<UITouch>()) Touch.Cancelled
    
