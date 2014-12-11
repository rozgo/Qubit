module Cortex.Asset

open System
open System.Collections.Generic
open System.Threading
open System.IO
open System.Net
open WebSocketSharp
open Neuron

module private __ =

//    let baseUrl = "http://192.168.4.110:8080/"
    let baseUrl = "http://localhost:8080/"

open __

let mutable mainContext : SynchronizationContext = null

let request asset = async {
    printfn "Request %A" asset
    let request = HttpWebRequest.Create (baseUrl + asset)
    let! response = request.AsyncGetResponse ()
    let stream = response.GetResponseStream ()
    let length = (int response.ContentLength)
    let! buffer = Atom.readToEnd stream
    return buffer }

let fetch asset = async {
    printfn "Fetch %A" asset
    let request = HttpWebRequest.Create (baseUrl + asset)
    let! response = request.AsyncGetResponse ()
    let stream = response.GetResponseStream ()
    let length = (int response.ContentLength)
    if length > 0 then
        let! buffer = Atom.readToEnd stream
        do! Async.SwitchToContext mainContext
        Axon.trigger<byte array> ("asset/changed:" + asset) buffer }

let observe asset : IEvent<byte array> =
    Async.Start (fetch asset)
    Axon.observe<unit> ("fswatch:" + asset)
    |> Observable.add (fun () -> Async.Start (fetch asset))
    Axon.observe<byte array> ("asset/changed:" + asset)

let watching = async {
    let ws = new WebSocket ("ws://localhost:8081/asset")
//    let ws = new WebSocket ("ws://192.168.4.110:8081/asset")
    ws.OnMessage
    |> Observable.add (fun (msg) ->
        Axon.trigger<unit> msg.Data ())
    ws.ConnectAsync ()
    while ws.IsAlive do
        ws.Ping () |> ignore
        do! Async.Sleep 30000 }