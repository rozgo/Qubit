module Atom.Async

open System
open System.Threading
open System.Reactive
open System.Reactive.Linq

#nowarn "40"

type Microsoft.FSharp.Control.Async with 
    static member AwaitObservable (obs : IObservable<'T>) =
        let timeReceived = new Event<'T> ()
        let rec trigger t =
            timeReceived.Trigger t
            sub.Dispose ()
        and sub : IDisposable = obs.Subscribe<'T> (trigger)
        Async.AwaitEvent (timeReceived.Publish)
