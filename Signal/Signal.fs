module Signal

open System

type Function =
    | Line
    | Sine
    | Square
    | Triangle
    | Sawtooth
    | Pulse
    | Floor

type Op =
    | Sample of (float -> float)
    | Add of Op * Op
    | Multiply of Op * Op
    | Compose of Op * Op
    | Op of Op

type Sampler =
    {
    func : Function
    freq : float
    phase : float
    amp : float
    offset : float
    }

let signal sampler =
    Sample (fun time -> 
        let time = sampler.freq * time + sampler.phase
        let sample = 
            match sampler.func with
            | Line -> time
            | Sine -> Math.Sin time
            | Square -> float (Math.Sign (Math.Sin (2.0 * Math.PI * time)))
            | Triangle -> 1.0 - 4.0 * Math.Abs (Math.Round (time - 0.25) - (time - 0.25))
            | Sawtooth -> 2.0 * (time - Math.Floor (time + 0.5))
            | Pulse -> if Math.Abs (Math.Sin (2.0 * Math.PI * time)) < 1.0 - 10E-3 then 0.0 else 1.0
            | Floor -> Math.Floor time
        sample * sampler.amp + sampler.offset)

let rec sample op time =
    match op with
    | Add (lhs , rhs) -> (sample lhs time) + (sample rhs time)
    | Multiply (lhs, rhs) -> (sample lhs time) * (sample rhs time)
    | Compose (lhs, rhs) -> (sample lhs << sample rhs) time
    | Sample func -> func time
    | Op op -> sample op time

