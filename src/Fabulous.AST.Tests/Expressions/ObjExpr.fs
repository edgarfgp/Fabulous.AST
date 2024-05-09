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
                    Method("x.ToString", [], ConstantExpr(String("F#")))
                }
            }
        }
        |> produces
            """
{ new System.Object() with
    member x.ToString = "F#" }
"""

    [<Fact>]
    let ``let value with a ObjExpr expression when typename is not a class``() =
        Oak() {
            AnonymousModule() {
                ObjExpr(LongIdent("System.IFormattable")) {
                            Method(
                              "x.ToString",
                              ParenPat(
                                  TuplePat(
                                      [ ParameterPat(ConstantPat(Constant("format")), String())
                                        ParameterPat(
                                            ConstantPat(Constant("provider")),
                                            LongIdent("System.IFormatProvider")
                                        ) ]
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
            }
        }
        |> produces
            """
{ new System.IFormattable with
    member x.ToString(format: string, provider: System.IFormatProvider) =
        if format = "D" then delim1 + value + delim2 else value }
"""
