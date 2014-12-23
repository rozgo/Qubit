module Atom.Observable

open System
open FSharp.Control.Reactive
open System.Reactive.Disposables

type Property<'T> (observable:IObservable<'T>, initial) =

    let mutable cached = false
    let mutable value = initial
    let mutable key = 0
    let mutable observers = Map.empty : Map<int,IObserver<'T>>

    let onNext (v) =
        value <- v
        cached <- true
        observers |> Seq.iter (fun (KeyValue(_,obs)) -> obs.OnNext v)
    let onCompleted () = observers |> Seq.iter (fun (KeyValue(_,obs)) -> obs.OnCompleted ())
    let onError (err) = observers |> Seq.iter (fun (KeyValue(_,obs)) -> obs.OnError err)

    let mutable disposable = None

    member this.Observable = Observable.createWithDisposable (fun subscriber ->
        key <- key + 1
        observers <- Map.add key subscriber observers
        if cached then
            subscriber.OnNext value
        match disposable with
        | Some disposable -> ()
        | None -> disposable <- Some (observable.Subscribe (onNext, onError, onCompleted))
        {new IDisposable with
             member x.Dispose () =
                 observers <- Map.remove key observers})

    member this.Value
        with get () = value

    interface IDisposable with
        member this.Dispose () =
            onCompleted ()
            observers <- Map.empty
            match disposable with
            | Some disposable -> disposable.Dispose ()
            | _ -> ()

let property<'T> initial obs =
    new Property<'T> (obs, initial)


type RemoteObservable<'T> (channel) =

    let mutable key = 0
    let mutable observers = Map.empty : Map<int,IObserver<'T>>

    let onNext (v) = observers |> Seq.iter (fun (KeyValue(_,obs)) -> obs.OnNext v)
    let onCompleted () = observers |> Seq.iter (fun (KeyValue(_,obs)) -> obs.OnCompleted ())
    let onError (err) = observers |> Seq.iter (fun (KeyValue(_,obs)) -> obs.OnError err)

    let disposables = [
        Axon.observe<string,'T> (channel + ":OnNext") |> Observable.subscribe onNext;
        Axon.observe<string,unit> (channel + ":OnCompleted") |> Observable.subscribe onCompleted;
        Axon.observe<string,string> (channel + ":OnError")
        |> Observable.map (fun err -> Exception err)
        |> Observable.subscribe onError]

    member this.Observable = Observable.createWithDisposable (fun subscriber ->
        key <- key + 1
        observers <- Map.add key subscriber observers
        Axon.trigger (channel + ":OnSubscribed") key
        {new IDisposable with
            member x.Dispose () =
                Axon.trigger (channel + ":OnDisposed") key
                observers <- Map.remove key observers})

    interface IDisposable with
        member this.Dispose () =
            onCompleted ()
            List.iter (fun (obs:IDisposable) -> obs.Dispose ()) disposables
            observers <- Map.empty

type RemoteObserver<'T> (observable:IObservable<'T>, channel) =

    let mutable subscribed = None

    let onNext (v) = Axon.trigger (channel + ":OnNext") v
    let onCompleted () = Axon.trigger (channel + ":OnCompleted") ()
    let onError (err) = Axon.trigger (channel + ":OnError") (err.ToString ())
    let onSubscribed (key) =
        if subscribed = None then
            subscribed <- Some (observable.Subscribe (onNext, onError, onCompleted))
    let onDisposed (key) = ()

    let disposables = [
        Axon.observe<string, int> (channel + ":OnSubscribed")
        |> Observable.subscribe onSubscribed;
        Axon.observe<string, int> (channel + ":OnDisposed")
        |> Observable.subscribe onDisposed]

    interface IDisposable with
        member this.Dispose () =
            match subscribed with
            | Some disposable -> disposable.Dispose ()
            | _ -> ()
            List.iter (fun (obs:IDisposable) -> obs.Dispose ()) disposables
            onCompleted ()