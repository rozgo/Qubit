module Atom.RenderGraph

type Computation = (unit -> unit) -> unit
type State = State of (Computation * State list * Computation)
type Block = Block of State list

let e = fun f -> f ()

let ret c = Block c

let zero = Block [State (e, [], e)]

let bind (f:State list -> Block) (s:Block) =
    let (Block a) = s
    match List.rev a with
    | [] -> zero
    | head::[] -> 
        let (State (cBegin, cStates, cEnd)) = head
        let (Block b) = f a
        ret [State (cBegin, cStates @ b, cEnd)]
    | head::tail ->
        let tail = List.rev tail
        let (State (cBegin, cStates, cEnd)) = head
        let (Block b) = f a
        ret (tail @ [State (cBegin, cStates @ b, cEnd)])

let combine (a:Block) (b:Block) =
    let (Block aS) = a
    let (Block bS) = b
    ret (aS @ bS)

type Builder () = 
    member x.Return v = ret v
    member x.ReturnFrom v = v
    member x.Bind (s, f) = bind f s
    member x.Combine (a, b) = combine a b
    member x.Delay f = f ()
    member x.Zero () = zero
    member x.For (sequence:seq<_>, body) =
        Seq.fold (fun a b -> combine a (body b)) zero sequence

