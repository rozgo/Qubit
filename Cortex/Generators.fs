module Cortex.Generators

open System
open OpenTK
open Cortex.Observable

module Constant =

    type Float (num) =
        let source = ObservableSource<float> ()
        do source.Next num
        member this.Next num = source.Next num
        member this.AsObservable = source.AsObservable

module Time =

    let DeltaTime = ObservableSource<float> ()

    let TotalTime = ObservableSource<float> ()

    type Global () =
        
        static let mutable time = 0.0
        static let mutable lastTime = 0.0

        static member _Time
            with get () = time
            and set v =
                lastTime <- time
                time <- v
                TotalTime.Next (time)
                DeltaTime.Next (time - lastTime)

module Wave =

    type Form =
        | Sine
        | Square
        | Triangle
        | Sawtooth
        | Pulse

    type Wave =
        {
        form : Form
        frequency : float
        phase : float
        amplitude : float
        offset : float
        inverted : bool
        }

    let sample wave time =
        let time' = wave.frequency * time + wave.phase
        let sample =
            match wave.form with
            | Sine -> Math.Sin (2.0 * Math.PI * time')
            | Square -> float (Math.Sign (Math.Sin (2.0 * Math.PI * time')))
            | Triangle -> 1.0 - 4.0 * Math.Abs (Math.Round (time' - 0.25) - (time' - 0.25))
            | Sawtooth -> 2.0 * (time' - Math.Floor (time' + 0.5))
            | Pulse -> if Math.Abs (Math.Sin (2.0 * Math.PI * time')) < 1.0 - 10E-3 then 0.0 else 1.0
        sample * wave.amplitude * (if wave.inverted then -1.0 else 1.0) + wave.offset

    type Oscillator (form, oFrequency, oPhase, oAmplitude, oOffset, inverted, oTime) =

        let mutable wave = {form = form; frequency = 1.0; phase = 0.0; amplitude = 1.0; offset = 0.0; inverted = inverted;}
        let source = ObservableSource<float> ()

        let Sample time = 
            source.Next <| sample wave time

        let o0 = oFrequency |> Observable.subscribe (fun n -> wave <- {wave with frequency = n})
        let o1 = oPhase |> Observable.subscribe (fun n -> wave <- {wave with phase = n})
        let o2 = oAmplitude |> Observable.subscribe (fun n -> wave <- {wave with amplitude = n})
        let o3 = oOffset |> Observable.subscribe (fun n -> wave <- {wave with offset = n})
        let o4 = oTime |> Observable.subscribe (fun n -> Sample n)

        member this.Wave = wave
        member this.AsObservable = source.AsObservable

module Touch =

    type Phase = Began | Moved | Ended | Cancelled

    type Finger = Finger of int

    type Touch =
        {
        finger : Finger
        phase : Phase
        position : Vector3
        }

    let Touches = ObservableSource<Touch> ()




