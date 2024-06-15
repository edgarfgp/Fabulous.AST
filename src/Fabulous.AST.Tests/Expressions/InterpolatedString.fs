namespace Fabulous.AST.Tests.Expressions

open Xunit
open Fabulous.AST.Tests

open Fabulous.AST

open type Ast

module InterpolatedString =
    [<Fact>]
    let ``InterpolatedString expression``() =
        Oak() {
            AnonymousModule() {
                InterpolatedStringExpr(ConstantExpr("12"))
                InterpolatedStringExpr(Int(12))
                InterpolatedStringExpr("12")

                InterpolatedStringExpr("This is a test: ", ConstantExpr("12"))
                InterpolatedStringExpr("This is a test: ", Int(12))
                InterpolatedStringExpr("This is a test: ", "12")

                InterpolatedStringExpr("$", "This is another test: ", ConstantExpr("12"))
                InterpolatedStringExpr("$", "This is another test: ", Int(12))
                InterpolatedStringExpr("$", "This is another test: ", "12")

                InterpolatedStringExpr("$$", "This is another test: ", ConstantExpr("{12}"))
                InterpolatedStringExpr("$$", "This is another test: ", Int(12))
                InterpolatedStringExpr("$$", "This is another test: ", "{12}")
            }
        }
        |> produces
            """
$"{12}"
$"{12}"
$"{12}"
$"This is a test: {12}"
$"This is a test: {12}"
$"This is a test: {12}"
$"This is another test: {12}"
$"This is another test: {12}"
$"This is another test: {12}"
$$"This is another test: {{12}}"
$$"This is another test: {12}"
$$"This is another test: {{12}}"
"""
