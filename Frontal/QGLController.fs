namespace Cortex

open System
open System.IO
open MonoTouch.UIKit
open OpenTK
open MonoTouch.OpenGLES
open MonoTouch.GLKit
open MonoTouch
open OpenTK.Graphics.ES30
open MonoTouch.CoreGraphics
open System.Drawing
open MonoTouch.Foundation

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

    new (frame) = //as this =
        {
        inherit GLKViewController

        context = null
        shader = None
        actors = List.empty
        }

    override this.ViewDidLoad () =
        base.ViewDidLoad ()

        this.context <- new EAGLContext (EAGLRenderingAPI.OpenGLES3)
        let view = this.View :?> GLKView
        view.Context <- this.context
        view.DrawableDepthFormat <- GLKViewDrawableDepthFormat.Format24
        view.DrawInRect.Add (this.Draw)
        this.Setup ()

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
