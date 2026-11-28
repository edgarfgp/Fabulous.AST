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
        |> producesValid
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
        |> producesValid
            """
a
"""

module ParenILEmbedded =

    [<Fact>]
    let ``ParenILEmbedded expression``() =
        Oak() {
            AnonymousModule() {
                Value("a", ParenILEmbeddedExpr("\"ldnull\" : 'T"))
                Value("b", ParenILEmbeddedExpr("(# \"add\" x y : int #)"))
            }
        }
        |> producesValid
            """
let a = (# "ldnull" : 'T #)
let b = (# "add" x y : int #)
"""
