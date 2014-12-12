namespace GenericTests

open System
open NUnit.Framework
open FSharp.Control.Reactive

[<TestFixture>]

type Test() = 

    let observe = new Builders.ObservableBuilder ()

    [<Test>]
    member x.TestCase() =



        Assert.IsTrue(true)

