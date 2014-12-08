namespace Tests.Definition

open System
open NUnit.Framework

open System.Reactive
open System.Reactive.Linq
open FSharp.Control

open Definition

[<TestFixture>]
type Building () =

    [<Test>]
    member x.``goldCost`` () =

        printfn "Building.goldCost for level %A: %A" 10 (Building.goldCost 10)

        Assert.AreEqual (0, 1)





