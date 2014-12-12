open System
open FSharp.Control.Reactive


[<EntryPoint>]
let main argv = 

    let observe = new Builders.ObservableBuilder ()

    printfn "%A" argv

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

    printfn "Press return to exit..."
    Console.ReadLine () |> ignore

    0

