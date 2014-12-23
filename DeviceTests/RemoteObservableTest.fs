namespace DeviceTests

open System
open NUnit.Framework
open FSharp.Control.Reactive

open Atom

[<TestFixture>]
type RemoteObservableTests () =

    [<Test>]
    member x.``Remote Observable`` () =

        let range = Observable.range 0 5

        let remObserver = new Observable.RemoteObserver<int> (range, "range")

        let remObservable = new Observable.RemoteObservable<int> ("range")

        remObservable.Observable
        |> Observable.add (printfn "%A")

