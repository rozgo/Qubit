module Cortex.Mario

open Atom
open Cortex
open OpenTK
open OpenTK.Graphics.ES30
open System
open FSharp.Control.Reactive

type ObservableProperty<'T> (obs:IObservable<'T>, initial:'T) =
    let mutable property = initial
    let disposable = obs.Subscribe (fun x -> property <- x)
    member this.Value = property
    interface IDisposable with
        member this.Dispose () = disposable.Dispose ()
    
let render = new RenderBuilder.Builder ()

let state r = RenderBuilder.State (fun f -> r(); f ())

let texture (tex:Texture.Texture) = state (fun () ->
    GL.ActiveTexture (TextureUnit.Texture0)
    tex.Bind ())

let vybe = Shader.Vybe ()
let prog = state (fun () -> vybe.Use)

let shape (mesh:Shape.Mesh) = state (fun () -> 
    vybe.Attribs mesh.VBOs
    mesh.Draw ())

let actor dt view proj =

    let animation = Observable.map (fun dt -> Matrix4.CreateRotationY dt) dt

    let model = new ObservableProperty<Matrix4> (animation, Matrix4.Identity)

    let model = state (fun () -> vybe.Model model.Value)
    let view  = state (fun () -> vybe.View view)
    let proj  = state (fun () -> vybe.Proj proj)

    let parts = ["Mario/FitMario_BodyB"; "Mario/FitMario_BodyA"; "Mario/FitMario_EyeDmg"; "Mario/FitMario_Kage"]

    let meshes = List.fold (fun meshes part -> 
        new Shape.Mesh (part) :: meshes ) [] parts

    render {

        let! s = prog
        let! m = model
        let! v = view
        let! p = proj

        for mesh in meshes do
            let! t = texture mesh.Tex
            return! shape mesh
    }
