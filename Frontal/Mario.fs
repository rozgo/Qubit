module Cortex.Mario

open Atom
open Cortex
open OpenTK
open OpenTK.Graphics.ES30
open System
open FSharp.Control.Reactive

type FrontalEvents =
    | DeltaTimeEvent

type ObservableProperty<'T> (obs:IObservable<'T>, initial:'T) =
    let mutable property = initial
    let disposable = obs.Subscribe (fun x -> property <- x)
    member this.Value = property
    interface IDisposable with
        member this.Dispose () = disposable.Dispose ()
    
let render = new RenderBuilder.Builder ()

let def r = RenderBuilder.State (fun f -> r(); f ())

let bindTexture (tex:Texture.Texture2D) = RenderBuilder.State (fun interlude ->
    GL.ActiveTexture (TextureUnit.Texture0)
    tex.Bind ()
    interlude ()
    tex.Unbind ())

let vybe = Shader.Vybe ()

let bindBuffers (mesh:Shape.Mesh) = RenderBuilder.State (fun interlude ->
    vybe.BindBuffers mesh.VBOs
    mesh.BindBuffers ()
    interlude ()
    mesh.UnbindBuffers ()
    vybe.UnbindBuffers ())

let draw (mesh:Shape.Mesh) = RenderBuilder.State (fun interlude ->
    mesh.Draw ()
    interlude ())

let actor view proj =

    let animation = Observable.map (fun dt -> Matrix4.CreateRotationY dt) (Axon.observe DeltaTimeEvent)
    let model = new ObservableProperty<Matrix4> (animation, Matrix4.Identity)

    let parts = ["Mario/FitMario_BodyB"; "Mario/FitMario_BodyA"; "Mario/FitMario_EyeDmg"; "Mario/FitMario_Kage"]

    let meshes = List.fold (fun meshes part -> 
        (new Shape.Mesh (part), new Texture.Texture2D (part)) :: meshes ) [] parts

    render {

        let! prog = def (fun () -> vybe.Use)
        let! model = def (fun () -> vybe.Model model.Value)
        let! view = def (fun () -> vybe.View view)
        let! proj = def (fun () -> vybe.Proj proj)

        for (mesh, tex) in meshes do
            let! t = bindTexture tex
            let! b = bindBuffers mesh
            return! draw mesh
    }
