open System
open System.Threading
open System.Collections
open System.Collections.Generic
open System.Linq
open System.Reactive.Linq
open System.Reactive.Concurrency
open System.Reactive.Disposables
open FSharp.Control.Reactive
open RenderStateModad


//type AmbientColor (color : string) =
//    interface IRenderState<string> with
//        member this.OnEnterState () = printfn "set ambient color"
//        member this.OnExitState () = printfn "clear ambient color"
//
//
//type RenderStateBuilder () =
//
//    member this.Bind (m:IRenderState<'I>, func:'I -> IRenderState<'O>) : IRenderState<'O> =
//        m.OnEnterState ()
//        let ot = func.Invoke (m)
//        m.OnExitState ()
//        ot

module __ =

    type State<'s,'a> = State of ('s -> 'a * 's)

    type StateBuilder<'s>() =
      member x.Return v : State<'s,_> = State(fun s -> v,s)
      member x.Bind(State v, f) : State<'s,_> =
        State(fun s ->
          let (a,s) = v s
          let (State v') = f a
          v' s)

    let withState<'s> = StateBuilder<'s>()

    let getState = State(fun s -> s,s)
    let putState v = State(fun _ -> (),v)

    let runState (State f) init = f init

    type Location = Room | Garden
    type Thing = { Name : string; Article : string }
    type Player = { Location : Location; Objects : Thing list }

open __

[<EntryPoint>]
let main argv = 

    let pickUp thing =
      withState {
        let! (player, objects:Map<_,_>) = getState
        let objs = objects.[player.Location]
        let attempt = objs |> List.partition (fun o -> o.Name = thing)    
        match attempt with    
        | [], _ -> 
            return "You cannot get that."
        | thing :: _, things ->    
            let player' = { player with Objects = thing :: player.Objects }        
            let objects' = objects.Add(player.Location, things)
            let msg = sprintf "You are now carrying %s %s" thing.Article thing.Name
            do! putState (player', objects')
            return msg
      }

    let player = { Location = Room; Objects = [] }   
    let objects =
      [Room, [{ Name = "whiskey"; Article = "some" }; { Name = "bucket"; Article = "a" }]
       Garden, [{ Name = "chain"; Article = "a length of" }]]    
      |> Map.ofList

    let (msg, (player', objects')) = 
      (player, objects)
      |> runState (pickUp "bucket")

    Test.renderSomething ()

    printfn "Press return to exit..."
    Console.ReadLine () |> ignore

    0

