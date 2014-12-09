namespace Tests

open System
open System.Threading
open System.Reactive
open System.Reactive.Linq
open FSharp.Control.Reactive

open NUnit.Framework

open Atom.Async

[<TestFixture>]
type AtomTests () =

    let waitSeconds s = async {
        for i in [1..s] do
            do! Async.Sleep 1000
            printfn "%i" i
        }

    let observe = new Builders.ObservableBuilder ()

    [<Test>]
    member x.``AwaitObservable`` () =
        let cts = new CancellationTokenSource (4000)
        let startTime = DateTime.Now
        let obs = Observable.FromAsync ((waitSeconds 5), cts)
        async {
            do! Async.AwaitObservable obs
            printfn "Async.AwaitObservable done"
        } |> Async.RunSynchronously
        let duration = DateTime.Now - startTime
        printfn "Duration %A secs" duration.TotalSeconds
        Assert.IsTrue ((duration.TotalSeconds < 4.0))

    [<Test>]
    member x.``Observe Async`` () =

        let obs = observe {
            yield 4
            }

        obs
        |> Observable.add (printfn "%A")


