namespace Cortex

open System
open System.IO
open System.Collections.Generic
open System.Linq

open OpenTK
open OpenTK.Graphics.ES20
open OpenTK.Platform.iPhoneOS
open MonoTouch.Foundation
open MonoTouch.CoreAnimation
open MonoTouch.ObjCRuntime
open MonoTouch.OpenGLES
open MonoTouch.UIKit
open System.Drawing

open Cortex.Generators



type Actor =
    {
    meshes : Meshes.Mesh.Mesh list
    offset : Vector3
    }
        
type EAGLView =
    inherit iPhoneOSGameView

    val mutable frameInterval : int
    val mutable isAnimating : bool
    val mutable displayLink : CADisplayLink
    val mutable transY : single
    val mutable time : float
    val mutable startTime : float
    val mutable tex0 : Texture2D.Texture2D option
    val mutable tex1 : Texture2D.Texture2D option
    val mutable tex2 : Texture2D.Texture2D option
    val mutable tex3 : Texture2D.Texture2D option
    val mutable shader : Shaders.Vybe.Program option
    val mutable uniforms : int[]
//    val mutable mesh : Meshes.Cube.Cube option
    val mutable frameBuffers : int[]
    val mutable renderBuffers : int[]
    val mutable touches : IObservable<Touch.Touch>
    val mutable actors : Actor list

    new (frame) = //as this =
        {
        inherit iPhoneOSGameView(frame : Drawing.RectangleF)
        frameInterval = 0
        isAnimating = false
        displayLink = null
        transY = 0.f
        startTime = -1.0
        time = 0.0
        tex0 = None
        tex1 = None
        tex2 = None
        tex3 = None
        shader = None
        uniforms = [| 0; 0; 0; |]
        frameBuffers = [| 0; |]
        renderBuffers = [| 0; 0; 0; |]
        touches = Touch.Touches.AsObservable
        actors = List.empty
        }
        then
            base.LayerRetainsBacking <- true
            base.LayerColorFormat <- EAGLColorFormat.RGBA8
            base.ContextRenderingApi <- EAGLRenderingAPI.OpenGLES2
//            this.wsc.OnConnect <- WebSocketClient.WebSocketClient.Action (fun () ->
//                Console.WriteLine("CONNECTED")
//                this.wsc.Send ("hello"))
//            this.wsc.Connect ()

    [<Export ("layerClass")>]
    static member GetLayerClass () =
        iPhoneOSGameView.GetLayerClass ()

    override this.ConfigureLayer eaglLayer =
        eaglLayer.Frame <- RectangleF(0.0f,0.0f,(single eaglLayer.Frame.Size.Width), (single eaglLayer.Frame.Size.Height))
        eaglLayer.ContentsScale <- 2.0f
        eaglLayer.Opaque <- true

    override this.CreateFrameBuffer () =
        base.CreateFrameBuffer ()

        GL.GenRenderbuffers (1, this.renderBuffers)
        GL.BindRenderbuffer (RenderbufferTarget.Renderbuffer, this.renderBuffers.[0])

        GL.RenderbufferStorage (
            RenderbufferTarget.Renderbuffer,
            RenderbufferInternalFormat.DepthComponent16,
            this.Size.Width*2, this.Size.Height*2)

        GL.FramebufferRenderbuffer (
            FramebufferTarget.Framebuffer,
            FramebufferSlot.DepthAttachment,
            RenderbufferTarget.Renderbuffer,
            this.renderBuffers.[0])

        let status = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer)
        Console.WriteLine ("CheckFramebufferStatus: {0}", status)

//        GL.BindRenderbuffer (RenderbufferTarget.Renderbuffer, 0)
//        GL.BindFramebuffer (FramebufferTarget.Framebuffer, 0)
//
//        GL.BindFramebuffer (FramebufferTarget.Framebuffer, this.Framebuffer)

        this.LoadShaders () |> ignore

//        GL.ActiveTexture (TextureUnit.Texture0)
//        GL.ActiveTexture (TextureUnit.Texture1)
//        GL.ActiveTexture (TextureUnit.Texture2)
//        GL.ActiveTexture (TextureUnit.Texture3)

//        let dt = Generators.Time.DeltaTime.AsObservable
//
//        dt |> Observable.add (printfn "observable dt: %f")
//
//        for p in Microsoft.FSharp.Reflection.FSharpType.GetRecordFields (( Scene.pc ).GetType()) do
//            Console.WriteLine("{0}", p)

    override this.DestroyFrameBuffer () =
        base.DestroyFrameBuffer ()
        GL.DeleteRenderbuffers(1, this.renderBuffers)

    member this.IsAnimating
        with get () = this.isAnimating
        and set (value) = this.isAnimating <- value

    member this.FrameInterval
        with get () = this.frameInterval
        and set (value) =
            this.frameInterval <- value
            if this.IsAnimating then
                this.StopAnimating
                this.StartAnimating

    member this.StartAnimating =
        match this.IsAnimating with
        | false ->
            this.CreateFrameBuffer ()
            this.displayLink <- UIScreen.MainScreen.CreateDisplayLink (fun () -> this.DrawFrame())
            this.displayLink.FrameInterval <- this.frameInterval
            this.displayLink.AddToRunLoop (NSRunLoop.Current, NSRunLoop.NSDefaultRunLoopMode)
            this.IsAnimating <- true
        | _ -> ()

    member this.StopAnimating =
        match this.IsAnimating with
        | true ->
            this.displayLink.Invalidate ()
            this.displayLink <- null
            this.DestroyFrameBuffer ()
            this.IsAnimating <- false
        | false -> ()

    member this.DrawFrame () =
        let time =
            (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds / 1000.0
        Generators.Time.Global._Time <- this.time
        if this.startTime < 0.0 then
            this.startTime <- time
            this.touches |> Observable.add (printfn "%A")
        this.time <- time - this.startTime
        this.OnRenderFrame (new FrameEventArgs ())
    
    override this.OnRenderFrame event =
        base.OnRenderFrame (event);

        //printfn "deltaTime %f  time: %f" Generators.Time.Global.DeltaTime Generators.Time.Global.Time

        //this.MakeCurrent ()

        //GL.ClearColor (0.8f,0.5f,0.5f,1.f)
        GL.ClearColor (0.f,0.f,0.f,1.f)
        GL.Clear (ClearBufferMask.ColorBufferBit ||| ClearBufferMask.DepthBufferBit)

        GL.Enable (EnableCap.DepthTest)
        //GL.Enable (EnableCap.CullFace)

        GL.Viewport (0, 0, this.Size.Width*2, this.Size.Height*2)

        let model = Matrix4.CreateRotationY this.transY

        //let view = Matrix4.LookAt (-10.f,5.f,-10.f,0.f,0.f,0.f,0.f,1.f,0.f)
        let view = Matrix4.LookAt (0.f,4.f,-20.f,0.f,3.f,0.f,0.f,1.f,0.f)
        let proj =
            Matrix4.CreatePerspectiveFieldOfView (
                45.f * (float32(Math.PI)/180.f), float32(this.Size.Width) / float32(this.Size.Height), 0.3f, 1000.f)

        this.transY <- this.transY + 0.01f;
                
        for actor in this.actors do

            match this.shader with
            | Some prog ->

                prog.Use

                prog.Model (model * (Matrix4.CreateTranslation(actor.offset)))
                prog.View view
                prog.Proj proj

                for mesh in actor.meshes do

                    match mesh.Tex with
                    | Some tex ->
                        GL.ActiveTexture (TextureUnit.Texture0)
                        tex.Use ()
                        prog.Chan0 0
                    | None -> ()

                    prog.Attribs mesh.VBOs

                    mesh.Draw ()

            | None -> ()

        GL.BindBuffer (BufferTarget.ArrayBuffer, 0)
        GL.BindBuffer (BufferTarget.ElementArrayBuffer, 0)
        GL.BindTexture (TextureTarget.Texture2D, 0)

        GL.UseProgram 0

        this.SwapBuffers ()

    member this.LoadShaders () =

        

        let mutable meshes = List.empty<Meshes.Mesh.Mesh>    
        let namesPath = NSBundle.MainBundle.PathForResource ("Link/names", "list")
        let names = File.ReadLines (namesPath)
        for name in names do
           let m = Meshes.Mesh.Mesh ("Link/" + name)
           meshes <- m :: meshes
        let actor = {meshes = meshes; offset = Vector3(-3.f,0.f,0.f)}
        this.actors <- actor :: this.actors

        let mutable meshes = List.empty<Meshes.Mesh.Mesh>    
        let namesPath = NSBundle.MainBundle.PathForResource ("Mario/names", "list")
        let names = File.ReadLines (namesPath)
        for name in names do
           let m = Meshes.Mesh.Mesh ("Mario/" + name)
           meshes <- m :: meshes
        let actor = {meshes = meshes; offset = Vector3(3.f,0.f,0.f)}
        this.actors <- actor :: this.actors

//        let writer = new StringWriter ()
//        //var obj = new[] { "foo", null, "bar" };
//
//        let s = YamlDotNet.Serialization.Serializer()
//        s.Serialize (writer, this.actors)
//        let dump = writer.ToString ()
//
//
//        //let dump = Yaml.dump {name = "a thing"}
//        printfn "YAML: %s" dump

        let m = Material.test ()
    
        this.shader <- Some (Shaders.Vybe.Program ())

