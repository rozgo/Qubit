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
    member inline x.Return v = ret v
    member inline x.ReturnFrom v = v
    member inline x.Bind (s, f) = bind f s
    member inline x.Combine (a, b) = combine a b
    member inline x.Delay f = f ()

    member this.Zero () = 
        this.Return (fun f -> f ())

//    member this.While(guard, body) =
//        if not (guard()) 
//        then this.Zero() 
//        else this.Bind( body(), fun () -> 
//            this.While(guard, body))  
//
//    member this.TryWith(body, handler) =
//        try this.ReturnFrom(body())
//        with e -> handler e
//
//    member this.TryFinally(body, compensation) =
//        try this.ReturnFrom(body())
//        finally compensation() 
//
//    member this.Using(disposable:#System.IDisposable, body) =
//        let body' = fun () -> body disposable
//        this.TryFinally(body', fun () -> 
//            match disposable with 
//                | null -> () 
//                | disp -> disp.Dispose())

    member x.For(sequence:seq<_>, body) =
        let combine a b = x.Combine(a, body b)
        Seq.fold combine (x.Zero()) sequence