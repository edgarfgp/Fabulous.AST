namespace Fabulous.AST.Tests.Specialized

open Fabulous.AST
open Fabulous.AST.Tests
open Xunit

open type Ast

/// Targeted tests for F# 8/9/10 language features. These cover scenarios that the
/// per-widget tests don't exercise but that real consumers expect to generate.
module FSharpFeatures =

    [<Fact>]
    let ``Static abstract member on interface (IWSAM-style)``() =
        Oak() { AnonymousModule() { TypeDefn("IShow") { AbstractMember("Show", [ Unit() ], String()).toStatic() } } }
        |> producesValid
            """
type IShow =
    static abstract Show: unit -> string
"""

    [<Fact>]
    let ``Static abstract property on interface``() =
        Oak() { AnonymousModule() { TypeDefn("IDefault") { AbstractMember("Default", Int()).toStatic() } } }
        |> producesValid
            """
type IDefault =
    static abstract Default: int
"""

    [<Fact>]
    let ``Anonymous struct record literal``() =
        Oak() {
            AnonymousModule() {
                Value(
                    "p",
                    AnonStructRecordExpr(
                        [ RecordFieldExpr("Name", ConstantExpr(String "Edgar"))
                          RecordFieldExpr("Age", ConstantExpr(Int 42)) ]
                    )
                )
            }
        }
        |> producesValid
            """
let p = struct {| Name = "Edgar"; Age = 42 |}
"""

    [<Fact>]
    let ``Units of measure type definitions``() =
        Oak() {
            AnonymousModule() {
                Measure("m")
                Measure("s")
                Measure("kg")
            }
        }
        |> producesValid
            """
[<Measure>]
type m

[<Measure>]
type s

[<Measure>]
type kg
"""

    [<Fact>]
    let ``Lambda with tuple-destructured parameter``() =
        Oak() {
            AnonymousModule() {
                Value(
                    "addPair",
                    LambdaExpr(
                        [ ParenPat(TuplePat([ NamedPat("a"); NamedPat("b") ])) ],
                        InfixAppExpr(ConstantExpr(Constant "a"), "+", ConstantExpr(Constant "b"))
                    )
                )
            }
        }
        |> producesValid
            """
let addPair = fun (a, b) -> a + b
"""

    [<Fact>]
    let ``Try / with single-clause expression``() =
        Oak() {
            AnonymousModule() {
                Value(
                    "result",
                    TryWithSingleClauseExpr(
                        AppExpr(ConstantExpr(Constant "System.Int32.Parse"), [ ConstantExpr(String "42") ]),
                        MatchClauseExpr(NamedPat("ex"), ConstantExpr(Int 0))
                    )
                )
            }
        }
        |> producesValid
            """
let result =
    try
        System.Int32.Parse "42"
    with ex ->
        0
"""

    [<Fact>]
    let ``Type extension via Augmentation (type X with member ...)``() =
        Oak() {
            AnonymousModule() {
                Augmentation("List") {
                    Member(
                        "this.Second",
                        AppExpr(ConstantExpr(Constant "List.head"), [ ConstantExpr(Constant "this.Tail") ])
                    )

                    Member(
                        "this.IsEmpty2",
                        InfixAppExpr(ConstantExpr(Constant "List.length this"), "=", ConstantExpr(Int 0))
                    )
                }
            }
        }
        |> producesValid
            """
type List with
    member this.Second = List.head this.Tail
    member this.IsEmpty2 = List.length this = 0
"""

    [<Fact>]
    let ``For-each loop over a list``() =
        Oak() {
            AnonymousModule() {
                Value(
                    "go",
                    ForEachDoExpr(
                        NamedPat("x"),
                        ArrayExpr([ ConstantExpr(Int 1); ConstantExpr(Int 2); ConstantExpr(Int 3) ]),
                        AppExpr(
                            ConstantExpr(Constant "printfn"),
                            [ ConstantExpr(String "%d"); ConstantExpr(Constant "x") ]
                        )
                    )
                )
            }
        }
        |> producesValid
            """
let go =
    for x in [| 1; 2; 3 |] do
        printfn "%d" x
"""
