namespace GenericTests

open System
open System.Threading
open System.Collections
open System.Collections.Generic
open System.Linq
open System.Reactive.Linq
open System.Reactive.Concurrency
open FSharp.Control.Reactive

open NUnit.Framework

type SM<'State, 'a> = SM of ('State -> 'a * 'State)

type StateBuilder () = 

  member x.Bind (SM c1, fc2) = 
    SM (
      fun s0 -> 
        let (r, s1) = c1 s0
        let (SM c2) = fc2 r
        c2 s1)

  member x.Return k = SM (fun s -> (k, s))

  member x.ReturnFrom (SM k) = SM k

  member x.Combine (m, k) =
     x.Bind (m, fun _ -> k)


[<TestFixture>]

type Test() = 

    let observe = new Builders.ObservableBuilder ()

    let mapper = 
        fun i ->
            printfn "%A" DateTime.Now
            printfn "test"
            i

    [<Test>]
    member x.``Parker Neil Case``() =

        let timedObservables =
            Observable.generateTimeSpan 0 
                (Func<int, bool>(fun (i:int) -> i < 3)) 
                (Func<int, int>((+)1)) mapper
                (fun _ -> new TimeSpan (0,0,1))

        let loopedObservables = 
            timedObservables.SelectMany (fun a ->
                (Observable.range 0 3).SelectMany (fun b -> 
                    let bValue = b
                    Observable.Return ((a,b))))
        loopedObservables |> Observable.add (printfn "%A")

        System.Threading.Thread.Sleep 5000

        Assert.IsTrue false

    [<Test>]
    member x.``Timer``() =
        observe {
            let! a = Observable.Interval (TimeSpan.FromMilliseconds(1.0)) |> Observable.take 3
            let! b = Observable.range 0 3
            return (a,b)
            } |> Observable.add (printfn "%A")
        Thread.Sleep 5000

    [<Test>]
    member x.``SelectMany``() =
        let a = Observable.range 0 3
        Observable.SelectMany (a, fun x ->
            Observable.range 0 3
            |> Observable.map (fun y -> (x, y)))
        |> Observable.add (printfn "%A")

    [<Test>]
    member x.``Linq SelectMany``() =
        let a = [0..2]
        let b = a.SelectMany (fun x -> ([0..2].Select (fun y -> (x, y))))
        printfn "%A" (List.ofSeq b)

    [<Test>]
    member x.``Rozgo Case``() =

        let mapper = 
            fun i ->
                printfn "%A" DateTime.Now
                printfn "test"
                i

        async {

            //do! Async.SwitchToNewThread ()

            observe {
                let! a = Observable.range 0 3
                let! b = Observable.range 0 3
                //return ("abcdefghij".Chars(a),b)}
                return (a,b)}
            |> Observable.add (printfn "%A")


        } |> Async.RunSynchronously 

        //Thread.Sleep 5000

        Assert.IsTrue false

    [<Test>]
    member x.``createWithDisposable`` () =

        let obsA =
            Observable.createWithDisposable (fun subscriber ->
                printfn "subscribed to obsA"
                subscriber.OnNext ("obsA says hi")
                {new IDisposable with
                     member x.Dispose () = printfn "obsA says bye"})

        let obsB =
            Observable.createWithDisposable (fun subscriber ->
                printfn "subscribed to obsB"
                subscriber.OnNext ("obsB says hi")
                {new IDisposable with
                     member x.Dispose () = printfn "obsB says bye"})


        let a = obsA
        let b = obsB

        a
        //|> b
        |> Observable.add (printfn "%A")


        Thread.Sleep 3000

    [<Test>]
    member x.``try catch`` () =

        try
            printfn "will fail now"
            failwith "fail now!!"
        with
            |e -> printfn "%A" e
