namespace Fabulous.AST.Tests.MethodDefinitions

open Fabulous.AST
open Fabulous.AST.Tests
open Xunit

open type Ast

module ExplicitConstructor =

    [<Fact>]
    let ``Produces a classes ExplicitCtor``() =
        Oak() {
            AnonymousModule() {
                TypeDefn("Person", ParenPat()) {
                    ExplicitConstructor(
                        ParenPat(
                            TuplePat(
                                [ (ConstantPat(Constant "x"))
                                  (ConstantPat(Constant "y"))
                                  (ConstantPat(Constant "z")) ]
                            )
                        ),
                        RecordExpr(
                            [ RecordFieldExpr("X", ConstantExpr(Constant "x"))
                              RecordFieldExpr("Y", ConstantExpr(Constant "y"))
                              RecordFieldExpr("Z", ConstantExpr(Constant "z")) ]
                        ),
                        "this"
                    )

                    ExplicitConstructor(
                        ParenPat(TuplePat([ Constant "x"; Constant "y"; Constant "z" ])),
                        RecordExpr(
                            [ RecordFieldExpr("X", ConstantExpr(Constant "x"))
                              RecordFieldExpr("Y", ConstantExpr(Constant "y"))
                              RecordFieldExpr("Z", ConstantExpr(Constant "z")) ]
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
