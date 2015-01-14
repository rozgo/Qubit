module Atom.Comm

type Message = 
    {
      channel : string
      typ : System.Type
      payload : byte array
    }

type IAxonComm =
    abstract member Forward<'MsgName,'MsgType when 'MsgName : equality> 
      : (string -> Message -> unit) -> 'MsgName -> 'MsgType -> unit

type DummyAxonComm () =

  interface IAxonComm with
    member this.Forward<'MsgName,'MsgType when 'MsgName : equality> 
      forwarder (path:'MsgName) (msg:'MsgType) = ()
