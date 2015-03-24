namespace DeviceTests

open System
open NUnit.Framework
open FSharp.Control.Reactive

open Atom

[<TestFixture>]
type RemoteObservableTests () =

    [<Test>]
    member x.``RemoteObserver Cold to Hot`` () =

        let obs =
            Observable.interval (TimeSpan.FromSeconds 1.0)
            |> Observable.take 3

        //Observer.remote "interval" obs

        Observable.remote "interval"
        |> Observable.map (fun (i:Int64) -> i)
        |> Observable.add (printfn "%i")

        Observer.remote "interval" obs

        Threading.Thread.Sleep 5000

    [<Test>]
    member x.``Remote Observable`` () =

        let range = Observable.range 0 5

        Observer.remote "range" range

        let obs1 = Observable.remote "range"

        obs1.Observable
        |> Observable.add (printfn "%i")

        obs1.Observable
        |> Observable.add (printfn "%i")

