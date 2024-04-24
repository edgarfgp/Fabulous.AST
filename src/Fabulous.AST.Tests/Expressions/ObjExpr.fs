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
                ObjExpr(LongIdent("System.Object"), ConstantExpr(ConstantUnit()))
                    .bindings([ Method("x.ToString", [], ConstantExpr(DoubleQuoted("F#"))) ])
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
                ObjExpr(LongIdent("System.IFormattable"))
                    .bindings(
                        [ Method(
                              "x.ToString",
                              ParenPat(
                                  TuplePat(
                                      [ ParameterPat("format", String())
                                        ParameterPat("provider", LongIdent("System.IFormatProvider")) ]
                                  )
                              ),
                              IfThenElseExpr(
                                  InfixAppExpr(
                                      ConstantExpr(Constant(Unquoted "format")),
                                      "=",
                                      ConstantExpr(Constant(DoubleQuoted "D"))
                                  ),
                                  SameInfixAppsExpr(
                                      ConstantExpr(Unquoted("delim1")),
                                      [ ("+", ConstantExpr(Unquoted("value")))
                                        ("+", ConstantExpr(Unquoted("delim2"))) ]
                                  ),
                                  ConstantExpr(Unquoted "value")
                              )
                          ) ]
                    )
            }
        }
        |> produces
            """
{ new System.IFormattable with
    member x.ToString(format: string, provider: System.IFormatProvider) =
        if format = "D" then delim1 + value + delim2 else value }
"""
