module Cortex.Shader

open System
open OpenTK
open OpenTK.Graphics.ES30
open Foundation
open Cortex.Renderer
open FSharp.Control.Reactive
open Atom

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

module X =

    type AttribName = AttribName of string
    type AttribLocation = AttribLocation of int
    type Attrib = AttribName * AttribLocation

    type UniformName = UniformName of string
    type UniformLocation = UniformLocation of int
    type Uniform = UniformName * UniformLocation

    type VertexAttrib =
        | Position
        | Color
        | UV

    type Shader = {
        attributes : Attrib list
        uniforms : Uniform list }

type Vybe () =

    let position = 0
    let color = 1
    let uv = 2

    let mutable model = 0
    let mutable view = 0
    let mutable proj = 0
    let mutable chan0 = 0

    let mutable vertShader = FailedVertShader NoShaderInfo
    let mutable fragShader = FailedFragShader NoShaderInfo
    let mutable progShader = FailedProgShader NoShaderInfo

    let bind progId =
        GL.BindAttribLocation (progId, position, "position")
        GL.BindAttribLocation (progId, color, "color")
        GL.BindAttribLocation (progId, uv, "uv")

    let vsPath = "Shaders/Vybe.vsh"
    let fsPath = "Shaders/Vybe.fsh"

    let vs =
        Asset.observe vsPath
        |> Observable.map binToAscii

    let fs =
        Asset.observe fsPath
        |> Observable.map binToAscii

    let observer =
        Observable.zip vs fs
        |> Observable.subscribe (fun (vs,fs) ->
            vertShader <- VertShader vs
            fragShader <- FragShader fs
            progShader <- ProgShader vertShader fragShader bind
            match progShader with
            | LinkedProgShader _ ->
                model <- GetUniformLocation progShader "model"
                view <- GetUniformLocation progShader "view"
                proj <- GetUniformLocation progShader "proj"
                chan0 <- GetUniformLocation progShader "chan0"
            | _ -> validate vertShader fragShader progShader vsPath fsPath |> ignore)

    member this.Model value =
        match progShader with
        | LinkedProgShader _ ->
            GL.UniformMatrix4 (model, false, ref value)
        | _ -> ()

    member this.View value =
        match progShader with
        | LinkedProgShader _ ->
            GL.UniformMatrix4 (view, false, ref value)
        | _ -> ()

    member this.Proj value =
        match progShader with
        | LinkedProgShader _ ->
            GL.UniformMatrix4 (proj, false, ref value)
        | _ -> ()

    member this.Chan0 (value:int) =
        match progShader with
        | LinkedProgShader _ ->
            GL.Uniform1 (chan0, value)
        | _ -> ()

    member this.Bind () =
        match progShader with
        | LinkedProgShader (ProgShaderObject (ProgShaderId glId), _) ->
            GL.UseProgram glId
            GL.EnableVertexAttribArray position
            GL.EnableVertexAttribArray color
            GL.EnableVertexAttribArray uv
        | _ -> ()

    member this.Unbind () =
        match progShader with
        | LinkedProgShader (ProgShaderObject (ProgShaderId glId), _) ->
            GL.UseProgram 0
            GL.DisableVertexAttribArray position
            GL.DisableVertexAttribArray color
            GL.DisableVertexAttribArray uv
        | _ -> ()

    member this.BindBuffers (vbos:int array) =
        match progShader with
        | LinkedProgShader _ ->
            GL.BindBuffer (BufferTarget.ArrayBuffer, vbos.[0])
            GL.VertexAttribPointer (position, 3, VertexAttribPointerType.Float, false, sizeof<single>*3, 0)

            GL.BindBuffer (BufferTarget.ArrayBuffer, vbos.[1])
            GL.VertexAttribPointer (color, 4, VertexAttribPointerType.Float, false, sizeof<single>*4, 0)

            GL.BindBuffer (BufferTarget.ArrayBuffer, vbos.[2])
            GL.VertexAttribPointer (uv, 2, VertexAttribPointerType.Float, false, sizeof<single>*2, 0)
        | _ -> ()

    member this.UnbindBuffers () =
        match progShader with
        | LinkedProgShader _ ->
            GL.BindBuffer (BufferTarget.ArrayBuffer, 0)
        | _ -> ()

