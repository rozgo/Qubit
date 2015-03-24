module Profiler

open System.Diagnostics
open System.Collections.Generic
open WebSocketSharp
open WebSocketSharp.Server

type private Stat =
    {
        thread : int
        frame : int
        duration : int
        depth : int
        count : int
    }

let mutable (wss:WebSocketBehavior option) = None

type private WebSocketStats () =
    inherit WebSocketBehavior ()

    member this.Cast (json:string) = this.Send json

    override this.OnOpen () =
        wss <- Some (this :> WebSocketBehavior)
        printfn "WebSocket Open"

    override this.OnClose (e: CloseEventArgs) =
        printfn "WebSocket Close %A" e

    override this.OnMessage (e:MessageEventArgs) =
        printfn "WebSocket Message %A" e

let mutable private time = 0.
let mutable private frame = 0
let mutable private depth = 0

let private stats = new Dictionary<string,Stat> ()

let private ws =
    let ws = new WebSocketServer ("ws://localhost:8888")
    ws.KeepClean <- false
    ws.AddWebSocketService<WebSocketStats> ("/stats", fun () -> new WebSocketStats ())
    ws.Start ()
    ws

type Sample (label:string) =

    let sw = new Stopwatch ()
    let thread = System.Threading.Thread.CurrentThread.ManagedThreadId
    let stat = stats.TryGetValue(label)

    do
        depth <- depth + 1
        sw.Start ()

    interface System.IDisposable with 
        member this.Dispose() = 
            sw.Stop ()
            let count =
                match stat with
                | (true, stat) -> stat.count + 1
                | _ -> 1
            let stat = {
                thread = thread
                frame = frame
                duration = sw.Elapsed.Milliseconds
                depth = depth
                count = count }
            stats.[label] <- stat
            depth <- depth - 1

let sample label = new Sample (label)

let private json (label:string) (stat:Stat) =
    sprintf """ {
    "label": "%s",
    "thread": %i,
    "frame": %i,
    "duration": %i,
    "depth": %i,
    "count": %i
}
    """ label stat.thread stat.frame stat.duration stat.depth stat.count

let private msg = new System.Text.StringBuilder ()

let tick deltaTime =
    time <- time + deltaTime
    frame <- frame + 1
    depth <- 0
    if time > 2. then
        time <- time - 2.
        msg.Clear () |> ignore
        msg.Append "[" |> ignore
        let mutable first = true
        for stat in stats do
            if not first then
                msg.Append ",\n" |> ignore
            first <- false
            let json = json stat.Key stat.Value
            msg.Append json |> ignore
            //printfn "PROFILER: %A %A" stat.Key stat.Value
            //printfn "JSON: %A" json
        msg.Append "]" |> ignore
        let json = msg.ToString ()
        printfn "JSON: %A" json
        match wss with
        | Some wss ->
            let wss = wss :?> WebSocketStats
            wss.Cast json
        | _ -> ()
    stats.Clear ()
