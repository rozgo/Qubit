module Cortex.Material

open System
open System.IO
open System.Collections.Generic
open System.Linq

open OpenTK
open OpenTK.Graphics.ES20
open OpenTK.Platform.iPhoneOS
open MonoTouch.Foundation
open MonoTouch.CoreAnimation
open MonoTouch.ObjCRuntime
open MonoTouch.OpenGLES
open MonoTouch.UIKit
open System.Drawing

open Nessos.FsPickler

// Blend SrcAlpha OneMinusSrcAlpha      -   Alpha blending
// Blend One One                        -   Additive
// Blend OneMinusDstColor One           -   Soft Additive
// Blend DstColor Zero                  -   Multiplicative
// Blend DstColor SrcColor              -   2x Multiplicative

type BlendFactor =
    | Zero
    | SrcColor
    | SrcAlpha
    | DstColor
    | DstAlpha
    | OneMinusSrcColor
    | OneMinusSrcAlpha
    | OneMinusDstColor
    | OneMinusDstAlpha

type Blend = Blend of BlendFactor * BlendFactor

type VertShader = VertShader of string
type FragShader = FragShader of string

type Material =
    {
    blend : Blend option
    vert : VertShader
    frag : FragShader
    }

let test () =
    let m =
        {
        blend = Some (Blend (SrcColor, OneMinusSrcAlpha))
        vert = VertShader "Vybe.vsh"
        frag = FragShader "Vybe.fsh"
        }

    //let json = Newtonsoft.Json.JsonConvert.SerializeObject(m)

    let writer = new MemoryStream ()
    let p = FsPickler.CreateBinary ()

    let bytes = p.Pickle m
    let mp = p.UnPickle<Material> bytes
    //jsp.Serialize<int option list>(writer, [Some 1; None; Some -1])
    //let dump = jsp.Pickle<int option list>([Some 1; None; Some -1])

//    let writer = new StringWriter ()
//    let s = YamlDotNet.Serialization.Serializer()
//    s.Serialize (writer, m)

    //let dump = writer.ToString ()
    printfn "YAML: %A" mp
    //printfn "YAML: %A" json
    ignore