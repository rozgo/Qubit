module Cortex.Shape

open System
open System.IO
open OpenTK
open OpenTK.Graphics.ES30
open Foundation
open FSharp.Control.Reactive
open Cortex.Renderer
open Cortex

let observe = new Builders.ObservableBuilder ()

module private __ =

    let buffer (target:BufferTarget) (bytes:byte array) (address:int) =
        GL.BindBuffer (target, address)
        GL.BufferData (target, IntPtr (int bytes.Length), bytes, BufferUsage.StaticDraw)
        GL.BindBuffer (target, 0)

open __
    
type Mesh (asset) =

    let mutable count = 0

    let vbos = [|0; 0; 0; 0;|]

    let v = Asset.observe (asset + ".verts")
    let c = Asset.observe (asset + ".colors")
    let u = Asset.observe (asset + ".uvs")
    let t = Asset.observe (asset + ".tris")

    let obs = observe {
        let z = Observable.zipWith (fun c v -> (c, v)) v c
        let z = Observable.zipWith (fun u (c,v) -> (u, c, v)) u z
        return! Observable.zipWith (fun t (u,v,c) -> (t, u, v, c)) t z}

    let observer =
        obs
        |> Observable.subscribe (fun (t, u, v, c) ->
            GL.DeleteBuffers (4, vbos)
            Array.Clear (vbos, 0, 4)
            GL.GenBuffers (4, vbos)
            count <- int t.Length / sizeof<int>
            buffer BufferTarget.ArrayBuffer v vbos.[0]
            buffer BufferTarget.ArrayBuffer c vbos.[1]
            buffer BufferTarget.ArrayBuffer u vbos.[2]
            buffer BufferTarget.ElementArrayBuffer t vbos.[3])

    member this.VBOs = vbos

    member this.BindBuffers () =
        GL.BindBuffer (BufferTarget.ElementArrayBuffer, vbos.[3])

    member this.UnbindBuffers () =
        GL.BindBuffer (BufferTarget.ElementArrayBuffer, 0)

    member this.Draw () =
        GL.DrawElementsInstanced (PrimitiveType.Triangles, count, DrawElementsType.UnsignedInt, nativeint 0, 100)

    interface IDisposable with
        member this.Dispose () =
            observer.Dispose ()
            GL.DeleteBuffers (4, vbos)
            Array.Clear (vbos, 0, 4)
