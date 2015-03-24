module Cortex.Asset

open System
open System.Collections.Generic
open System.Threading
open System.IO
open System.Net
open WebSocketSharp
open Atom

module private __ =

//    let baseUrl = "http://192.168.3.156:8080/"
    let baseUrl = "http://localhost:8080/"

open __

let mutable mainContext : SynchronizationContext = null

let request asset = async {
    printfn "Request %A" asset
    let request = HttpWebRequest.Create (baseUrl + asset)
    let! response = request.AsyncGetResponse ()
    let stream = response.GetResponseStream ()
    let length = (int response.ContentLength)
    let! buffer = Atom.Util.readToEnd stream
    return buffer }

let fetch asset = async {
    printfn "Fetch %A" asset
    let request = HttpWebRequest.Create (baseUrl + asset)
    let! response = request.AsyncGetResponse ()
    let stream = response.GetResponseStream ()
    let length = (int response.ContentLength)
    if length > 0 then
        let! buffer = Atom.Util.readToEnd stream
        do! Async.SwitchToContext mainContext
        Axon.trigger ("asset/changed:" + asset) buffer }

let observe asset : IEvent<byte array> =
    Async.Start (fetch asset)
    Axon.observe ("fswatch:" + asset)
    |> Observable.add (fun () -> Async.Start (fetch asset))
    Axon.observe ("asset/changed:" + asset)

let watching = async {
    let ws = new WebSocket ("ws://localhost:8081/asset")
//    let ws = new WebSocket ("ws://192.168.3.156:8081/asset")
    ws.OnMessage
    |> Observable.add (fun (msg) ->
        Axon.trigger msg.Data ())
    ws.ConnectAsync ()
    while ws.IsAlive do
        ws.Ping () |> ignore
        do! Async.Sleep 30000 }