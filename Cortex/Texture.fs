module Cortex.Texture

open System
open System.IO
open OpenTK
open OpenTK.Graphics.ES30
open Foundation
open GLKit
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

type Texture2D (asset) =

    let mutable texInfo = stub

    let Release () =
        if texInfo <> stub then
            GL.DeleteTexture texInfo.Name
            texInfo.Dispose ()
            texInfo <- stub

    let png = Asset.observe (asset + ".png")
    let jpg = Asset.observe (asset + ".jpg")
    let observer =
        Observable.merge png jpg
        |> Observable.subscribe (onAsset asset (fun tex ->
            Release ()
            texInfo <- tex
            GL.BindTexture (TextureTarget.Texture2D, texInfo.Name)
            setup ()))

    member this.Bind () = GL.BindTexture (TextureTarget.Texture2D, texInfo.Name)
    member this.Unbind () = GL.BindTexture (TextureTarget.Texture2D, 0)

    interface IDisposable with
        member this.Dispose () =
            observer.Dispose ()
            Release ()
