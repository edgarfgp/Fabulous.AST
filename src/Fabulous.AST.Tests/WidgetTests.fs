namespace Fabulous.AST.Tests

open Fantomas.Core
open Fantomas.Core.SyntaxOak
open NUnit.Framework

open Fabulous.AST

open type Ast

module WidgetTests =
    [<Test>]
    let ``Produces a top level let binding``() =
        Oak() {
            ModuleOrNamespace() {
                Let("x", "12")
            }
        }
        |> produces """
        
let x = 12

"""
        
    [<Test>]
    let ``Produces a simple hello world console app``() =
        Oak() {
            ModuleOrNamespace() {
                Call("printfn", "\"hello, world\"")
            }
        }
        |> produces """
        
printfn "hello, world"

"""

    [<Test>]
    let ``Produces Hello world with a let binding``() =
        Oak() {
            ModuleOrNamespace() {
                Let("x", "\"hello, world\"")
                Call("printfn", "\"%s\"", "x")
            }
        }
        |> produces """
        
let x = "hello, world"
printfn "%s" x

"""

    [<Test>]
    let ``Produces several Call nodes`` () =
        Oak() {
            ModuleOrNamespace() {
                for i = 0 to 2 do
                    Call("printfn", "\"%s\"", $"{i}")
            }
        }
        |> produces """
        
printfn "%s" 0
printfn "%s" 1
printfn "%s" 2

"""
        
    [<Test>]
    let ``Produces if-then`` () =
        Oak() {
            ModuleOrNamespace() {
                IfThen("x", "=", "12", Expr.For(Unit()))
            }
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