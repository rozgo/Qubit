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
    
type Mesh =

    val mutable vbos : int array
    val mutable count : int
    val mutable tex : Texture.Texture
    val mutable observer : IDisposable

    new (filename) as this =
        {
        vbos = [|0; 0; 0; 0;|]
        count = 0
        tex = new Texture.Texture (filename)
        observer = null
        }
        then
            let v = Asset.observe (filename + ".verts")
            let c = Asset.observe (filename + ".colors")
            let u = Asset.observe (filename + ".uvs")
            let t = Asset.observe (filename + ".tris")

            let obs = observe {
                let z0 = Observable.zipWith (fun c v -> (c, v)) v c
                let z1 = Observable.zipWith (fun u (c,v) -> (u, c, v)) u z0
                return! Observable.zipWith (fun t (u,v,c) -> (t, u, v, c)) t z1}

            this.observer <- obs
            |> Observable.subscribe (fun (t, u, v, c) ->
                GL.DeleteBuffers (4, this.vbos)
                this.vbos <- [|0; 0; 0; 0;|]
                GL.GenBuffers (4, this.vbos)
                this.count <- int t.Length / sizeof<int>
                buffer BufferTarget.ArrayBuffer v this.vbos.[0]
                buffer BufferTarget.ArrayBuffer c this.vbos.[1]
                buffer BufferTarget.ArrayBuffer u this.vbos.[2]
                buffer BufferTarget.ElementArrayBuffer t this.vbos.[3])

    member this.VBOs = this.vbos
    member this.Tex = this.tex

    member this.Draw () =
        GL.BindBuffer (BufferTarget.ElementArrayBuffer, this.vbos.[3])
        GL.DrawElements (BeginMode.Triangles, this.count, DrawElementsType.UnsignedInt, IntPtr.Zero)

    interface IDisposable with
        member this.Dispose () =
            this.observer.Dispose ()
            GL.DeleteBuffers (4, this.vbos)
            this.vbos <- [|0; 0; 0; 0;|]
