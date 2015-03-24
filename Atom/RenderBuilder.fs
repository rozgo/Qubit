module Atom.RenderBuilder

type Computation = (unit -> unit) -> unit

type State = State of Computation

let draw s =
    let (State c) = s
    c (fun () -> ())

let ret c = State c

let bind f s =
    let (State a) = s
    let (State b) = f a
    ret (fun f -> a (fun f -> b (fun f -> f)))

let combine a b =
    let (State a) = a
    let (State b) = b
    let a = (fun f -> a (fun f -> f))
    let b = (fun f -> b (fun f -> f))
    ret (fun f -> a f; b f)

let empty = State (fun f -> f ())

type Builder () = 
    member x.Return v = ret v
    member x.ReturnFrom v = v
    member x.Bind (s, f) = bind f s
    member x.Combine (a, b) = combine a b
    member x.Delay f = f ()
    member x.Zero () = empty
    member x.For (sequence:seq<_>, body) =
        Seq.fold (fun a b -> combine a (body b)) empty sequence
