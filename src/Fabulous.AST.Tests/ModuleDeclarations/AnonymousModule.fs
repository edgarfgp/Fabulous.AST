namespace Fabulous.AST.Tests.ModuleDeclarations

open Fabulous.AST.Tests
open NUnit.Framework

open Fabulous.AST

open type Ast

module AnonymousModule =
    [<Test>]
    let ``Produces a simple hello world console app`` () =
        AnonymousModule() { AppExpr("printfn") { ConstantStringExpr("hello, world") } }
        |> produces
            """
        
printfn "hello, world"

"""

    [<Test>]
    let ``Produces Hello world with a let binding`` () =
        AnonymousModule() {
            Value("x", "hello, world", true)

            AppExpr("printfn") {
                ConstantStringExpr("%s")
                ConstantExpr("x")
            }
        }
        |> produces
            """
        
let x = "hello, world"
printfn "%s" x

"""

    [<Test>]
    let ``Produces several Call nodes`` () =
        AnonymousModule() {
            for i = 0 to 2 do
                AppExpr("printfn") {
                    ConstantStringExpr("%s")
                    ConstantExpr($"{i}")
                }
        }
        |> produces
            """
        
printfn "%s" 0
printfn "%s" 1
printfn "%s" 2

"""
