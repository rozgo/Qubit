namespace Neuron

open System
open System.Collections
open System.Collections.Generic

module private __ =

    type __<'T> () =
        static let __ = Dictionary<string,Event<'T>> ()
        static member Dict () = __

    let get<'T> axon =
        let axons = __<'T>.Dict ()
        let (hasKey, observable) = axons.TryGetValue axon
        if hasKey then
            observable
        else
            let observable = Event<'T> ()
            axons.[axon] <- observable
            observable

open __

module Axon =

    let observe<'T> path =
        (get<'T> path).Publish

    let trigger<'T> path msg =
        (get<'T> path).Trigger msg


