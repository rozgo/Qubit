module RenderStateModad

type RenderState = RenderState of ((unit->unit) -> unit)

let apply rs =
    let (RenderState func) = rs
    func (fun () -> ())

let empty = RenderState (fun func -> func ())

let drawcall =
    let label = "drawcall"
    RenderState (fun func ->
        printfn "set %s" label
        func ()
        printfn "unset %s" label)

let shader =
    let label = "shader"
    RenderState (fun func ->
        printfn "set %s" label
        func ()
        printfn "unset %s" label)

let color =
    let label = "color"
    RenderState (fun func ->
        printfn "set %s" label
        func ()
        printfn "unset %s" label)

let shape =
    let label = "shape"
    RenderState (fun func ->
        printfn "set %s" label
        func ()
        printfn "unset %s" label)

let fullscreen =
    let label = "fullscreen"
    RenderState (fun func ->
        printfn "set %s" label
        func ()
        printfn "unset %s" label)

type RenderStateBuilder () = 

  member inline x.Return v = empty

  member inline x.Bind (rs, f) =
      let (RenderState x) = rs
      let (RenderState y) = f x
      let g = (fun func -> x (fun func -> y (fun func -> func )))
      RenderState g

  member inline x.Combine (a, b) = b

  member inline x.Delay f = f ()

let render = RenderStateBuilder ()

module Test =

    let renderSomething () =

        let scene = render {
            let! dc = drawcall
            let! sh = shader
            let! co = color
            let! sh = shape
            return sh
        }

        let fx = render {
            let! fs = fullscreen
            return fs
        }

        apply scene
        apply fx

