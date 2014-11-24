namespace Cortex.Shaders

open System
open System.Runtime.InteropServices
open OpenTK
open OpenTK.Graphics.ES20
open MonoTouch.Foundation
open Cortex.Renderer

module Validate =

    let ok vertShader fragShader progShader vertPath fragPath =
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

module Vybe =

    type Program () =

        let vertPath = NSBundle.MainBundle.PathForResource ("Vybe", "vsh")
        let fragPath = NSBundle.MainBundle.PathForResource ("Vybe", "fsh")

        let vertSource = System.IO.File.ReadAllText vertPath
        let fragSource = System.IO.File.ReadAllText fragPath
        
        let vertShader = VertShader vertSource
        let fragShader = FragShader fragSource

        let position = 0
        let color = 1
        let uv = 2

        let Bind progId =
            GL.BindAttribLocation (progId, position, "position")
            GL.BindAttribLocation (progId, color, "color")
            GL.BindAttribLocation (progId, uv, "uv")

        let progShader = ProgShader vertShader fragShader Bind

        let ok = Validate.ok vertShader fragShader progShader vertPath fragPath

        let model = GetUniformLocation progShader "model"
        let view = GetUniformLocation progShader "view"
        let proj = GetUniformLocation progShader "proj"
        let chan0 = GetUniformLocation progShader "chan0"

        member this.Model value =
            GL.UniformMatrix4 (model, false, ref value)

        member this.View value =
            GL.UniformMatrix4 (view, false, ref value)

        member this.Proj value =
            GL.UniformMatrix4 (proj, false, ref value)

        member this.Chan0 (value:int) =
            GL.Uniform1 (chan0, value)

        member this.Use =
            match progShader with
            | LinkedProgShader (ProgShaderObject (ProgShaderId glId), _) ->
                GL.UseProgram glId
                GL.EnableVertexAttribArray position
                GL.EnableVertexAttribArray color
                GL.EnableVertexAttribArray uv
            | _ -> ()

        member this.Attribs (vbos:int array) =

            GL.BindBuffer (BufferTarget.ArrayBuffer, vbos.[0])
            GL.VertexAttribPointer (position, 3, VertexAttribPointerType.Float, false, sizeof<single>*3, 0)

            GL.BindBuffer (BufferTarget.ArrayBuffer, vbos.[1])
            GL.VertexAttribPointer (color, 4, VertexAttribPointerType.Float, false, sizeof<single>*4, 0)

            GL.BindBuffer (BufferTarget.ArrayBuffer, vbos.[2])
            GL.VertexAttribPointer (uv, 2, VertexAttribPointerType.Float, false, sizeof<single>*2, 0)

