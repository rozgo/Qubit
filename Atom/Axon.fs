module Atom.Axon

open System
open System.Collections.Generic
open Nessos.FsPickler
open Nessos.FsPickler.Json
open Newtonsoft.Json

type Message<'T> = 
    {
      channel : 'T
      typ : System.Type
      payload : byte array
    }

module private __ =
   
    type __<'MsgName,'MsgType when 'MsgName : equality> () =
        static let __ = Dictionary<'MsgName, Event<'MsgType>> ()
        static member Dict () = __

    let get<'MsgName,'MsgType when 'MsgName : equality> axon =
        let axons = __<'MsgName,'MsgType>.Dict ()
        let (hasKey, observable) = axons.TryGetValue axon
        if hasKey then
            observable
        else
            let observable = Event<'MsgType> ()
            axons.[axon] <- observable
            observable
    
    let Compare (path:obj)  =
        let serverPattern = [":OnNext";":OnCompleted";":OnError"]
        let clientPattern = ":OnSubscribed"
        match path with
        | :? string as strPath ->
            let serverForward = List.fold (fun state pattern -> state || strPath.Contains(pattern)) false serverPattern
            let clientForward = strPath.Contains(clientPattern)
            (serverForward,"serverSend"),(clientForward,"clientSend")
        | _ -> (false,""),(false,"")

    let CreateMessage<'MsgName,'MsgType when 'MsgName : equality> (path:'MsgName) (msg:'MsgType) =
        let json = FsPickler.CreateJson (omitHeader = true)
        let typ = typeof<'MsgType>
        let ms = new System.IO.MemoryStream ()
        json.Serialize<'MsgType>(ms,msg)
        let payload = ms.ToArray ()
        let message = {channel = path;typ = typ;payload = payload}
        message

open __

let observe<'MsgName,'MsgType when 'MsgName : equality> path =
    (get<'MsgName,'MsgType> path).Publish

let trigger<'MsgName,'MsgType when 'MsgName : equality> path msg =
    (get<'MsgName,'MsgType> path).Trigger msg
    match Compare path with
    | (true,channel),(_)
    | (_),(true,channel) -> 
        let message = CreateMessage<'MsgName,'MsgType> path msg
        (get<string,Message<'MsgName>> channel).Trigger message
    | _ -> ()

