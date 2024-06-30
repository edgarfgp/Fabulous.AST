namespace Fabulous.AST.Tests.Expressions

open Xunit
open Fabulous.AST.Tests

open Fabulous.AST

open type Ast

module New =

    [<Fact>]
    let ``let value with a New expression``() =
        Oak() {
            AnonymousModule() {
                Value(ConstantPat(Constant("x")), NewExpr(LongIdent("MyType"), ConstantExpr(Int(12))))
                Value(ConstantPat(Constant("x")), NewExpr("MyType", ConstantExpr(Int(12))))
                Value(ConstantPat(Constant("x")), NewExpr(LongIdent("MyType"), Int(12)))
                Value(ConstantPat(Constant("x")), NewExpr("MyType", Int(12)))
                Value(ConstantPat(Constant("x")), NewExpr("MyType", "12"))
            }
        }
        |> produces
            """

let x = new MyType 12
let x = new MyType 12
let x = new MyType 12
let x = new MyType 12
let x = new MyType 12
"""

    [<Fact>]
    let ``let value with a New expression with parenthesis``() =
        Oak() {
            AnonymousModule() {
                Value(ConstantPat(Constant("x")), NewExpr(LongIdent "MyType", ParenExpr(ConstantExpr(Int(12)))))
            }
        }
        |> produces
            """

let x = new MyType(12)
"""
