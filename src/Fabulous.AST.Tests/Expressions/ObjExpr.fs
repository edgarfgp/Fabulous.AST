namespace Fabulous.AST.Tests.Expressions

open Xunit
open Fabulous.AST.Tests

open Fabulous.AST

open type Ast

module ObjExpr =

    [<Fact>]
    let ``let value with a ObjExpr expression when typename is a class``() =
        Oak() {
            AnonymousModule() {
                ObjExpr(LongIdent("System.Object"), ConstantExpr(ConstantUnit())) {
                    Member("x.ToString", UnitPat(), ConstantExpr(String("F#")))
                }
            }
        }
        |> producesValid
            """
{ new System.Object() with
    member x.ToString() = "F#" }
"""

    [<Fact>]
    let ``let value with a ObjExpr expression when typename is not a class``() =
        Oak() {
            AnonymousModule() {
                ObjExpr(LongIdent("System.IFormattable")) {
                    Member(
                        "x.ToString",
                        ParenPat(
                            TuplePat(
                                [ ParameterPat(ConstantPat(Constant("format")), String())
                                  ParameterPat(ConstantPat(Constant("provider")), LongIdent("System.IFormatProvider")) ]
                            )
                        ),
                        IfThenElseExpr(
                            InfixAppExpr(ConstantExpr(Constant("format")), "=", ConstantExpr(String("D"))),
                            SameInfixAppsExpr(
                                ConstantExpr(Constant("delim1")),
                                [ ("+", ConstantExpr(Constant("value")))
                                  ("+", ConstantExpr(Constant("delim2"))) ]
                            ),
                            ConstantExpr(Constant "value")
                        )
                    )
                }

                ObjExpr(LongIdent("System.IFormattable")) {
                    Member(
                        "x.ToString",
                        ParenPat(
                            TuplePat(
                                [ ParameterPat(ConstantPat(Constant("format")), String())
                                  ParameterPat(ConstantPat(Constant("provider")), LongIdent("System.IFormatProvider")) ]
                            )
                        ),
                        IfThenElseExpr(
                            InfixAppExpr(ConstantExpr(Constant("format")), "=", ConstantExpr(String("D"))),
                            SameInfixAppsExpr("delim1", [ ("+", "value"); ("+", "delim2") ]),
                            "value"
                        )
                    )
                }
            }
        }
        |> producesValid
            """
{ new System.IFormattable with
    member x.ToString(format: string, provider: System.IFormatProvider) =
        if format = "D" then delim1 + value + delim2 else value }

{ new System.IFormattable with
    member x.ToString(format: string, provider: System.IFormatProvider) =
        if format = "D" then delim1 + value + delim2 else value }
"""

    [<Fact>]
    let ``ObjExpr implementing several interfaces``() =
        Oak() {
            AnonymousModule() {
                Value(
                    "x",
                    ObjExpr(LongIdent("System.IDisposable")) {
                        Member("_.Dispose", UnitPat(), ConstantExpr(ConstantUnit()))

                        InterfaceWith("System.IFormattable") {
                            Member(
                                "_.ToString(format: string, provider: System.IFormatProvider)",
                                ConstantExpr(String("x"))
                            )
                        }
                    }
                )
            }
        }
        |> producesValid
            """
let x =
    { new System.IDisposable with
        member _.Dispose() = ()
      interface System.IFormattable with
          member _.ToString(format: string, provider: System.IFormatProvider) = "x" }
"""

    [<Fact>]
    let ``ObjExpr members modifier``() =
        Oak() {
            AnonymousModule() {
                Value(
                    "x",
                    (ObjExpr(LongIdent("System.IDisposable")) {
                        Member("_.Dispose", UnitPat(), ConstantExpr(ConstantUnit()))
                    })
                        .members() {
                        InterfaceWith("System.IFormattable") {
                            Member(
                                "_.ToString(format: string, provider: System.IFormatProvider)",
                                ConstantExpr(String("x"))
                            )
                        }
                    }
                )
            }
        }
        |> producesValid
            """
let x =
    { new System.IDisposable with
        member _.Dispose() = ()
      interface System.IFormattable with
          member _.ToString(format: string, provider: System.IFormatProvider) = "x" }
"""
