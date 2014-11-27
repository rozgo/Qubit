module React

open System
open System.Threading
open System.Net
open Cortex.Signal


open Nessos.FsPickler
//open Nessos.FsPickler.Combinators

open FSharp.Control.Reactive

let Test = 

    let cFreq = Constant.Float 0.1
    let cPhase = Constant.Float 0.
    let cAmplitude = Constant.Float 2.
    let cOffset = Constant.Float 0.

    let osc = Wave.Oscillator (Wave.Form.Sine, 
                cFreq.AsObservable,
                cPhase.AsObservable,
                cAmplitude.AsObservable,
                cOffset.AsObservable,
                false, Time.TotalTime.AsObservable)


    osc.AsObservable |> Observable.add (printfn "sample %f")
//    osc.AsObservable |> Observable.add ignore

//    let w = {

    let fsp = FsPickler.CreateBinary ()
    let bytes = fsp.Pickle osc.Wave
    let up = fsp.UnPickle<Wave.Wave> bytes

    printfn "bytes %i unpickle %A" bytes.Length up

    //Touch.Touches.AsObservable |> Observable.add (printfn "%A")

    let finger0 = Touch.Touches.AsObservable |> Observable.filter (fun t -> if t.finger = Touch.Finger 0 then true else false)
    let finger1 = Touch.Touches.AsObservable |> Observable.filter (fun t -> if t.finger = Touch.Finger 1 then true else false)

    let finger0Moved = finger0 |> Observable.filter (fun t -> t.phase = Touch.Moved)
    let finger1Moved = finger1 |> Observable.filter (fun t -> t.phase = Touch.Moved)

    Observable.merge finger0Moved finger1Moved
    |> Observable.pairwise
    |> Observable.add (printfn "%A")

    let observe = new Builders.ObservableBuilder ()

    let rec generate x =
        observe {
            yield x
            if x < 1 then
                yield! generate (x + 1) }
    generate 5
    |> Observable.add (printfn "Rx: %A")

//    |> Observable.subscribeWithCallbacks ignore ignore ignore
//    |> ignore




let Test2 =

    ()