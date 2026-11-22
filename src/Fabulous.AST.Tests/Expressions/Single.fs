namespace Fabulous.AST.Tests.Expressions

open Xunit
open Fabulous.AST.Tests

open Fabulous.AST

open type Ast

module Single =
    [<Fact>]
    let ``let value with a Single expression``() =
        Oak() {
            AnonymousModule() {
                Value(
                    ConstantPat(Constant("x")),
                    SingleExpr(SingleNode("a", ConstantExpr(Constant("b"))).addSpace(true).supportsStroustrup(false))

                )

                Value(
                    ConstantPat(Constant("x")),
                    SingleExpr(SingleNode("a", Constant("b")).addSpace(true).supportsStroustrup(false))
                )

                Value(
                    ConstantPat(Constant("x")),
                    SingleExpr(SingleNode("a", "b").addSpace(true).supportsStroustrup(false))
                )
            }
        }
        |> producesValid
            """

let x = a b
let x = a b
let x = a b
"""

    [<Fact>]
    let ``Keywords are separated from their expression by default``() =
        Oak() {
            AnonymousModule() {
                Value("a", NamedComputationExpr("async", SingleExpr("return", "x")))
                Value("b", SeqExpr([ SingleExpr("yield", Int(1)); SingleExpr("yield!", "rest") ]))
                Value("c", SingleExpr(DoExpr(AppExpr("printfn", String("hi")))))
                Value("d", SingleExpr("lazy", Int(1)))
            }
        }
        |> producesValid
            """
let a = async { return x }

let b =
    seq {
        yield 1
        yield! rest
    }

let c = do printfn "hi"
let d = lazy 1
"""

    [<Fact>]
    let ``Symbols are not separated from their expression by default``() =
        Oak() { AnonymousModule() { Value("p", SingleExpr("&&", "x")) } }
        |> produces
            """
let p = &&x
"""

    [<Fact>]
    let ``addSpace overrides the default``() =
        Oak() { AnonymousModule() { Value("p", SingleExpr(SingleNode("return", "x").addSpace(false))) } }
        |> produces
            """
let p = returnx
"""
