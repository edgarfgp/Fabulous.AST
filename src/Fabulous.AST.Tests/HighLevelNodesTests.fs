namespace Fabulous.AST.Tests

open Fabulous.AST
open type Fabulous.AST.Ast
open NUnit.Framework

module HighLevelNodesTests =
    [<Test>]
    let ``Can create a top level let binding inside an implicit module``() =
        ImplicitModule() {
            Let("x", "12")
        }
        |> produces """
        
let x = 12

"""

    [<Test>]
    let ``Can write a simple hello world console app``() =
        ImplicitModule() {
            Call("printfn", "\"hello, world\"")
        }
        |> produces """
        
printfn "hello, world"

"""

    [<Test>]
    let ``Hello world with a let binding``() =
        ImplicitModule() {
            Let("x", "\"hello, world\"")
            Call("printfn", "\"%s\"", "x")
        }
        |> produces """
        
let x = "hello, world"
printfn "%s" x

"""

    [<Test>]
    let CanCompileBasicTree11 () =
        ImplicitModule() {
            for i = 0 to 2 do
                Call("printfn", "\"%s\"", $"{i}")
        }
        |> produces """
        
printfn "%s" 0
printfn "%s" 1
printfn "%s" 2

"""
