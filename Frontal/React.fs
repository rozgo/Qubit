module React

open System
open System.Threading
open System.Net
open Cortex.Generators

open Nessos.FsPickler
open Nessos.FsPickler.Combinators

//let guard f (e:IObservable<'Args>) =  
//  { new IObservable<'Args> with  
//      member x.Subscribe(observer) =  
//        let rm = e.Subscribe(observer) in f(); rm }
//
//let ofSeq s = 
//    let evt = new Event<_>()
//    evt.Publish |> guard (fun o ->for n in s do evt.Trigger(n))

let createTimer timerInterval =

    let timer = new System.Timers.Timer(float timerInterval)
    timer.AutoReset <- true

    let observable = timer.Elapsed

    let task = async {
        timer.Start()
        do! Async.Sleep 5000
        timer.Stop()
        }
    (task, observable)

let AsyncHttp(url:string) =
    async {  let req = WebRequest.Create(url)
             let! rsp = req.AsyncGetResponse ()
             use stream = rsp.GetResponseStream ()
             use reader = new System.IO.StreamReader (stream)
             return reader.ReadToEnd () }

let workThenWait() = 
  Thread.Sleep(1000)
  printfn "work done"
  async { do! Async.Sleep(1000) }

let demo() = 
  let work = workThenWait() |> Async.StartAsTask
  printfn "started"
  work.Wait()
  printfn "completed"

let Test = 
//    let basicHandler _ = printfn "tick %A" DateTime.Now

//    let basicTimer, timerEventStream = createTimer 1000
//
//    timerEventStream
//    |> Observable.scan (fun count _ -> count + 1) 0
//    |> Observable.subscribe (fun count -> printfn "timer ticked with count %i" count)
//    |> ignore

//    let o1 = seq { for i in [-10..0] -> i } |> ofSeq
//
//    let o2 = seq { for i in [0..10] -> i } |> ofSeq
//
//
//    Observable.merge o1 o2
//    |> Observable.filter (fun n -> n%2 = 0)
//    |> Observable.add (printfn "ofSeq %d")

    let s = AsyncHttp "http://brink-dev.beyondgames.io/t" |> Async.StartAsTask
    //let s = workThenWait |> Async.StartAsTask
    printfn "http %s" s.Result

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
//    |> Observable.scan
    |> Observable.add (printfn "%A")
//
//    finger0 |> Observable.add (printfn "finger0 %A")

//    Async.RunSynchronously (async { do! Async.Sleep(15000) })
//    printfn "done waiting"
//    cAmplitude.Next 0.0

//    dt |> Observable.wave.Sample

    //let basicTimer1 = createTimer 1000 basicHandler

//    Async.RunSynchronously basicTimer



  
let AsyncTest =
    let a = async {
        do! Async.Sleep 5000
    }

    ()