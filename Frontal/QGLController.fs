namespace Cortex

open System
open System.IO
open System.Threading
open System.Drawing
open UIKit
open OpenTK
open OpenGLES
open GLKit
open Foundation
open OpenTK.Graphics.ES30
open Cortex.Generator
open FSharp.Control.Reactive
open System.Reactive.Linq
open Atom


[<Register("QGLController")>]
type QGLController =
    inherit GLKViewController

    val mutable context : EAGLContext
    val mutable mario : RenderBuilder.State
    val mutable curve : RenderBuilder.State
    val fingers : int array
    val touches : Event<Touch.Touch>
    val deltaTime : Event<single>
    //val mutable

    new (frame) as this =
        {
        inherit GLKViewController ()
        context = null
        mario = RenderBuilder.empty
        curve = RenderBuilder.empty
        fingers = [| 0; 0; 0; 0; 0; |]
        touches = new Event<Touch.Touch> ()
        deltaTime = new Event<single> ()
        }
        then
            this.View.Frame <- frame

    override this.ViewDidLoad () =
        base.ViewDidLoad ()

        Asset.mainContext <- SynchronizationContext.Current

        this.context <- new EAGLContext (EAGLRenderingAPI.OpenGLES3)
        this.context.IsMultiThreaded <- true
        let view = this.View :?> GLKView
        view.Context <- this.context
        view.ContentScaleFactor <- UIScreen.MainScreen.Scale
        view.DrawableDepthFormat <- GLKViewDrawableDepthFormat.Format24
        view.DrawInRect.Add this.Draw

        view.MultipleTouchEnabled <- true
        view.UserInteractionEnabled <- true

        let num : nint = nint.op_Explicit 10L

        this.PreferredFramesPerSecond <- nint.op_Explicit 60L
        Async.Start Asset.watching

        this.touches.Publish |> Observable.add (printfn "%A")

        EAGLContext.SetCurrentContext this.context |> ignore

        let size = this.View.Frame.Size

        let model = Matrix4.CreateRotationY ((single this.TimeSinceFirstResume) * 1.f)//80.0f//this.transY

        //let view = Matrix4.LookAt (-10.f,5.f,-10.f,0.f,0.f,0.f,0.f,1.f,0.f)
        let view = Matrix4.LookAt (0.f,4.f,-40.f,0.f,3.f,0.f,0.f,1.f,0.f)
        let proj =
            Matrix4.CreatePerspectiveFieldOfView (
                45.f * (float32(Math.PI)/180.f), float32(size.Width) / float32(size.Height), 0.3f, 1000.f)

        this.mario <- Mario.actor view proj
        this.curve <- Curve.actor view proj

    override this.Update () =
        Profiler.tick this.TimeSinceLastUpdate
        use p = Profiler.sample "update"
        Axon.trigger Mario.DeltaTimeEvent (single this.TimeSinceFirstResume)
        Axon.trigger Curve.DeltaTimeEvent (single this.TimeSinceFirstResume)
        //this.deltaTime.Trigger (single this.TimeSinceFirstResume)

    member this.Draw (args : GLKViewDrawEventArgs) =
        use p = Profiler.sample "render"

        Threading.Thread.Sleep 8

        GL.ClearColor (0.1f,0.1f,0.1f,1.f)
        GL.Clear (ClearBufferMask.ColorBufferBit ||| ClearBufferMask.DepthBufferBit)

        GL.Enable EnableCap.DepthTest
        GL.Enable EnableCap.CullFace

        RenderBuilder.draw this.mario
        RenderBuilder.draw this.curve

    member this.PushTouches (touches:NSSet) phase =
        let touches = touches.ToArray<UITouch> ()
        let fs = Seq.ofArray this.fingers
        for touch in touches do
            let mutable fingerIdx = -1
            if phase = Touch.Began then
                fingerIdx <- Seq.findIndex (fun i -> i = 0) fs
                this.fingers.[fingerIdx] <- touch.Handle.GetHashCode ()
            else
                fingerIdx <- Seq.findIndex (fun i -> i = touch.Handle.GetHashCode ()) fs
                if phase = Touch.Ended || phase = Touch.Cancelled then
                    this.fingers.[fingerIdx] <- 0
            let loc = touch.LocationInView this.View
            this.touches.Trigger {
                finger = Touch.Finger fingerIdx
                phase = phase
                position = Vector3 (single loc.X, single loc.Y, 0.0f) }

    override this.TouchesBegan (touches, evt) =
        base.TouchesBegan (touches, evt)
        this.PushTouches touches Touch.Began

    override this.TouchesMoved (touches, evt) =
        base.TouchesMoved (touches, evt)
        this.PushTouches touches Touch.Moved

    override this.TouchesEnded (touches, evt) =
        base.TouchesEnded (touches, evt)
        this.PushTouches touches Touch.Ended

    override this.TouchesCancelled (touches, evt) =
        base.TouchesCancelled (touches, evt)
        this.PushTouches touches Touch.Cancelled
