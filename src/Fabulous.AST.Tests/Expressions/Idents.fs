namespace Fabulous.AST.Tests.Expressions

open Xunit
open Fabulous.AST.Tests

open Fabulous.AST

open type Ast

module LongIdentSet =

    [<Fact>]
    let ``LongIdentSet expression``() =
        Oak() {
            AnonymousModule() {
                LongIdentSetExpr("a", ConstantExpr(Constant("b")))

            }
        }
        |> produces
            """
a <- b
"""

module Ident =

    [<Fact>]
    let ``Ident expression``() =
        Oak() {
            AnonymousModule() {
                IdentExpr("a")

            }
        }
        |> produces
            """
a
"""

module ParenILEmbedded =

    [<Fact>]
    let ``ParenILEmbedded expression``() =
        Oak() {
            AnonymousModule() {
                ParenILEmbeddedExpr("a")

            }
        }
        |> produces
            """
a
"""
