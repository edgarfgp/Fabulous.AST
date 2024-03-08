namespace Fabulous.AST.Tests.Patterns

open NUnit.Framework
open Fabulous.AST.Tests

open Fabulous.AST

open type Ast

module StructTuplePat =

    [<Test>]
    let ``let value with a StructTuple pattern`` () =
        AnonymousModule() {
            Value(
                StructTuplePat() {
                    NamedPat("a")
                    NamedPat("b")
                },
                ConstantExpr(Constant("12", false))
            )
        }
        |> produces
            """

let struct (a, b) = 12
"""
