module Cortex.Shader

open System
open OpenTK
open OpenTK.Graphics.ES30
open Foundation
open Cortex.Renderer
open FSharp.Control.Reactive

module private __ =

    let validate vertShader fragShader progShader vertPath fragPath =
        match vertShader, fragShader, progShader with
        | FailedVertShader info, _, _ -> 
            printfn "%A" info
            printfn "\tin %s" vertPath
            false
        | _, FailedFragShader info, _ -> 
            printfn "%A" info
            printfn "\tin %s" fragPath
            false
        | _, _, FailedProgShader info -> 
            printfn "%A" info
            printfn "\tin %s" vertPath
            printfn "\tin %s" fragPath
            false
        | _ -> true

    let binToAscii bin = Text.Encoding.ASCII.GetString bin

open __

type Vybe =

    val position : int
    val color : int
    val uv : int

    val mutable model : int
    val mutable view : int
    val mutable proj : int
    val mutable chan0 : int

    val mutable vertShader : VertShader
    val mutable fragShader : FragShader
    val mutable progShader : ProgShader

    new () as this =
        {
            position = 0
            color = 1
            uv = 2
            model = 0
            view = 0
            proj = 0
            chan0 = 0
            vertShader = FailedVertShader NoShaderInfo
            fragShader = FailedFragShader NoShaderInfo
            progShader = FailedProgShader NoShaderInfo
        }
        then

            let Bind progId =
                GL.BindAttribLocation (progId, this.position, "position")
                GL.BindAttribLocation (progId, this.color, "color")
                GL.BindAttribLocation (progId, this.uv, "uv")

            let vs =
                Asset.observe ("Shaders/Vybe.vsh")
                |> Observable.map binToAscii

            let fs =
                Asset.observe ("Shaders/Vybe.fsh")
                |> Observable.map binToAscii

            Observable.zip vs fs
            |> Observable.add (fun (vs,fs) ->
                this.vertShader <- VertShader vs
                this.fragShader <- FragShader fs
                this.progShader <- ProgShader this.vertShader this.fragShader Bind
                match this.progShader with
                | LinkedProgShader _ ->
                    this.model <- GetUniformLocation this.progShader "model"
                    this.view <- GetUniformLocation this.progShader "view"
                    this.proj <- GetUniformLocation this.progShader "proj"
                    this.chan0 <- GetUniformLocation this.progShader "chan0"
                | _ ->
                    printfn "%A" this.vertShader
                    printfn "%A" this.fragShader
                    printfn "%A" this.progShader
                )

    member this.Model value =
        match this.progShader with
        | LinkedProgShader _ ->
            GL.UniformMatrix4 (this.model, false, ref value)
        | _ -> ()

    member this.View value =
        match this.progShader with
        | LinkedProgShader _ ->
            GL.UniformMatrix4 (this.view, false, ref value)
        | _ -> ()

    member this.Proj value =
        match this.progShader with
        | LinkedProgShader _ ->
            GL.UniformMatrix4 (this.proj, false, ref value)
        | _ -> ()

    member this.Chan0 (value:int) =
        match this.progShader with
        | LinkedProgShader _ ->
            GL.Uniform1 (this.chan0, value)
        | _ -> ()

    member this.Use =
        match this.progShader with
        | LinkedProgShader (ProgShaderObject (ProgShaderId glId), _) ->
            GL.UseProgram glId
            GL.EnableVertexAttribArray this.position
            GL.EnableVertexAttribArray this.color
            GL.EnableVertexAttribArray this.uv
        | _ -> ()

    member this.BindBuffers (vbos:int array) =
        match this.progShader with
        | LinkedProgShader _ ->
            GL.BindBuffer (BufferTarget.ArrayBuffer, vbos.[0])
            GL.VertexAttribPointer (this.position, 3, VertexAttribPointerType.Float, false, sizeof<single>*3, 0)

            GL.BindBuffer (BufferTarget.ArrayBuffer, vbos.[1])
            GL.VertexAttribPointer (this.color, 4, VertexAttribPointerType.Float, false, sizeof<single>*4, 0)

            GL.BindBuffer (BufferTarget.ArrayBuffer, vbos.[2])
            GL.VertexAttribPointer (this.uv, 2, VertexAttribPointerType.Float, false, sizeof<single>*2, 0)
        | _ -> ()

    member this.UnbindBuffers () =
        match this.progShader with
        | LinkedProgShader _ ->
            GL.BindBuffer (BufferTarget.ArrayBuffer, 0)
        | _ -> ()

