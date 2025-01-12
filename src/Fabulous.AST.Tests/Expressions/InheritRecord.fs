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
                InheritRecordExpr(InheritType("Foo()"))
                InheritRecordExpr(InheritUnit(LongIdent "Foo"))
                InheritRecordExpr(InheritParen(LongIdent("Foo "), ConstantExpr(String("123"))))

                InheritRecordExpr(
                    InheritParen(LongIdent "BaseClass", ConstantExpr(Constant "1")),
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
{ inherit Foo() }
{ inherit Foo() }
{ inherit Foo ("123") }
{ inherit BaseClass(1); B = 1; C = 2 }
{ inherit BaseClass(); B = 1; C = 2 }
{ inherit BaseClass (1); B = 1; C = 2 }
"""
