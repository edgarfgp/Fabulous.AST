namespace Fabulous.AST.Tests.TypeDefinitions

open Fantomas.FCS.Text
open Fabulous.AST.Tests
open Fantomas.Core.SyntaxOak
open Xunit

open Fabulous.AST
open type Ast

module Struct =
    [<Fact>]
    let ``Produces a verbose struct``() =
        let expr = Expr.Constant(Constant.FromText(SingleTextNode("name", Range.Zero)))

        Oak() {
            AnonymousModule() {
                StructEnd("X").members() {
                    ValField("x", Int()).toMutable()

                    ExplicitConstructor(ParenPat("x"), RecordExpr([ RecordFieldExpr("x", "x") ]))
                }

                StructEnd("X", ImplicitConstructor(ParenPat())).members() {
                    ValField("x", Int()).toMutable()

                    ExplicitConstructor(ParenPat("x"), RecordExpr([ RecordFieldExpr("x", "x") ]))
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

type X() =
    struct
        val mutable x: int
        new(x) = { x = x }
    end
"""

    [<Fact>]
    let ``Produces a struct definition``() =
        let expr = Expr.Constant(Constant.FromText(SingleTextNode("name", Range.Zero)))

        Oak() {
            AnonymousModule() {
                TypeDefn("X") {
                    ValField("x", Int()).toMutable()
                    ExplicitConstructor(ParenPat("x"), RecordExpr([ RecordFieldExpr("x", "x") ]))
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
