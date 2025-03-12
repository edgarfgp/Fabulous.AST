namespace Fabulous.AST.Tests.Expressions

open Xunit
open Fabulous.AST.Tests

open Fabulous.AST

open type Ast

module InterpolatedString =
    [<Fact>]
    let ``Basic interpolation expressions``() =
        Oak() {
            AnonymousModule() {
                // Basic interpolation
                // $"{12}"
                InterpolatedStringExpr(ConstantExpr("12"))
                InterpolatedStringExpr(Int(12))
                InterpolatedStringExpr("12")

                // Multiple expressions
                // $"{12}{12}{12}"
                InterpolatedStringExpr([ ConstantExpr("12"); ConstantExpr("12"); ConstantExpr("12") ])
                InterpolatedStringExpr([ Int(12); Int(12); Int(12) ])
                InterpolatedStringExpr([ "12"; "12"; "12" ])
            }
        }
        |> produces
            """
$"{12}"
$"{12}"
$"{12}"
$"{12}{12}{12}"
$"{12}{12}{12}"
$"{12}{12}{12}"
"""

    [<Fact>]
    let ``Raw string interpolation expressions``() =
        let source =
            Oak() {
                AnonymousModule() {
                    // Raw string interpolation
                    // $"""{12}"""
                    InterpolatedRawStringExpr(ConstantExpr("12"))
                    InterpolatedRawStringExpr(Int(12))
                    InterpolatedRawStringExpr("12")

                    // Raw string with multiple expressions
                    // $"""{12}{12}{12}"""
                    InterpolatedRawStringExpr([ ConstantExpr("12"); ConstantExpr("12"); ConstantExpr("12") ])
                    InterpolatedRawStringExpr([ Int(12); Int(12); Int(12) ])
                    InterpolatedRawStringExpr([ "12"; "12"; "12" ])

                    // Raw string with text and expressions
                    // $"""This is a test: {12} This is a test: {12}"""
                    InterpolatedRawStringExpr(
                        [ "This is a test: "; " This is a test: " ],
                        [ ConstantExpr("12"); ConstantExpr("12") ]
                    )

                    InterpolatedRawStringExpr([ "This is a test: "; " This is a test: " ], [ Int(12); Int(12) ])
                    InterpolatedRawStringExpr([ "This is a test: "; " This is a test: " ], [ "12"; "12" ])
                }
            }

        let res = Gen.mkOak source |> Gen.run
        (*
$"""{12}"""
$"""{12}"""
$"""{12}"""
$"""{12}{12}{12}"""
$"""{12}{12}{12}"""
$"""{12}{12}{12}"""
$"""This is a test: {12} This is a test: {12}"""
$"""This is a test: {12} This is a test: {12}"""
$"""This is a test: {12} This is a test: {12}"""
*)

        Assert.NotNull(res)

    [<Fact>]
    let ``Multiple dollar sign expressions``() =
        let source =
            Oak() {
                AnonymousModule() {
                    // Multiple dollar signs
                    // $$"""{12}"""
                    InterpolatedRawStringExpr("$$", ConstantExpr("12"))
                    InterpolatedRawStringExpr("$$", ConstantExpr("12"), numberOfBraces = 2)
                    InterpolatedRawStringExpr("$$", Int(12))
                    InterpolatedRawStringExpr("$$", "12")

                    // Multiple dollar signs with multiple expressions
                    // $$"""{12}{12}{12}"""
                    InterpolatedRawStringExpr("$$", [ ConstantExpr("12"); ConstantExpr("12"); ConstantExpr("12") ])
                    InterpolatedRawStringExpr("$$", [ Int(12); Int(12); Int(12) ])
                    InterpolatedRawStringExpr("$$", [ "12"; "12"; "12" ])

                    // Multiple dollar signs with text and expressions
                    // $$"""This is a test: {12} This is a test: {12}"""
                    InterpolatedRawStringExpr(
                        "$$",
                        [ "This is a test: "; " This is a test: " ],
                        [ ConstantExpr("12"); ConstantExpr("12") ]
                    )

                    InterpolatedRawStringExpr("$$", [ "This is a test: "; " This is a test: " ], [ Int(12); Int(12) ])
                    InterpolatedRawStringExpr("$$", [ "This is a test: "; " This is a test: " ], [ "12"; "12" ])
                }
            }
        (*
$$"""{12}"""
$$"""{12}"""
$$"""{12}"""
$$"""{12}{12}{12}"""
$$"""{12}{12}{12}"""
$$"""{12}{12}{12}"""
$$"""This is a test: {12} This is a test: {12}"""
$$"""This is a test: {12} This is a test: {12}"""
$$"""This is a test: {12} This is a test: {12}"""
*)

        let res = Gen.mkOak source |> Gen.run

        Assert.NotNull(res)

    [<Fact>]
    let ``Text with expression interpolation``() =
        Oak() {
            AnonymousModule() {
                // Text with expression
                // $"This is a test: {12}"
                InterpolatedStringExpr("This is a test: ", ConstantExpr("12"))

                // Simple string with expression
                // $"This is a test: {12}"
                InterpolatedStringExpr("This is a test: ", "12")

                // Text with multiple expressions
                // $"This is a test: {12} This is a test: {12}"
                InterpolatedStringExpr(
                    [ "This is a test: "; " This is a test: " ],
                    [ ConstantExpr("12"); ConstantExpr("12") ]
                )

                InterpolatedStringExpr([ "This is a test: "; " This is a test: " ], [ Int(12); Int(12) ])
                InterpolatedStringExpr([ "This is a test: "; " This is a test: " ], [ "12"; "12" ])
            }
        }
        |> produces
            """
$"This is a test: {12}"
$"This is a test: {12}"
$"This is a test: {12} This is a test: {12}"
$"This is a test: {12} This is a test: {12}"
$"This is a test: {12} This is a test: {12}"
"""
