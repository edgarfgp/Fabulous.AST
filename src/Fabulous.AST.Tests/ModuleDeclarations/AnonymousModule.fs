namespace Fabulous.AST.Tests.ModuleDeclarations

open Fabulous.AST.Tests
open Fantomas.Core.SyntaxOak
open Xunit

open Fabulous.AST

open type Ast

module AnonymousModule =
    [<Fact>]
    let ``Produces a simple hello world console app``() =
        Oak() { AnonymousModule() { AppExpr("printfn") { ConstantExpr("hello, world") } |> _.hasQuotes(false) } }
        |> produces
            """

printfn "hello, world"

"""

    [<Fact>]
    let ``Produces Hello world with a let binding``() =
        Oak() {
            AnonymousModule() {
                Value("x", "hello, world")

                AppExpr("printfn") {
                    ConstantExpr("%s")
                    ConstantExpr("x").hasQuotes(false)
                }
                |> _.hasQuotes(false)
            }
        }

        |> produces
            """

let x = "hello, world"
printfn "%s" x

"""

    [<Fact>]
    let ``Produces several Call nodes``() =
        Oak() {
            AnonymousModule() {
                for i = 0 to 2 do
                    AppExpr("printfn") {
                        ConstantExpr("%s")
                        ConstantExpr($"{i}").hasQuotes(false)
                    }
                    |> _.hasQuotes(false)
            }
        }

        |> produces
            """

printfn "%s" 0
printfn "%s" 1
printfn "%s" 2

"""
