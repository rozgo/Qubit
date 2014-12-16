open System
open System.Threading
open System.Collections
open System.Collections.Generic
open System.Linq
open System.Reactive.Linq
open System.Reactive.Concurrency
open System.Reactive.Disposables
open FSharp.Control.Reactive

let doSomething01 =

    let observe = new Builders.ObservableBuilder ()

    let evt1 = new Event<string> ()
    let evt2 = new Event<string> ()
    let evt3 = new Event<string> ()
    let evt4 = new Event<string> ()

    let obs =
        observe {
            let! s = evt1.Publish
            printfn "%A" s
            let! s = evt2.Publish
            printfn "%A" s
            let! s = evt3.Publish
            printfn "%A" s
            let! s = evt4.Publish
            printfn "%A" s
            return 2
        }

    obs
    |> Observable.add (printfn "%A")

    let a = 
        async {
        do! Async.Sleep 1000
        evt1.Trigger "Hello 1"
        do! Async.Sleep 1000
        evt2.Trigger "Hello 2"
        do! Async.Sleep 1000
        evt3.Trigger "Hello 3"
        do! Async.Sleep 1000
        evt4.Trigger "Hello 4"
        do! Async.Sleep 1000
        }

    a |> Async.Start


[<EntryPoint>]
let main argv = 

    let evt (obs:IObserver<int>) = async {
        do! Async.Sleep 1000
        obs.OnNext 1
        do! Async.Sleep 1000
        obs.OnNext 2
        do! Async.Sleep 1000
        obs.OnNext 3
        obs.OnCompleted () }

    let obs = Observable.createWithDisposable (fun obs ->
        Async.Start (evt obs)
        Disposable.Empty)

    Observable.add (printfn "%A") obs

    printfn "Press return to exit..."
    Console.ReadLine () |> ignore

    0

