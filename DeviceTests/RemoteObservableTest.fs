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

        Observer.remote "range" range

        let remObservable = Observable.remote "range"

        remObservable
        |> Observable.add (printfn "%i")

