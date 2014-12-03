namespace Tests

open System
open NUnit.Framework

open Signal

[<TestFixture>]
type SignalTests () =

    [<Test>]
    member x.``Floor Function`` () =
        let op = signal {func = Floor; freq = 1.0; phase = 0.0; amp = 1.0; offset = 0.0}
        let r0 = [-0.5; 0.5; 1.5; 2.5; 3.5] |> List.map (sample op)
        Assert.AreEqual (r0, [-1.0; 0.0; 1.0; 2.0; 3.0])

    [<Test>]
    member x.``Sine Function`` () =
        let s0 = signal {func = Sine; freq = 1.0; phase = 0.0; amp = 1.0; offset = 0.0}
        let r0 = [-Math.PI/2.0; 0.0; Math.PI/2.0] |> List.map (sample s0)
        Assert.AreEqual (r0, [-1.0; 0.0; 1.0])

    [<Test>]
    member x.``Add Signals`` () =
        let sampler = {func = Sine; freq = 1.0; phase = 0.0; amp = 1.0; offset = 0.0}
        let s0 = signal sampler
        let s1 = signal {sampler with amp = -1.0}
        let inputs = [-Math.PI/2.0; 0.0; Math.PI/2.0]
        let r0 = inputs |> List.map (sample (Add (s0, s1)))
        let r1 = inputs |> List.map (sample (Add (s0, s0)))
        Assert.AreEqual (r0, [0.0; 0.0; 0.0])
        Assert.AreEqual (r1, [-2.0; 0.0; 2.0])

    [<Test>]
    member x.``Compose Signals`` () =
        let s0 = signal {func = Sine; freq = 1.0; phase = 0.0; amp = 1.0; offset = 0.0}
        let s1 = signal {func = Floor; freq = 1.0; phase = 0.0; amp = 1.0; offset = 0.0}
        let inputs = [-Math.PI/2.0; 0.0; Math.PI/2.0]
        let r0 = inputs |> List.map (sample (Compose (s0, s1)))
        let r0' = inputs |> List.map (Math.Sin << Math.Floor)
        let r1 = inputs |> List.map (sample (Compose (s1, s0)))
        let r1' = inputs |> List.map (Math.Sin >> Math.Floor)
        Assert.AreEqual (r0, r0')
        Assert.AreEqual (r1, r1')

    [<Test>]
    member x.``Custom Function`` () =
        let s0 = signal {func = Sine; freq = 1.0; phase = 0.0; amp = -1.0; offset = 0.0}
        let op = Add (s0, Sample (fun x -> Math.Sin x))
        let r0 = sample op 1.0
        Assert.AreEqual (r0, 0.0)

