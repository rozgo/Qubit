module Atom.Observable

open System
open FSharp.Control.Reactive
open System.Reactive.Disposables

type Property<'T> (observable:IObservable<'T>, initial) =

    let mutable cached = false
    let mutable value = initial
    let mutable key = 0
    let mutable subscribers = Map.empty : Map<int,IObserver<'T>>

    let OnNext (v) =
        value <- v
        cached <- true
        subscribers |> Seq.iter (fun (KeyValue(_,sub)) -> sub.OnNext v)
    let OnCompleted () = subscribers |> Seq.iter (fun (KeyValue(_,sub)) -> sub.OnCompleted ())
    let OnError (err) = subscribers |> Seq.iter (fun (KeyValue(_,sub)) -> sub.OnError err)

    let disposable = observable.Subscribe (OnNext, OnError, OnCompleted)

    member this.Observable = Observable.createWithDisposable (fun subscriber ->
        key <- key + 1
        subscribers <- Map.add key subscriber subscribers
        if cached then subscriber.OnNext value
        {new IDisposable with
             member x.Dispose () =
                 subscribers <- Map.remove key subscribers})

    member this.Value
        with get () = value

    interface IDisposable with
        member this.Dispose () = disposable.Dispose ()

let property<'T> obs initial =
    new Property<'T> (obs, initial)



