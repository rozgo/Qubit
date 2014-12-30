module Cortex.Curve

open Atom
open Cortex
open OpenTK
open OpenTK.Graphics.ES30
open System
open FSharp.Control.Reactive

type FrontalEvents =
    | DeltaTimeEvent
    
let render = new RenderBuilder.Builder ()

let texture (tex:Texture.Texture2D) = RenderBuilder.State (fun interlude ->
    GL.ActiveTexture (TextureUnit.Texture0)
    tex.Bind ()
    interlude ()
    tex.Unbind ())

let shader = Shader.Line ()

let program = RenderBuilder.State (fun interlude ->
    shader.Bind ()
    interlude ()
    shader.Unbind ())

let shape (line:Shape.Line) = RenderBuilder.State (fun interlude ->
    shader.BindBuffers line.VBOs
    line.BindBuffers ()
    interlude ()
    line.UnbindBuffers ()
    shader.UnbindBuffers ())

let draw (line:Shape.Line) = RenderBuilder.State (fun interlude ->
    line.Draw ()
    interlude ())

let state f = RenderBuilder.State (fun interlude ->
    f ()
    interlude ())

let bezierPoint (p0:Vector3) (p1:Vector3) (p2:Vector3) (p3:Vector3) t =
  let u = 1.f - t
  let tt = t * t
  let uu = u * u
  let uuu = uu * u
  let ttt = tt * t
 
  let p = uuu * p0
  let p = p + 3.f * uu * t * p1
  let p = p + 3.f * u * tt * p2
  p + ttt * p3

let observe = new Builders.ObservableBuilder ()

let actor view proj =

    let dt =
        Axon.observe<FrontalEvents,single> DeltaTimeEvent
        |> Observable.map (fun dt -> float dt)

    let white =
        Observable.repeatCount Vector4.One 4
        |> Observable.fold (fun s c -> c :: s) []
        |> Observable.map Array.ofList

    let controls = observe {
        let! dt = dt
        return [| Vector3(-10.f,0.f,0.f); Vector3(-5.f,5.f * (single (Math.Sin (dt))),0.f); Vector3(5.f,-2.f,0.f); Vector3(10.f,0.f,0.f);|]
    }

    let points = observe {
        let! controls = controls
        let bezier = bezierPoint controls.[0] controls.[1] controls.[2] controls.[3]
        return Array.map bezier [|0.f .. 0.01f .. 1.f|]
    }

    let control = new Shape.Line (controls, white)
    let curve = new Shape.Line (points)

    let properties () = 
        shader.Model (Matrix4.CreateRotationY 0.f)
        shader.View view
        shader.Proj proj

    render {

        let! s = program
        let! s = state properties

        let! b = shape control
        let! c = draw control

        let! b = shape curve
        return! draw curve


        }

