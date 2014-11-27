namespace Cortex

open System
open System.IO
open OpenTK
open OpenTK.Graphics.ES30
open MonoTouch.Foundation
open MonoTouch.GLKit
open Renderer
open Cortex.Observable

module Texture =

    module private __ =

        let mutable stub : GLKTextureInfo option = None

        let getStub =
            match stub with
            | Some texInfo -> texInfo
            | None ->
                let texOps = new GLKTextureOperations ()
                texOps.OriginBottomLeft <- new Nullable<bool> (true)
                let path = NSBundle.MainBundle.PathForResource ("texguide", "jpg")
                let (texInfo, err) = GLKTextureLoader.FromFile (path, texOps)
                stub <- Some texInfo
                texInfo

    open __

    type Texture =

        val mutable texInfo : GLKTextureInfo
        val mutable data : byte [] option
        val name : string

        new (name) as this =
            {
            texInfo = getStub
            data = None
            name = name
            }
            then
                this.Bind ()
                this.Setup ()

        member this.Release () =
            let deleteTexture =
                GL.DeleteTexture this.texInfo.Name
                this.texInfo.Dispose ()
                this.texInfo <- getStub
            match stub with
            | Some stub when this.texInfo <> stub -> deleteTexture
            | None -> deleteTexture
            | _ -> ()

        member this.OnData bytes =
            this.data <- Some bytes

        member this.Setup () =
            GL.TexParameter (TextureTarget.Texture2D, TextureParameterName.TextureWrapS, int All.Repeat)
            GL.TexParameter (TextureTarget.Texture2D, TextureParameterName.TextureWrapT, int All.Repeat)
            GL.TexParameter (TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, int All.Linear)
            GL.TexParameter (TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, int All.Linear)

        member this.Bind () =
            match this.data with
            | Some bytes ->
                use texData = NSData.FromArray bytes
                let texOps = new GLKTextureOperations ()
                texOps.OriginBottomLeft <- new Nullable<bool> (true)
                match GLKTextureLoader.FromData (texData, texOps) with
                | (tex, null) ->
                    this.Release ()
                    this.texInfo <- tex
                    GL.BindTexture (TextureTarget.Texture2D, this.texInfo.Name)
                    this.Setup ()
                | (_, err) -> printfn "Err loading tex: %A %A" this.name err
                this.data <- None
            | None ->
                GL.BindTexture (TextureTarget.Texture2D, this.texInfo.Name)

        interface IDisposable with
            member this.Dispose() = this.Release ()

    let fromRemote asset =
        let tex = new Texture (asset)
        Asset.AsObservable asset
        |> Observable.subscribe tex.OnData
        |> ignore
        tex

