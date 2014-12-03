module Atom.Async

open System
open System.Reactive
open System.Reactive.Linq

#nowarn "40"

let synchronize f = 
  let ctx = System.Threading.SynchronizationContext.Current 
  f (fun g arg ->
    let nctx = System.Threading.SynchronizationContext.Current 
    if ctx <> null && ctx <> nctx then ctx.Post((fun _ -> g(arg)), null)
    else g(arg) )

type Microsoft.FSharp.Control.Async with 
  static member AwaitObservable(evt:IObservable<'a>) =
    synchronize (fun f ->
      Async.FromContinuations((fun (cont,econt,ccont) -> 
        let rec callback = (fun value ->
          remover.Dispose()
          f cont value )
        and remover : IDisposable  = evt.Subscribe(callback) 
        () )))

type System.Reactive.Linq.Observable with 
  static member FromAsync computation = 
      Observable.Create<'a> (Func<IObserver<'a>, Action>(fun o ->
        if o = null then nullArg "observer"
        let cts = new System.Threading.CancellationTokenSource ()
        let invoked = ref 0
        let cancelOrDispose cancel =
          if Threading.Interlocked.CompareExchange (invoked, 1, 0) = 0 then
            if cancel then cts.Cancel () else cts.Dispose ()
        let wrapper = async {
          try
            let res = ref Unchecked.defaultof<_>
            try
              let! result = computation
              res := result
            with e -> o.OnError (e)
            o.OnNext (!res)
            o.OnCompleted ()
          finally cancelOrDispose false }
        Async.StartImmediate (wrapper, cts.Token)
        Action(fun () -> cancelOrDispose true)))