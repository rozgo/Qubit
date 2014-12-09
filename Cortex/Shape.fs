module Cortex.Shape

open System
open System.IO
open OpenTK
open OpenTK.Graphics.ES30
open MonoTouch.Foundation
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
        tex = Texture.fromRemote (filename + ".png")
        }
        then

            GL.GenBuffers (4, this.vbos)

            let vertsData =
                Asset.request (filename + ".verts")
                |> Async.RunSynchronously

            let colorsData =
                Asset.request (filename + ".colors")
                |> Async.RunSynchronously

            let uvsData = 
                Asset.request (filename + ".uvs")
                |> Async.RunSynchronously

            let trisData =
                Asset.request (filename + ".tris")
                |> Async.RunSynchronously

            this.count <- int trisData.Length / sizeof<int>

            GL.BindBuffer (BufferTarget.ArrayBuffer, this.vbos.[0])
            GL.BufferData (BufferTarget.ArrayBuffer,
                IntPtr (int vertsData.Length),
                vertsData, BufferUsage.StaticDraw)
            GL.BindBuffer (BufferTarget.ArrayBuffer, 0)
        
            GL.BindBuffer (BufferTarget.ArrayBuffer, this.vbos.[1])
            GL.BufferData (BufferTarget.ArrayBuffer,
                IntPtr (int colorsData.Length),
                colorsData, BufferUsage.StaticDraw)
            GL.BindBuffer (BufferTarget.ArrayBuffer, 0)
        
            GL.BindBuffer (BufferTarget.ArrayBuffer, this.vbos.[2])
            GL.BufferData (BufferTarget.ArrayBuffer,
                IntPtr (int uvsData.Length),
                uvsData, BufferUsage.StaticDraw)
            GL.BindBuffer (BufferTarget.ArrayBuffer, 0)

            GL.BindBuffer (BufferTarget.ElementArrayBuffer, this.vbos.[3])
            GL.BufferData (BufferTarget.ElementArrayBuffer,
                IntPtr (int trisData.Length),
                trisData, BufferUsage.StaticDraw)
            GL.BindBuffer (BufferTarget.ElementArrayBuffer, 0)

    member this.VBOs = this.vbos

    member this.Tex = this.tex

    member this.Draw () =

        GL.BindBuffer (BufferTarget.ElementArrayBuffer, this.vbos.[3])
        GL.DrawElements (BeginMode.Triangles, this.count, DrawElementsType.UnsignedInt, IntPtr.Zero)















