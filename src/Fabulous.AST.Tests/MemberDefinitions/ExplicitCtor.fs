namespace Fabulous.AST.Tests.MethodDefinitions

open Fabulous.AST
open Fabulous.AST.Tests
open Xunit

open type Ast

module ExplicitCtor =

    [<Fact>]
    let ``Produces a classes ExplicitCtor``() =
        Oak() {
            AnonymousModule() {
                Class("Person") {
                    ExplicitCtor(
                        ParenPat(TuplePat([ ParameterPat("x"); ParameterPat("y"); ParameterPat("z") ])),
                        RecordExpr(
                            [ RecordFieldExpr("X", ConstantExpr(Unquoted "x"))
                              RecordFieldExpr("Y", ConstantExpr(Unquoted "y"))
                              RecordFieldExpr("Z", ConstantExpr(Unquoted "z")) ]
                        ),
                        "this"
                    )

                    ExplicitCtor(
                        ParenPat(TuplePat([ ParameterPat("x"); ParameterPat("y"); ParameterPat("z") ])),
                        RecordExpr(
                            [ RecordFieldExpr("X", ConstantExpr(Unquoted "x"))
                              RecordFieldExpr("Y", ConstantExpr(Unquoted "y"))
                              RecordFieldExpr("Z", ConstantExpr(Unquoted "z")) ]
                        )
                    )
                }
            }
        }
        |> produces
            """
type Person() =
    new(x, y, z) as this = { X = x; Y = y; Z = z }
    new(x, y, z) = { X = x; Y = y; Z = z }
"""
