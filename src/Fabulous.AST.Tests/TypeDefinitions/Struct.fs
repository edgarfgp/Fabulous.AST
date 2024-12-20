namespace Fabulous.AST.Tests.TypeDefinitions

open Fabulous.AST.Tests
open Xunit

open Fabulous.AST
open type Ast

module Struct =
    [<Fact>]
    let ``Produces a verbose struct``() =
        Oak() {
            AnonymousModule() {
                StructEnd("X") {
                    ValField("x", Int()).toMutable()

                    Constructor(ParenPat("x"), RecordExpr([ RecordFieldExpr("x", "x") ]))
                }

                StructEnd("Y") {
                    ValField("x", Int()).toMutable()

                    Constructor(ParenPat("x"), RecordExpr([ RecordFieldExpr("x", "x") ]))
                }
            }
        }
        |> produces
            """
type X =
    struct
        val mutable x: int
        new(x) = { x = x }
    end

type Y =
    struct
        val mutable x: int
        new(x) = { x = x }
    end
"""

    [<Fact>]
    let ``Produces a struct definition``() =
        Oak() {
            AnonymousModule() {
                TypeDefn("X") {
                    ValField("x", Int()).toMutable()
                    Constructor(ParenPat("x"), RecordExpr([ RecordFieldExpr("x", "x") ]))
                }
                |> _.attribute(Attribute("Struct"))
            }
        }
        |> produces
            """

[<Struct>]
type X =
    val mutable x: int
    new(x) = { x = x }
"""
