namespace Cortex

open System
open OpenTK
open OpenTK.Graphics.ES30
open MonoTouch.Foundation

module Renderer =

    type UniformLocation = UniformLocation of int

    type ShaderInfo =
        | NoShaderInfo
        | ShaderInfo of string

    type VertShaderId = VertShaderId of int
    type FragShaderId = FragShaderId of int
    type ProgShaderId = ProgShaderId of int

    type VertShaderObject = VertShaderObject of VertShaderId
    type FragShaderObject = FragShaderObject of FragShaderId
    type ProgShaderObject = ProgShaderObject of ProgShaderId

    type VertShader =
        | CompiledVertShader of VertShaderObject * ShaderInfo
        | FailedVertShader of ShaderInfo

    type FragShader = 
        | CompiledFragShader of FragShaderObject * ShaderInfo
        | FailedFragShader of ShaderInfo

    type ProgShader = 
        | LinkedProgShader of ProgShaderObject * ShaderInfo
        | FailedProgShader of ShaderInfo

    let VertShader source =
        let glId = GL.CreateShader (ShaderType.VertexShader)
        GL.ShaderSource (glId, source)
        GL.CompileShader (glId)
        let info =
            let log = GL.GetShaderInfoLog (glId)
            if log.Length = 0 then NoShaderInfo
            else ShaderInfo log
        let err = GL.GetErrorCode ()
        if err = ErrorCode.NoError then
            CompiledVertShader (VertShaderObject (VertShaderId glId), info)
        else
            FailedVertShader info

    let FragShader source =
        let glId = GL.CreateShader (ShaderType.FragmentShader)
        GL.ShaderSource (glId, source)
        GL.CompileShader (glId)
        let info =
            let log = GL.GetShaderInfoLog (glId)
            if log.Length = 0 then NoShaderInfo
            else ShaderInfo log
        let err = GL.GetErrorCode ()
        if err = ErrorCode.NoError then
            CompiledFragShader (FragShaderObject (FragShaderId glId), info)
        else
            FailedFragShader info

    let ProgShader vertShader fragShader bind =
        match vertShader, fragShader with
        | CompiledVertShader (cvs, _), CompiledFragShader (cfs, _) ->
            let (VertShaderObject (VertShaderId glVertId)) = cvs
            let (FragShaderObject (FragShaderId glFragId)) = cfs
            let glId = GL.CreateProgram ()
            GL.AttachShader (glId, glVertId)
            GL.AttachShader (glId, glFragId)
            bind glId
            GL.LinkProgram (glId)
            let info =
                let log = GL.GetProgramInfoLog (glId)
                if log.Length = 0 then NoShaderInfo
                else ShaderInfo log
            let err = GL.GetErrorCode ()
            if err = ErrorCode.NoError then
                LinkedProgShader (ProgShaderObject (ProgShaderId glId), info)
            else
                FailedProgShader info
        | FailedVertShader fvs, FailedFragShader ffs -> FailedProgShader fvs
        | _, FailedFragShader ffs -> FailedProgShader ffs
        | FailedVertShader fvs, _ -> FailedProgShader fvs

    let GetUniformLocation progShader name =
        let id =
            match progShader with
            | LinkedProgShader (lps, _) ->
                let (ProgShaderObject (ProgShaderId glProgId)) = lps
                glProgId
            | _ -> -1
        GL.GetUniformLocation (id, name)



