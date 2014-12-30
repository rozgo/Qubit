module Cortex.Shape

open System
open System.IO
open OpenTK
open OpenTK.Graphics.ES30
open System.Drawing
open Foundation
open FSharp.Control.Reactive
open Cortex.Renderer
open Cortex

let observe = new Builders.ObservableBuilder ()

module private Buffer =

    let bytes<'T> (target:BufferTarget) data (address:int) =
        GL.BindBuffer (target, address)
        GL.BufferData (target, nativeint ((Array.length data) * sizeof<'T>), data, BufferUsage.StaticDraw)
        GL.BindBuffer (target, 0)
    
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
            Buffer.bytes BufferTarget.ArrayBuffer v vbos.[0]
            Buffer.bytes BufferTarget.ArrayBuffer c vbos.[1]
            Buffer.bytes BufferTarget.ArrayBuffer u vbos.[2]
            Buffer.bytes BufferTarget.ElementArrayBuffer t vbos.[3])

    member this.VBOs = vbos

    member this.BindBuffers () =
        GL.BindBuffer (BufferTarget.ElementArrayBuffer, vbos.[3])

    member this.UnbindBuffers () =
        GL.BindBuffer (BufferTarget.ElementArrayBuffer, 0)

    member this.Draw () =
        GL.DrawElementsInstanced (PrimitiveType.Triangles, count, DrawElementsType.UnsignedInt, nativeint 0, 1)

    interface IDisposable with
        member this.Dispose () =
            observer.Dispose ()
            GL.DeleteBuffers (4, vbos)
            Array.Clear (vbos, 0, 4)

let vectors_to_bytes (vs:Vector3 array) =
    let bytes = Array.create<byte> (vs.Length * sizeof<float> * 3) 0uy
    use ms = new MemoryStream (bytes)
    for v in vs do
        ms.Write (System.BitConverter.GetBytes (v.X), 0, 4)
        ms.Write (System.BitConverter.GetBytes (v.Y), 0, 4)
        ms.Write (System.BitConverter.GetBytes (v.Z), 0, 4)
    bytes


type Line (points:IObservable<Vector3 array>, ?colors:IObservable<Vector4 array>) =

    let mutable count = 0

    let vbos = [|0; 0; 0; 0;|]

    let rainbow count =
        seq {
            while true do
                yield Vector4 (1.f, 0.f, 0.f, 1.f)
                yield Vector4 (0.f, 1.f, 0.f, 1.f)
                yield Vector4 (0.f, 0.f, 1.f, 1.f)
                yield Vector4 (1.f, 1.f, 0.f, 1.f)
            }
            |> Seq.take count
            |> Observable.ofSeq

    let observer =
        observe {
            let! points = points
            let rainbow =
                rainbow points.Length
                |> Observable.fold (fun s c -> c :: s) []
                |> Observable.map Array.ofList
            let! colors = defaultArg colors rainbow
            return (points, colors)
        }
        |> Observable.subscribe (fun (points, colors) ->
            GL.DeleteBuffers (4, vbos)
            Array.Clear (vbos, 0, 4)
            GL.GenBuffers (4, vbos)
            count <- points.Length
            //Buffer.bytes BufferTarget.ArrayBuffer points vbos.[0]

            GL.BindBuffer (BufferTarget.ArrayBuffer, vbos.[0])
            GL.BufferData (BufferTarget.ArrayBuffer, nativeint ((Array.length points) * sizeof<Vector3>), points, BufferUsage.StaticDraw)

            GL.BindBuffer (BufferTarget.ArrayBuffer, vbos.[1])
            GL.BufferData (BufferTarget.ArrayBuffer, nativeint ((Array.length colors) * sizeof<Vector4>), colors, BufferUsage.StaticDraw)

            let indices = Array.ofList [0 .. points.Length - 1]
            GL.BindBuffer (BufferTarget.ElementArrayBuffer, vbos.[2])
            GL.BufferData (BufferTarget.ElementArrayBuffer, nativeint ((Array.length indices) * sizeof<int>), indices, BufferUsage.StaticDraw)
            )


    member this.VBOs = vbos

    member this.BindBuffers () =
        GL.LineWidth 5.f
        GL.BindBuffer (BufferTarget.ElementArrayBuffer, vbos.[2])

    member this.UnbindBuffers () =
        GL.BindBuffer (BufferTarget.ElementArrayBuffer, 0)

    member this.Draw () =
        GL.DrawElementsInstanced (PrimitiveType.LineStrip, count, DrawElementsType.UnsignedInt, nativeint 0, 1)

    interface IDisposable with
        member this.Dispose () =
            observer.Dispose ()
            GL.DeleteBuffers (4, vbos)
            Array.Clear (vbos, 0, 4)
