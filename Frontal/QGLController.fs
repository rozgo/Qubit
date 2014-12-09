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
open FSharp.Control.Reactive
open System.Reactive.Linq

type Actor =
    {
    meshes : Shape.Mesh list
    offset : Vector3
    }

[<Register("QGLController")>]
type QGLController =
    inherit GLKViewController

    val mutable context : EAGLContext
    val mutable shader : Shader.Vybe option
    val mutable actors : Map<string,Actor>
    val mutable fingers : int array
    val mutable touches : Event<Touch.Touch>

    new (frame) as this =
        {
        inherit GLKViewController ()
        context = null
        shader = None
        actors = Map.empty
        fingers = [| 0; 0; 0; 0; 0; |]
        touches = new Event<Touch.Touch> ()
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
        view.DrawInRect.Add (this.Draw)

        view.MultipleTouchEnabled <- true
        view.UserInteractionEnabled <- true

        this.PreferredFramesPerSecond <- 60

        Async.Start Asset.watching

        this.touches.Publish |> Observable.add (printfn "%A")

        EAGLContext.SetCurrentContext this.context |> ignore
        this.LoadShaders ()

    override this.Update () =
        ()

    member this.Draw (args : GLKViewDrawEventArgs) =

        GL.ClearColor (0.f,0.f,1.f,1.f)
        GL.Clear (ClearBufferMask.ColorBufferBit ||| ClearBufferMask.DepthBufferBit)

        GL.Enable EnableCap.DepthTest
        GL.Enable EnableCap.CullFace

        let size = this.View.Frame.Size

        let model = Matrix4.CreateRotationY ((single this.TimeSinceFirstResume) * 1.f)//80.0f//this.transY

        //let view = Matrix4.LookAt (-10.f,5.f,-10.f,0.f,0.f,0.f,0.f,1.f,0.f)
        let view = Matrix4.LookAt (0.f,4.f,-20.f,0.f,3.f,0.f,0.f,1.f,0.f)
        let proj =
            Matrix4.CreatePerspectiveFieldOfView (
                45.f * (float32(Math.PI)/180.f), float32(size.Width) / float32(size.Height), 0.3f, 1000.f)

//        this.transY <- this.transY + 0.01f;

        for kv in this.actors do

            let actor = kv.Value

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

        let lines buffer =
            let text = Text.Encoding.ASCII.GetString (buffer)
            text.Split ([|Environment.NewLine|], StringSplitOptions.None)

        let link = "Link/names.list"
        Asset.observe link
        |> Observable.add (fun buffer ->
            let names = lines buffer
            let meshes = Array.fold (fun meshes name -> (Shape.Mesh ("Link/" + name)) :: meshes) [] names
            let actor = {meshes = meshes; offset = Vector3(-3.f,0.f,0.f)}
            this.actors <- Map.add link actor this.actors)

        let mario = "Mario/names.list"
        Asset.observe mario
        |> Observable.add (fun buffer ->
            let names = lines buffer
            let meshes = Array.fold (fun meshes name -> (Shape.Mesh ("Mario/" + name)) :: meshes) [] names
            let actor = {meshes = meshes; offset = Vector3(3.f,0.f,0.f)}
            this.actors <- Map.add mario actor this.actors)
    
        this.shader <- Some (Shader.Vybe ())

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
                position = Vector3 (loc.X, loc.Y, 0.0f) }

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
