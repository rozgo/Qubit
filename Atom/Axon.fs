module Atom.Axon

open System
open System.Collections
open System.Collections.Generic

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

open __

let observe<'MsgName,'MsgType when 'MsgName : equality> path =
    (get<'MsgName,'MsgType> path).Publish

let trigger<'MsgName,'MsgType when 'MsgName : equality> path msg =
    (get<'MsgName,'MsgType> path).Trigger msg


