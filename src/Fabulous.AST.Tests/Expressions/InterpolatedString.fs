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
                // $"{12}"
                InterpolatedStringExpr(ConstantExpr("12"))
                InterpolatedStringExpr(Int(12))
                InterpolatedStringExpr("12")

                //$"{12}{12}{12}"
                InterpolatedStringExpr([ ConstantExpr("12"); ConstantExpr("12"); ConstantExpr("12") ])
                InterpolatedStringExpr([ Int(12); Int(12); Int(12) ])
                InterpolatedStringExpr([ "12"; "12"; "12" ])

                // $"""{12}"""
                //InterpolatedRawStringExpr(ConstantExpr("12"))
                //InterpolatedRawStringExpr(Int(12))
                //InterpolatedRawStringExpr("12")

                // $"""{12}{12}{12}"""
                //InterpolatedRawStringExpr([ConstantExpr("12"); ConstantExpr("12"); ConstantExpr("12")])
                //InterpolatedRawStringExpr([Int(12); Int(12); Int(12)])
                //InterpolatedRawStringExpr(["12"; "12"; "12"])

                // $$"""{12}"""
                //InterpolatedRawStringExpr("$$", ConstantExpr("12"))
                //InterpolatedRawStringExpr("$$", Int(12))
                //InterpolatedRawStringExpr("$$", "12")

                // $$"""{12}{12}{12}"""
                //InterpolatedRawStringExpr("$$", [ConstantExpr("12"); ConstantExpr("12"); ConstantExpr("12")])
                //InterpolatedRawStringExpr("$$", [Int(12); Int(12); Int(12)])
                //InterpolatedRawStringExpr("$$", ["12"; "12"; "12"])

                // $"This is a test: {12}"
                InterpolatedStringExpr("This is a test: ", ConstantExpr("12"))
                // $"This is a test format-specifier %d{12}"
                InterpolatedStringExpr("This is a test format-specifier %d", Int(12))

                // $"This is a test: {12}"
                InterpolatedStringExpr("This is a test: ", "12")

                // $"This is a test: {12} This is a test: {12}"
                InterpolatedStringExpr(
                    [ "This is a test: "; " This is a test: " ],
                    [ ConstantExpr("12"); ConstantExpr("12") ]
                )

                InterpolatedStringExpr([ "This is a test: "; " This is a test: " ], [ Int(12); Int(12) ])
                InterpolatedStringExpr([ "This is a test: "; " This is a test: " ], [ "12"; "12" ])

            // $"""This is a test: {12} This is a test: {12}"""
            //InterpolatedRawStringExpr(["This is a test: "; " This is a test: " ], [ ConstantExpr("12"); ConstantExpr("12")])
            //InterpolatedRawStringExpr(["This is a test: "; " This is a test: " ], [ Int(12); Int(12) ])
            //InterpolatedRawStringExpr(["This is a test: "; " This is a test: " ], [ "12"; "12" ])

            // $$"""This is a test: {12} This is a test: {12}"""
            //InterpolatedRawStringExpr("$$", ["This is a test: "; " This is a test: " ], [ ConstantExpr("12"); ConstantExpr("12")])
            //InterpolatedRawStringExpr("$$", ["This is a test: "; " This is a test: " ], [ Int(12); Int(12) ])
            //InterpolatedRawStringExpr("$$", ["This is a test: "; " This is a test: " ], [ "12"; "12" ])
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
$"This is a test: {12}"
$"This is a test format-specifier %d{12}"
$"This is a test: {12}"
$"This is a test: {12} This is a test: {12}"
$"This is a test: {12} This is a test: {12}"
$"This is a test: {12} This is a test: {12}"
"""
