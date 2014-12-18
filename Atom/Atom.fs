module Atom.Util

open System
open System.IO
open System.Threading
open System.Reactive
open System.Reactive.Linq

#nowarn "40"

type Microsoft.FSharp.Control.Async with 
    static member AwaitObservable (obs : IObservable<'T>) =
        let timeReceived = new Event<'T> ()
        let rec trigger t =
            timeReceived.Trigger t
            sub.Dispose ()
        and sub : IDisposable = obs.Subscribe (trigger)
        Async.AwaitEvent (timeReceived.Publish)

let readToEnd (stream:Stream) = async {
    let buffer = Array.zeroCreate 1024
    use output = new MemoryStream ()
    let finished = ref false
    while not finished.Value do
      let! count = stream.AsyncRead (buffer, 0, 1024)
      do! output.AsyncWrite (buffer, 0, count)
      finished := count <= 0
    output.Seek(0L, SeekOrigin.Begin) |> ignore
    use sr = new BinaryReader (output)
    return sr.ReadBytes (int output.Length) }

