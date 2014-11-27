module What

open FSharp.Control.Reactive

let observe = new Builders.ObservableBuilder ()

let rec generate x =
    observe {
        yield x
        if x < 1000 then
            yield! generate (x + 1) }

generate 5
|> Observable.add (printfn "Rx: %A")

