module Cortex.Texture

open System
open System.IO
open OpenTK
open OpenTK.Graphics.ES30
open MonoTouch.Foundation
open MonoTouch.GLKit
open FSharp.Control.Reactive

module private __ =

    let setup () =
        GL.TexParameter (TextureTarget.Texture2D, TextureParameterName.TextureWrapS, int All.Repeat)
        GL.TexParameter (TextureTarget.Texture2D, TextureParameterName.TextureWrapT, int All.Repeat)
        GL.TexParameter (TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, int All.Linear)
        GL.TexParameter (TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, int All.Linear)

    let stub =
        let texOps = new GLKTextureOperations ()
        texOps.OriginBottomLeft <- new Nullable<bool> (true)
        let path = NSBundle.MainBundle.PathForResource ("texguide", "jpg")
        let (texInfo, err) = GLKTextureLoader.FromFile (path, texOps)
        GL.BindTexture (TextureTarget.Texture2D, texInfo.Name)
        setup ()
        GL.BindTexture (TextureTarget.Texture2D, 0)
        texInfo

    let onAsset name success bytes =
        try
            use texData = NSData.FromArray bytes
            let texOps = new GLKTextureOperations ()
            texOps.OriginBottomLeft <- new Nullable<bool> (true)
            match GLKTextureLoader.FromData (texData, texOps) with
            | (tex, null) -> success tex
            | (_, err) -> printfn "Err loading tex: %A %A" name err
        with e ->
            printfn "%A" e

open __

type Texture =

    val mutable texInfo : GLKTextureInfo
    val asset : string
    val mutable observer : IDisposable

    new (asset) as this =
        {
        texInfo = stub
        asset = asset
        observer = null
        }
        then
            let png = Asset.observe (asset + ".png")
            let jpg = Asset.observe (asset + ".jpg")
            this.observer <- Observable.merge png jpg
            |> Observable.subscribe (onAsset asset (fun tex ->
                this.Release ()
                this.texInfo <- tex
                GL.BindTexture (TextureTarget.Texture2D, this.texInfo.Name)
                setup ()))

    member this.Release () =
        if this.texInfo <> stub then
            GL.DeleteTexture this.texInfo.Name
            this.texInfo.Dispose ()
            this.texInfo <- stub

    member this.Bind () = GL.BindTexture (TextureTarget.Texture2D, this.texInfo.Name)

    interface IDisposable with
        member this.Dispose () =
            this.observer.Dispose ()
            this.Release ()
