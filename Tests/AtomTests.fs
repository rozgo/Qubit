namespace Tests

open System
open NUnit.Framework

open System.Reactive
open System.Reactive.Linq

open Atom.Async

[<TestFixture>]
type AtomTests () =

    let waitABit = async {
        do! Async.Sleep 1000
        printfn "1"
        do! Async.Sleep 1000
        printfn "2"
        do! Async.Sleep 1000
        printfn "3"
        }

    [<Test>]
    member x.``AwaitObservable`` () =

        let obs = Observable.FromAsync waitABit

        async {

            let! fstObs = Async.AwaitObservable obs
            printfn "first done"
            let! sndObs = Async.AwaitObservable obs
            printfn "second done"
        }

        |> Async.Start

        Async.RunSynchronously (Async.Sleep 10000)

        Assert.AreEqual (0, 1)
