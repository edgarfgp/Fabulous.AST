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
                    InheritType(LongIdent "BaseClass"),
                    [ RecordFieldExpr("B", ConstantExpr(Int 1))
                      RecordFieldExpr("C", ConstantExpr(Constant "2")) ]
                )

                InheritRecordExpr(
                    InheritParen(LongIdent "BaseClass", ParenExpr(ConstantExpr(Constant "1"))),
                    [ RecordFieldExpr("B", ConstantExpr(Constant "1"))
                      RecordFieldExpr("C", ConstantExpr(Constant "2")) ]
                )

                InheritRecordExpr(
                    InheritUnit(LongIdent "BaseClass"),
                    [ RecordFieldExpr("B", ConstantExpr(Constant "1"))
                      RecordFieldExpr("C", ConstantExpr(Constant "2")) ]
                )

                InheritRecordExpr(
                    InheritOther(LongIdent "BaseClass", ParenExpr(ConstantExpr(Constant "1"))),
                    [ RecordFieldExpr("B", ConstantExpr(Constant "1"))
                      RecordFieldExpr("C", ConstantExpr(Constant "2")) ]
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
