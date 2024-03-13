namespace Fabulous.AST.Tests.ModuleDeclarations

open Fabulous.AST.Tests
open Xunit

open Fabulous.AST

open type Ast

module AnonymousModule =
    [<Fact>]
    let ``Produces a simple hello world console app``() =
        AnonymousModule() { AppExpr("printfn") { ConstantExpr("hello, world") } |> _.hasQuotes(false) }
        |> produces
            """

printfn "hello, world"

"""

    [<Fact>]
    let ``Produces Hello world with a let binding``() =
        AnonymousModule() {
            Value("x", "hello, world")

            AppExpr("printfn") {
                ConstantExpr("%s")
                ConstantExpr("x").hasQuotes(false)
            }
            |> _.hasQuotes(false)
        }
        |> produces
            """

let x = "hello, world"
printfn "%s" x

"""

    [<Fact>]
    let ``Produces several Call nodes``() =
        AnonymousModule() {
            for i = 0 to 2 do
                AppExpr("printfn") {
                    ConstantExpr("%s")
                    ConstantExpr($"{i}").hasQuotes(false)
                }
                |> _.hasQuotes(false)
        }
        |> produces
            """

printfn "%s" 0
printfn "%s" 1
printfn "%s" 2

"""
