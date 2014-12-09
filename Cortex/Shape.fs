module Cortex.Shape

open System
open System.IO
open OpenTK
open OpenTK.Graphics.ES30
open MonoTouch.Foundation
open FSharp.Control.Reactive
open Cortex.Renderer
open Cortex

type DataPath = DataPath of string

type Verts = Verts of Vector3 array

type Colors = Colors of byte array
    
type Mesh =

    val mutable vbos : int array
    val mutable count : int
    val mutable tex : Texture.Texture

    new (filename) as this =
        {
        vbos = [| 0; 0; 0; 0; |]
        count = 0
        tex = new Texture.Texture (filename)
        }
        then

            let v = Asset.observe (filename + ".verts")
            let c = Asset.observe (filename + ".colors")
            let u = Asset.observe (filename + ".uvs")
            let t = Asset.observe (filename + ".tris")

            Observable.zipWith (fun c v -> (c, v)) v c
            |> Observable.zipWith (fun u (c,v) -> (u, c, v)) u
            |> Observable.zipWith (fun t (u,v,c) -> (t, u, v, c)) t
            |> Observable.add (fun (t, u, v, c) ->

                GL.DeleteBuffers (4, this.vbos)
                this.vbos <- [| 0; 0; 0; 0; |]
                GL.GenBuffers (4, this.vbos)

                this.count <- int t.Length / sizeof<int>

                GL.BindBuffer (BufferTarget.ArrayBuffer, this.vbos.[0])
                GL.BufferData (BufferTarget.ArrayBuffer,
                    IntPtr (int v.Length),
                    v, BufferUsage.StaticDraw)
                GL.BindBuffer (BufferTarget.ArrayBuffer, 0)
            
                GL.BindBuffer (BufferTarget.ArrayBuffer, this.vbos.[1])
                GL.BufferData (BufferTarget.ArrayBuffer,
                    IntPtr (int c.Length),
                    c, BufferUsage.StaticDraw)
                GL.BindBuffer (BufferTarget.ArrayBuffer, 0)
            
                GL.BindBuffer (BufferTarget.ArrayBuffer, this.vbos.[2])
                GL.BufferData (BufferTarget.ArrayBuffer,
                    IntPtr (int u.Length),
                    u, BufferUsage.StaticDraw)
                GL.BindBuffer (BufferTarget.ArrayBuffer, 0)

                GL.BindBuffer (BufferTarget.ElementArrayBuffer, this.vbos.[3])
                GL.BufferData (BufferTarget.ElementArrayBuffer,
                    IntPtr (int t.Length),
                    t, BufferUsage.StaticDraw)
                GL.BindBuffer (BufferTarget.ElementArrayBuffer, 0))

    member this.VBOs = this.vbos

    member this.Tex = this.tex

    member this.Draw () =

        GL.BindBuffer (BufferTarget.ElementArrayBuffer, this.vbos.[3])
        GL.DrawElements (BeginMode.Triangles, this.count, DrawElementsType.UnsignedInt, IntPtr.Zero)

    interface IDisposable with
        member this.Dispose() =
            GL.DeleteBuffers (4, this.vbos)
            this.vbos <- [| 0; 0; 0; 0; |]












