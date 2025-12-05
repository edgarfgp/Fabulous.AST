namespace Fabulous.AST.Tests.Expressions

open Xunit
open Fabulous.AST.Tests

open Fabulous.AST

open type Ast

module Expr =

    [<Fact>]
    let ``yield! multiple expressions``() =
        Oak() {
            AnonymousModule() {
                yield!
                    [ String "A"; String "B"; String "C"; Int(0); Bool(false); Float(9.0) ]
                    |> List.map ConstantExpr
            }
        }
        |> produces
            """
"A"
"B"
"C"
0
false
9.0
"""
