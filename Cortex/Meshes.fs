namespace Cortex.Meshes

open System
open System.IO
open System.Runtime.InteropServices
open OpenTK
open OpenTK.Graphics.ES20
open MonoTouch.Foundation
open Cortex.Renderer
open Cortex

module Mesh =

    type DataPath = DataPath of string

    type Verts = Verts of Vector3 array

    type Colors = Colors of byte array
        
    type Mesh =

        val mutable vbos : int array
        val mutable count : int
        val mutable tex : Texture2D.Texture2D option

        new (filename) as this =
            {
            vbos = [| 0; 0; 0; 0; |]
            count = 0
            tex = Texture2D.fromFile filename
            }
            then

                GL.GenBuffers (4, this.vbos)

                let vertsPath = NSBundle.MainBundle.PathForResource (filename, "verts")
                let vertsData = NSData.FromFile (vertsPath)

                let mutable verts = Array.zeroCreate<byte> (int vertsData.Length)
                Marshal.Copy (vertsData.Bytes, verts, 0, int vertsData.Length)

                let colorsPath = NSBundle.MainBundle.PathForResource (filename, "colors")
                let colorsData = NSData.FromFile (colorsPath)

                let uvsPath = NSBundle.MainBundle.PathForResource (filename, "uvs")
                let uvsData = NSData.FromFile (uvsPath)

                let trisPath = NSBundle.MainBundle.PathForResource (filename, "tris")
                let trisData = NSData.FromFile (trisPath)

//                printfn "%A %A" vertsPath vertsData.Length
//                printfn "%A %A" uvsPath uvsData.Length
//                printfn "%A %A" trisPath trisData.Length

                this.count <- int trisData.Length / sizeof<int>

                GL.BindBuffer (BufferTarget.ArrayBuffer, this.vbos.[0])
                GL.BufferData (BufferTarget.ArrayBuffer,
                    IntPtr (int vertsData.Length),
                    verts, BufferUsage.StaticDraw)
                GL.BindBuffer (BufferTarget.ArrayBuffer, 0)
            
                GL.BindBuffer (BufferTarget.ArrayBuffer, this.vbos.[1])
                GL.BufferData (BufferTarget.ArrayBuffer,
                    IntPtr (int colorsData.Length),
                    colorsData.Bytes, BufferUsage.StaticDraw)
                GL.BindBuffer (BufferTarget.ArrayBuffer, 0)
            
                GL.BindBuffer (BufferTarget.ArrayBuffer, this.vbos.[2])
                GL.BufferData (BufferTarget.ArrayBuffer,
                    IntPtr (int uvsData.Length),
                    uvsData.Bytes, BufferUsage.StaticDraw)
                GL.BindBuffer (BufferTarget.ArrayBuffer, 0)

                GL.BindBuffer (BufferTarget.ElementArrayBuffer, this.vbos.[3])
                GL.BufferData (BufferTarget.ElementArrayBuffer,
                    IntPtr (int trisData.Length),
                    trisData.Bytes, BufferUsage.StaticDraw)
                GL.BindBuffer (BufferTarget.ElementArrayBuffer, 0)

        member this.VBOs = this.vbos

        member this.Tex = this.tex

        member this.Draw () =
            GL.BindBuffer (BufferTarget.ElementArrayBuffer, this.vbos.[3])
            GL.DrawElements (BeginMode.Triangles, this.count, DrawElementsType.UnsignedInt, IntPtr.Zero)















