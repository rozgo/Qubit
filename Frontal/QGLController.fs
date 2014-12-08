namespace Cortex

open System
open System.IO
open System.Threading
open System.Drawing
open MonoTouch.UIKit
open OpenTK
open MonoTouch
open MonoTouch.OpenGLES
open MonoTouch.GLKit
open MonoTouch.CoreGraphics
open MonoTouch.Foundation
open OpenTK.Graphics.ES30
open Cortex.Generator

type Actor =
    {
    meshes : Meshes.Mesh.Mesh list
    offset : Vector3
    }

type QGLController =
    inherit GLKViewController

    val mutable context : EAGLContext
    val mutable shader : Shaders.Vybe.Program option
    val mutable actors : Actor list
    val mutable fingers : int array


    new (frame) = //as this =
        {
        inherit GLKViewController

        context = null
        shader = None
        actors = List.empty
        fingers = [| 0; 0; 0; 0; 0; |]
        }

    override this.ViewDidLoad () =
        base.ViewDidLoad ()

        Asset.mainContext <- SynchronizationContext.Current

        this.context <- new EAGLContext (EAGLRenderingAPI.OpenGLES3)
        this.context.IsMultiThreaded <- true
        let view = this.View :?> GLKView
        view.Context <- this.context
        view.DrawableDepthFormat <- GLKViewDrawableDepthFormat.Format24
        view.DrawInRect.Add (this.Draw)
        this.Setup ()

        this.View.MultipleTouchEnabled <- true

        Async.Start Asset.watch

        Touch.Touches |> Observable.add (printfn "%A")


    member this.Setup () =

        EAGLContext.SetCurrentContext this.context |> ignore

        this.LoadShaders ()

        GL.Enable EnableCap.DepthTest

    override this.Update () =
        ()

    member this.Draw (args : GLKViewDrawEventArgs) =

        GL.ClearColor (0.f,0.f,0.f,1.f)
        GL.Clear (ClearBufferMask.ColorBufferBit ||| ClearBufferMask.DepthBufferBit)

        GL.Enable (EnableCap.DepthTest)
        GL.Enable (EnableCap.CullFace)

        let size = this.View.Frame.Size

        let model = Matrix4.CreateRotationY ((single this.TimeSinceFirstResume) * 1.f)//80.0f//this.transY

        //let view = Matrix4.LookAt (-10.f,5.f,-10.f,0.f,0.f,0.f,0.f,1.f,0.f)
        let view = Matrix4.LookAt (0.f,4.f,-20.f,0.f,3.f,0.f,0.f,1.f,0.f)
        let proj =
            Matrix4.CreatePerspectiveFieldOfView (
                45.f * (float32(Math.PI)/180.f), float32(size.Width) / float32(size.Height), 0.3f, 1000.f)

//        this.transY <- this.transY + 0.01f;

        for actor in this.actors do

            match this.shader with
            | Some prog ->

                prog.Use

                prog.Model (model * (Matrix4.CreateTranslation (actor.offset)))
                prog.View view
                prog.Proj proj

                for mesh in actor.meshes do

                    GL.ActiveTexture (TextureUnit.Texture0)
                    mesh.tex.Bind ()
                    prog.Chan0 0
                    prog.Attribs mesh.VBOs

                    mesh.Draw ()

            | None -> ()

    member this.LoadShaders () =

//        let mutable meshes = List.empty<Meshes.Mesh.Mesh>    
//
//        use names = Async.RunSynchronously <| Asset.request ("Link/names.list")
//        let names = (new StreamReader (names)).ReadToEnd ()
//        let names = names.Split ([|Environment.NewLine|], StringSplitOptions.None)
//
//        for name in names do
//           let m = Meshes.Mesh.Mesh ("Link/" + name)
//           meshes <- m :: meshes
//        let actor = {meshes = meshes; offset = Vector3(-3.f,0.f,0.f)}
//        this.actors <- actor :: this.actors

        let mutable meshes = List.empty<Meshes.Mesh.Mesh>
        let buffer =
            Asset.request ("Mario/names.list")
            |> Async.RunSynchronously
        let names = Text.Encoding.ASCII.GetString (buffer)
        let names = names.Split ([|Environment.NewLine|], StringSplitOptions.None)

        for name in names do
           let m = Meshes.Mesh.Mesh ("Mario/" + name)
           meshes <- m :: meshes
        let actor = {meshes = meshes; offset = Vector3(0.f,0.f,0.f)}
        this.actors <- actor :: this.actors

//        let m = Material.test ()
    
        this.shader <- Some (Shaders.Vybe.Program ())


    override this.Dispose disposing =
        base.Dispose disposing
        NSNotificationCenter.DefaultCenter.RemoveObserver (this)

    override this.DidReceiveMemoryWarning () =
        base.DidReceiveMemoryWarning ()

//    override this.ViewWillAppear animated =
//        base.ViewWillAppear animated
//        eaglView.StartAnimating
//
//    override this.ViewWillDisappear animated =
//        base.ViewWillDisappear animated
//        eaglView.StopAnimating

    member this.PushTouches (touches:UITouch[]) phase =
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
            Touch.Generator.Trigger {
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