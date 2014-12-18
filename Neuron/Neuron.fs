namespace Neuron

open System
open System.Collections
open System.Collections.Generic



module private __ =

    type __<'Service,'MsgType when 'Service : equality> () =
        static let __ = Dictionary<'Service,Event<'MsgType>> ()
        static member Dict () = __

    let get<'Service,'MsgType when 'Service : equality> axon =
        let axons = __<'Service,'MsgType>.Dict ()
        let (hasKey, observable) = axons.TryGetValue axon
        if hasKey then
            observable
        else
            let observable = Event<'MsgType> ()
            axons.[axon] <- observable
            observable

open __

module Axon =

    let observe<'Service,'MsgType when 'Service : equality> path =
        (get<'Service,'MsgType> path).Publish

    let trigger<'Service,'MsgType when 'Service : equality> path msg =
        (get<'Service,'MsgType> path).Trigger msg

