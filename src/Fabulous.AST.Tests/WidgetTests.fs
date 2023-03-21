namespace Fabulous.AST.Tests

open Fantomas.Core
open NUnit.Framework

open Fabulous.AST

open type Ast

module WidgetTests =
    [<Test>]
    let ``Produces a top level let binding``() =
        ImplicitModule() {
            Let("x", "12")
        }
        |> produces """
        
let x = 12

"""
        
    [<Test>]
    let ``Produces a simple hello world console app``() =
        ImplicitModule() {
            Call("printfn", "\"hello, world\"")
        }
        |> produces """
        
printfn "hello, world"

"""

    [<Test>]
    let ``Produces Hello world with a let binding``() =
        ImplicitModule() {
            Let("x", "\"hello, world\"")
            Call("printfn", "\"%s\"", "x")
        }
        |> produces """
        
let x = "hello, world"
printfn "%s" x

"""

    [<Test>]
    let ``Produces several Call nodes`` () =
        ImplicitModule() {
            for i = 0 to 2 do
                Call("printfn", "\"%s\"", $"{i}")
        }
        |> produces """
        
printfn "%s" 0
printfn "%s" 1
printfn "%s" 2

"""
        
    [<Test>]
    let ``Produces if-then`` () =
        ImplicitModule() {
            IfThen("x", "=", "12", UnitExpr(Unit()))
        }
        |> produces """

if x = 12 then
    ()

"""

    [<Test>]
    let z () =
        let source =
            """

if x = 1 then
    ()

"""
        let rootNode = CodeFormatter.ParseOakAsync(false, source) |> Async.RunSynchronously    
        Assert.NotNull(rootNode)