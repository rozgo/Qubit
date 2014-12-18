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

let texture (tex:Texture.Texture2D) = RenderBuilder.State (fun interlude ->
    GL.ActiveTexture (TextureUnit.Texture0)
    tex.Bind ()
    interlude ()
    tex.Unbind ())

let vybe = Shader.Vybe ()

let shape (mesh:Shape.Mesh) = RenderBuilder.State (fun interlude ->
    vybe.BindBuffers mesh.VBOs
    mesh.BindBuffers ()
    interlude ()
    mesh.UnbindBuffers ()
    vybe.UnbindBuffers ())

let draw (mesh:Shape.Mesh) = RenderBuilder.State (fun interlude ->
    mesh.Draw ()
    interlude ())

let state f = RenderBuilder.State (fun interlude ->
    f ()
    interlude ())

let actor view proj =

    let animation =
        Axon.observe DeltaTimeEvent
        |> Observable.map (fun dt -> Matrix4.CreateRotationY dt)

    let model = new ObservableProperty<Matrix4> (animation, Matrix4.Identity)

    let parts = ["Mario/FitMario_BodyB"; "Mario/FitMario_BodyA"; "Mario/FitMario_EyeDmg"; "Mario/FitMario_Kage"]

    let meshes = List.fold (fun meshes part -> 
        (new Shape.Mesh (part), new Texture.Texture2D (part)) :: meshes ) [] parts

    let shader = vybe

    let properties () = 
        shader.Use
        shader.Model model.Value
        shader.View view
        shader.Proj proj

    render {

        let! s = state properties

        for (mesh, tex) in meshes do
            let! t = texture tex
            let! b = shape mesh
            return! draw mesh
    }
