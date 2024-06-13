namespace Fabulous.AST.Tests.ModuleDeclarations

open Fabulous.AST.Tests
open Xunit

open Fabulous.AST

open type Ast

module AnonymousModule =
    [<Fact>]
    let ``Produces a simple hello world console app``() =
        Oak() { AnonymousModule() { AppExpr(ConstantExpr(Constant("printfn")), ConstantExpr(String("hello, world"))) } }
        |> produces
            """

printfn "hello, world"

"""

    [<Fact>]
    let ``Produces Hello world with a let binding``() =
        Oak() {
            AnonymousModule() {
                Value(ConstantPat(Constant("x")), ConstantExpr(String("hello, world")))

                AppExpr(ConstantExpr(Constant("printfn")), [ ConstantExpr(String("%s")); ConstantExpr(Constant("x")) ])

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
                    AppExpr(ConstantExpr(Constant("printfn")), [ ConstantExpr(String("%s")); ConstantExpr(Int(i)) ])
            }
        }

        |> produces
            """

printfn "%s" 0
printfn "%s" 1
printfn "%s" 2

"""

    [<Fact>]
    let ``AnonymousModule inside of top level module``() =
        Oak() { TopLevelModule("MyModule") { AnonymousModule() { () } } }
        |> produces
            """
module MyModule
"""
