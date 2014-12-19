module Atom.Observable

open System

type Property<'T> (obs:IObservable<'T>, initial:'T) =
    let mutable property = initial
    let disposable = obs.Subscribe (fun x -> property <- x)
    member this.Value = property
    interface IDisposable with
        member this.Dispose () = disposable.Dispose ()

let property<'T> obs initial =
    new Property<'T> (obs, initial)