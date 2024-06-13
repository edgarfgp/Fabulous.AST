namespace Fabulous.AST.Tests.Expressions

open Xunit
open Fabulous.AST.Tests

open Fabulous.AST

open type Ast

module IndexFromEnd =

    [<Fact>]
    let ``IndexFromEnd expression``() =
        Oak() { AnonymousModule() { IndexFromEndExpr(ConstantExpr(Int(0))) } }
        |> produces
            """
^0
"""
