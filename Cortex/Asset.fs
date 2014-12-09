module Cortex.Asset

open System
open System.Collections.Generic
open System.Threading
open System.IO
open System.Net
open WebSocketSharp

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

    let sources = Dictionary<string,Event<byte array>> ()

    let getSource asset =
        printfn "sources count: %A" sources.Count
        let (hasKey, obs) = sources.TryGetValue asset
        if hasKey then
            obs
        else
            let obs = Event<byte array> ()
            sources.[asset] <- obs
            obs

    let requestQueue = new List<WebRequest> ()

//    let baseUrl = "http://192.168.3.139:8080/"
    let baseUrl = "http://localhost:8080/"

open __

let mutable mainContext : SynchronizationContext = null

let request asset = async {
    printfn "Request %A" asset
    let request = HttpWebRequest.Create (baseUrl + asset)
    let! response = request.AsyncGetResponse ()
    let stream = response.GetResponseStream ()
    let length = (int response.ContentLength)
    let! buffer = readToEnd stream
    return buffer }

let fetch asset = async {
    printfn "Fetch %A" asset
    let request = HttpWebRequest.Create (baseUrl + asset)
    let! response = request.AsyncGetResponse ()
    let stream = response.GetResponseStream ()
    let length = (int response.ContentLength)
    if length > 0 then
        let! buffer = readToEnd stream
        do! Async.SwitchToContext mainContext
        (getSource asset).Trigger buffer
    }

let observe asset =
    Async.Start (fetch asset)
    (getSource asset).Publish

let watching = async {
    let ws = new WebSocket ("ws://localhost:8081/asset")
//    let ws = new WebSocket ("ws://192.168.3.139:8081/asset")
    ws.OnMessage
    |> Observable.add (fun (msg) ->
        printfn "WS Client: %A" msg.Data
        let m = String.Copy msg.Data
        Async.Start (fetch m))
    ws.ConnectAsync ()
    while ws.IsAlive do
        ws.Ping () |> ignore
        do! Async.Sleep 30000 }