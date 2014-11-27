module Cortex.Asset

open System
open System.Collections.Generic
open System.IO
open System.Net
open Cortex.Observable

module private __ =

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

    let sources = Dictionary<string,ObservableSource<byte array>> ()

    let getSource asset =
        printfn "sources count: %A" sources.Count
        let (hasKey, obs) = sources.TryGetValue asset
        if hasKey then
            obs
        else
            let obs = ObservableSource<byte array> ()
            sources.[asset] <- obs
            obs

open __

let requestQueue = new List<WebRequest> ()

let request asset = async {
    printfn "Request %A" asset
//    let request = HttpWebRequest.Create ("http://localhost:8080/" + asset)
    let request = HttpWebRequest.Create ("http://192.168.0.177:8080/" + asset)
    let! response = request.AsyncGetResponse ()
    let stream = response.GetResponseStream ()
    let length = (int response.ContentLength)
    let! buffer = readToEnd stream
    printfn "length reported: %A  length read: %A" length buffer.Length
    return buffer }

let mutable sleeper = 4000

let fetch asset = async {
    sleeper <- sleeper + 2000
    let sleeper = sleeper
    let request = HttpWebRequest.Create ("http://192.168.0.177:8080/" + asset)
    let! response = request.AsyncGetResponse ()
    let stream = response.GetResponseStream ()
    let length = (int response.ContentLength)
    let! buffer = readToEnd stream
//    let r = 2000 + (int ((new Random ()).NextDouble () * 3000.0))
    do! Async.Sleep sleeper
    (getSource asset).Next buffer }

let AsObservable asset =
    Async.Start (fetch asset)
    (getSource asset).AsObservable