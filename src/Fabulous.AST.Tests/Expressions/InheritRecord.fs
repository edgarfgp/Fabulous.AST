namespace Fabulous.AST.Tests.Expressions

open Xunit
open Fabulous.AST.Tests

open Fabulous.AST

open type Ast

module InheritRecord =

    [<Fact>]
    let ``let value with a InheritRecordExpr expression``() =
        Oak() {
            AnonymousModule() {
                InheritRecordExpr(
                    InheritConstructorTypeOnly(Unquoted "BaseClass"),
                    [ RecordFieldExpr("B", ConstantExpr(Unquoted "1"))
                      RecordFieldExpr("C", ConstantExpr(Unquoted "2")) ]
                )

                InheritRecordExpr(
                    InheritConstructorParen(Unquoted "BaseClass", ParenExpr(ConstantExpr(Unquoted "1"))),
                    [ RecordFieldExpr("B", ConstantExpr(Unquoted "1"))
                      RecordFieldExpr("C", ConstantExpr(Unquoted "2")) ]
                )

                InheritRecordExpr(
                    InheritConstructorUnit(Unquoted "BaseClass"),
                    [ RecordFieldExpr("B", ConstantExpr(Unquoted "1"))
                      RecordFieldExpr("C", ConstantExpr(Unquoted "2")) ]
                )

                InheritRecordExpr(
                    InheritConstructorOther(Unquoted "BaseClass", ParenExpr(ConstantExpr(Unquoted "1"))),
                    [ RecordFieldExpr("B", ConstantExpr(Unquoted "1"))
                      RecordFieldExpr("C", ConstantExpr(Unquoted "2")) ]
                )
            }
        }
        |> produces
            """
{ inherit BaseClass; B = 1; C = 2 }
{ inherit BaseClass(1); B = 1; C = 2 }
{ inherit BaseClass(); B = 1; C = 2 }
{ inherit BaseClass (1); B = 1; C = 2 }
"""
