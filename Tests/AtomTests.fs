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

    let observe = new Builders.ObservableBuilder ()

    [<Test>]
    member x.``AwaitObservable`` () =
        let cts = new CancellationTokenSource (4000)
        let startTime = DateTime.Now
        let obs = Observable.Timer (TimeSpan (0, 0, 3))
        async {
            let! t = Async.AwaitObservable obs
            printfn "Async.AwaitObservable done"
        } |> Async.RunSynchronously
        let duration = DateTime.Now - startTime
        printfn "Duration %A secs" duration.TotalSeconds
        Assert.IsTrue ((duration.TotalSeconds < 4.0))

    [<Test>]
    member x.``Observe Async`` () =
        observe {yield 4}
        |> Observable.add (printfn "%A")

