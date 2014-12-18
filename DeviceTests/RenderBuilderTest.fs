namespace DeviceTests

open System
open NUnit.Framework

open Atom

module DebugRenderer =

    let mutable depth = 0
    let mutable id = 0

    let debugState label =
        let label = label + " (" + (id.ToString()) + ")"
        id <- id + 1
        RenderBuilder.State (fun f ->
            printfn "%sset %s" (String.replicate depth "-") label
            depth <- depth + 1
            f ()
            depth <- depth - 1
            printfn "%sunset %s" (String.replicate depth "-") label)

    let drawcall () = debugState "drawcall"
    let shader name = debugState ("shader:" + name)
    let color name = debugState ("color:" + name)
    let shape form = debugState ("shape:" + form)
    let fullscreen () = debugState "fullsreen"
    let uniform () = debugState "uniform"
    let attrib () = debugState "attrib"

open DebugRenderer

[<TestFixture>]
type RenderBuilderTest () =

    let render = new RenderBuilder.Builder ()

    [<Test>]
    member this.``Depth`` () =

        let scene = render {

            let! dc = drawcall ()
            let! sh = shader "Unified"
            let! co = color "red"
            return! shape "box"

            let! p = render {
                let! dc = drawcall ()
                return! attrib ()
            }

            let! un = uniform ()
            return! fullscreen ()

            let! at = attrib ()
            return! shape "circle"
        }

        RenderBuilder.draw scene        

        Assert.IsTrue ((DebugRenderer.depth = 0))

    [<Test>]
    member this.``Left Identity`` () =

        // https://github.com/fsprojects/fsharpx/blob/master/tests/FSharpx.Tests/MaybeTest.fs

        let ret x = render.Return x
        let (>>=) m f = render.Bind (m, f)

        //let m = fun f a -> ret a >>= f = f a

        Assert.IsTrue (true)



