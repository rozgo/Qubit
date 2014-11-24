namespace Cortex

open System
open System.IO
open System.Runtime.InteropServices
open OpenTK
open OpenTK.Graphics.ES20
open MonoTouch.Foundation
open MonoTouch.UIKit
open MonoTouch.CoreImage
open MonoTouch.CoreGraphics
open System.Drawing
open Cortex.Renderer

module Texture2D =

    type Texture2D (name:string) =
        
        let id =
            let mutable glId = [| 0 |]
            GL.Hint (HintTarget.GenerateMipmapHint, HintMode.Nicest);
            GL.GenTextures (1, glId)
            glId.[0]

        member this.Use () = GL.BindTexture (TextureTarget.Texture2D, id)


    let fromFile filename =

        let tex = Texture2D (filename)
        tex.Use ()



        GL.TexParameter (TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int) All.Repeat)
        GL.TexParameter (TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int) All.Repeat)
        GL.TexParameter (TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int) All.Linear)
        GL.TexParameter (TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int) All.Linear)
        //GL.TexParameter (TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int) All.Nearest)
        //GL.TexParameter (TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int) All.Nearest)

        //let extension = Path.GetExtension (filename)
        //let baseFilename = Path.GetFileNameWithoutExtension (filename)
        //let path = NSBundle.MainBundle.PathForResource (baseFilename, extension)

        let path = NSBundle.MainBundle.PathForResource (filename, "png")

        let texData = NSData.FromFile (path)

        let image = UIImage.LoadFromData (texData)

        if image <> null then

            let width = image.CGImage.Width;
            let height = image.CGImage.Height;

            let colorSpace = CGColorSpace.CreateDeviceRGB ()
            let imageData : byte[] = Array.zeroCreate (height * width * 4)
            let context = new CGBitmapContext  (imageData, width, height, 8, 4 * width, colorSpace,
                                                      CGBitmapFlags.PremultipliedLast ||| CGBitmapFlags.ByteOrder32Big);

            context.TranslateCTM (0.f, single height)
            context.ScaleCTM (1.f, -1.f)
            colorSpace.Dispose ()
            context.ClearRect (new RectangleF (0.f, 0.f, single width, single height))
            context.DrawImage (new RectangleF (0.f, 0.f, single width, single height), image.CGImage)

            GL.TexImage2D (TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, width, height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, imageData)
            //GL.GenerateMipmap (TextureTarget.Texture2D)
            context.Dispose ()

            Some tex

        else
            printfn "Texture: Error loading %s" filename
            None

