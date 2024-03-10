namespace Fabulous.AST.Tests.ModuleDeclarations

open Fabulous.AST.Tests
open Xunit

open Fabulous.AST

open type Ast

module AnonymousModule =
    [<Fact>]
    let ``Produces a simple hello world console app`` () =
        AnonymousModule() { AppExpr("printfn") { ConstantExpr("hello, world") } }
        |> produces
            """
        
printfn "hello, world"

"""

    [<Fact>]
    let ``Produces Hello world with a let binding`` () =
        AnonymousModule() {
            Value("x", "hello, world", true)

            AppExpr("printfn") {
                ConstantExpr("%s")
                ConstantExpr("x", false)
            }
        }
        |> produces
            """
        
let x = "hello, world"
printfn "%s" x

"""

    [<Fact>]
    let ``Produces several Call nodes`` () =
        AnonymousModule() {
            for i = 0 to 2 do
                AppExpr("printfn") {
                    ConstantExpr("%s")
                    ConstantExpr($"{i}", false)
                }
        }
        |> produces
            """
        
printfn "%s" 0
printfn "%s" 1
printfn "%s" 2

"""
