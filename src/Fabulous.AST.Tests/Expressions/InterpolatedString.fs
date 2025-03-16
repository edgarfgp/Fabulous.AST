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
                // $"{12}"
                InterpolatedStringExpr([ Expr(FillExpr(Int(12)), 1) ])
                InterpolatedStringExpr([ ConstantExpr(Int(12)) ])
                InterpolatedStringExpr([ Int(12) ])
                InterpolatedStringExpr([ "12" ])

                // $"{12}"
                InterpolatedStringExpr([ ConstantExpr("12") ])
                InterpolatedStringExpr([ Int(12) ])
                InterpolatedStringExpr([ "12" ])
                InterpolatedStringExpr([ Text("{12}") ])
                InterpolatedStringExpr([ Text("{12}"); Text("{12}"); Text("{12}") ])

                // $"{12}{12}{12}"
                InterpolatedStringExpr(
                    [ Expr(FillExpr(Int(12)), 1)
                      Expr(FillExpr(Int(12)), 1)
                      Expr(FillExpr(Int(12)), 1) ]
                )

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
$"{12}"
$"{12}"
$"{12}"
$"{12}"
$"{12}"
$"{12}{12}{12}"
$"{12}{12}{12}"
$"{12}{12}{12}"
$"{12}{12}{12}"
$"{12}{12}{12}"
"""

    [<Fact>]
    let ``Basic interpolation expressions with number of braces``() =
        Oak() {
            AnonymousModule() {
                // $"{12}"
                InterpolatedStringExpr([ Expr(FillExpr(Int(12)), 2) ])
                InterpolatedStringExpr([ Expr(FillExpr(Int(12)), 3) ])
                InterpolatedStringExpr([ Expr(FillExpr(Int(12)), 4) ])
                InterpolatedStringExpr([ Expr(FillExpr("12"), 5) ])
            }
        }
        |> produces
            """
$"{{12}}"
$"{{{12}}}"
$"{{{{12}}}}"
$"{{{{{12}}}}}"
"""

    [<Fact>]
    let ``Verbatim interpolation Aligning expressions in interpolated strings``() =
        let source =
            Oak() {
                AnonymousModule() {
                    // $"""|{"Left",-7}|{"Right",7}|"""
                    InterpolatedStringExpr(
                        [ Text("|")
                          Expr(FillExpr(TupleExpr([ String("Left"); Constant("-7") ])), 1)
                          Text("|")
                          Expr(FillExpr(TupleExpr([ String("Right"); Constant("7") ])), 1)
                          Text("|") ],
                        isVerbatim = true
                    )
                }
            }

        (*
$"""|{"Left",-7}|{"Right",7}|"""
*)

        let res = Gen.mkOak source |> Gen.run

        Assert.NotNull(res)

    [<Fact>]
    let ``Interpolated strings and FormattableString formatting``() =
        let source =
            Oak() {
                AnonymousModule() {
                    // $"The speed of light is {speedOfLight:N3} km/s."
                    InterpolatedStringExpr(
                        [ Text("The speed of light is ")
                          Expr(FillExpr("speedOfLight", "N3"), 1)
                          Text(" km/s.") ]
                    )
                }
            }

        (*
$"The speed of light is {speedOfLight:N3} km/s."
*)

        let res = Gen.mkOak source |> Gen.run

        Assert.NotNull(res)

    [<Fact>]
    let ``Basic interpolation expressions with format specifiers``() =
        Oak() {
            AnonymousModule() {
                // $"%0.3f{System.Math.PI}"
                InterpolatedStringExpr([ Text("%0.3f"); Expr(FillExpr("System.Math.PI"), 1) ])

                // $"0x%08x{43962}"
                InterpolatedStringExpr([ Text("0x%08x"); Expr(FillExpr(Int(43962)), 1) ])

                // $"The data is %A{data}"
                InterpolatedStringExpr([ Text("The data is %A"); Expr(FillExpr("data"), 1) ])

                // $"{System.Math.PI:N4}"
                InterpolatedStringExpr([ Expr(FillExpr("System.Math.PI", "N4"), 1) ])

                // $"{System.DateTime.UtcNow:yyyyMMdd}"
                InterpolatedStringExpr([ Expr(FillExpr("System.DateTime.UtcNow", "``yyyyMMdd``"), 1) ])

                // $"{System.DateTime.UtcNow:``yyyy-MM-dd``}"
                InterpolatedStringExpr([ Expr(FillExpr("System.DateTime.UtcNow", "``yyyy-MM-dd``"), 1) ])

            }
        }
        |> produces
            """
$"%0.3f{System.Math.PI}"
$"0x%08x{43962}"
$"The data is %A{data}"
$"{System.Math.PI:N4}"
$"{System.DateTime.UtcNow:``yyyyMMdd``}"
$"{System.DateTime.UtcNow:``yyyy-MM-dd``}"
"""

    [<Fact>]
    let ``Text with expression interpolation``() =
        Oak() {
            AnonymousModule() {

                // $"This is a test: {12}"
                InterpolatedStringExpr([ Text("This is a test: "); Expr(FillExpr(Int(12)), 1) ])

                // $"This is a test: {12}"
                InterpolatedStringExpr([ Text("This is a test: "); Text("{12}") ])

                // $"This is a test: {12} This is a test: {12}"
                InterpolatedStringExpr(
                    [ Text("This is a test: ")
                      Expr(FillExpr(Int(12)), 1)
                      Text(" This is a test: ")
                      Expr(FillExpr(Int(12)), 1) ]
                )
                // $"This is a test: {12} This is a test: {12}"
                InterpolatedStringExpr(
                    [ Text("This is a test: ")
                      Expr(FillExpr(Int(12)), 1)
                      Text(" This is a test: ")
                      Expr(FillExpr(Int(12)), 1) ]
                )

                // $"This is a test: {12} This is a test: {12}"
                InterpolatedStringExpr(
                    [ Text("This is a test: ")
                      Text("{12}")
                      Text(" This is a test: ")
                      Text("{12}") ]
                )
                // $"'{a}' is not a valid number"
                InterpolatedStringExpr([ Text("'"); Expr(FillExpr("a"), 1); Text("'"); Text(" is not a valid number") ])

                InterpolatedStringExpr([ Text("'"); Text("{a}"); Text("'"); Text(" is not a valid number") ])

                InterpolatedStringExpr([ Text("'{a}'"); Text(" is not a valid number") ])
            }
        }
        |> produces
            """
$"This is a test: {12}"
$"This is a test: {12}"
$"This is a test: {12} This is a test: {12}"
$"This is a test: {12} This is a test: {12}"
$"This is a test: {12} This is a test: {12}"
$"'{a}' is not a valid number"
$"'{a}' is not a valid number"
$"'{a}' is not a valid number"
"""

    [<Fact>]
    let ``Text with expression interpolation with number of braces``() =
        Oak() {
            AnonymousModule() {

                // $"This is a test: {{12}}"
                InterpolatedStringExpr([ Text("This is a test: "); Expr(FillExpr(Int(12)), 2) ])

                // $"This is a test: {{{12}}}"
                InterpolatedStringExpr([ Text("This is a test: "); Text("{12}") ])

                // $"This is a test: {12} This is a test: {12}"
                InterpolatedStringExpr(
                    [ Text("This is a test: ")
                      Expr(FillExpr(Int(12)), 4)
                      Text(" This is a test: ")
                      Expr(FillExpr(Int(12)), 4) ]
                )
                // $"This is a test: {12} This is a test: {12}"
                InterpolatedStringExpr(
                    [ Text("This is a test: ")
                      Expr(FillExpr(Int(12)), 5)
                      Text(" This is a test: ")
                      Expr(FillExpr(Int(12)), 5) ]
                )

                // $"This is a test: {12} This is a test: {12}"
                InterpolatedStringExpr(
                    [ Text("This is a test: ")
                      Text("{12}")
                      Text(" This is a test: ")
                      Text("{12}") ]
                )
                // $"'{a}' is not a valid number"
                InterpolatedStringExpr([ Text("'"); Expr(FillExpr("a"), 2); Text("'"); Text(" is not a valid number") ])

                InterpolatedStringExpr([ Text("'"); Text("{a}"); Text("'"); Text(" is not a valid number") ])

                InterpolatedStringExpr([ Text("'{a}'"); Text(" is not a valid number") ])
            }
        }
        |> produces
            """
$"This is a test: {{12}}"
$"This is a test: {12}"
$"This is a test: {{{{12}}}} This is a test: {{{{12}}}}"
$"This is a test: {{{{{12}}}}} This is a test: {{{{{12}}}}}"
$"This is a test: {12} This is a test: {12}"
$"'{{a}}' is not a valid number"
$"'{a}' is not a valid number"
$"'{a}' is not a valid number"
"""

    [<Fact>]
    let ``Verbatim string interpolation expressions``() =
        let source =
            Oak() {
                AnonymousModule() {
                    // $"""{12}"""
                    InterpolatedStringExpr([ Expr(FillExpr(Int(12)), 1) ], isVerbatim = true)
                    InterpolatedStringExpr([ ConstantExpr(Int(12)) ], isVerbatim = true)
                    InterpolatedStringExpr([ Int(12) ], isVerbatim = true)
                    InterpolatedStringExpr([ "12" ], isVerbatim = true)

                    // $"""{12}{12}{12}"""
                    InterpolatedStringExpr(
                        [ Expr(FillExpr(Int(12)), 1)
                          Expr(FillExpr(Int(12)), 1)
                          Expr(FillExpr(Int(12)), 1) ],
                        isVerbatim = true
                    )

                    InterpolatedStringExpr(
                        [ ConstantExpr(Int(12)); ConstantExpr(Int(12)); ConstantExpr(Int(12)) ],
                        isVerbatim = true
                    )

                    InterpolatedStringExpr([ Int(12); Int(12); Int(12) ], isVerbatim = true)
                    InterpolatedStringExpr([ "12"; "12"; "12" ], isVerbatim = true)
                }
            }

        let res = Gen.mkOak source |> Gen.run
        (*
$"""{12}"""
$"""{12}"""
$"""{12}"""
$"""{12}"""
$"""{12}{12}{12}"""
$"""{12}{12}{12}"""
$"""{12}{12}{12}"""
$"""{12}{12}{12}"""
*)

        Assert.NotNull(res)

    [<Fact>]
    let ``Text with verbatim string interpolation expressions``() =
        let source =
            Oak() {
                AnonymousModule() {
                    // $"""This is a test: {12} This is a test: {12}"""
                    InterpolatedStringExpr(
                        [ Text("This is a test: ")
                          Expr(FillExpr(ConstantExpr("12")), 1)
                          Text(" This is a test: ")
                          Expr(FillExpr(ConstantExpr("12")), 1) ],
                        isVerbatim = true
                    )

                    InterpolatedStringExpr(
                        [ Text("This is a test: ")
                          Expr(FillExpr(Int(12)), 1)
                          Text(" This is a test: ")
                          Expr(FillExpr(Int(12)), 1) ],
                        isVerbatim = true
                    )

                    InterpolatedStringExpr(
                        [ Text("This is a test: ")
                          Expr(FillExpr("12"), 1)
                          Text(" This is a test: ")
                          Expr(FillExpr("12"), 1) ],
                        isVerbatim = true
                    )

                    InterpolatedStringExpr([ Text("'{a}'"); Text(" is not a valid number") ], isVerbatim = true)
                }
            }

        let res = Gen.mkOak source |> Gen.run
        (*
$"""This is a test: {12} This is a test: {12}"""
$"""This is a test: {12} This is a test: {12}"""
$"""This is a test: {12} This is a test: {12}"""
$"""'{a}' is not a valid number"""
*)

        Assert.NotNull(res)

    [<Fact>]
    let ``Multiple dollar single expressions``() =
        let source =
            Oak() {
                AnonymousModule() {
                    // $$"""{{12}}"""
                    InterpolatedStringExpr([ ConstantExpr("12") ], dollars = 2)
                    InterpolatedStringExpr([ Int(12) ], dollars = 2)
                    InterpolatedStringExpr([ "12" ], dollars = 2)
                    InterpolatedStringExpr([ Text("{{12}}") ], isVerbatim = true, dollars = 2)

                }
            }
        (*
$$"""{{12}}"""
$$"""{{12}}"""
$$"""{{12}}"""
$$"""{{12}}"""
*)

        let res = Gen.mkOak source |> Gen.run

        Assert.NotNull(res)

    [<Fact>]
    let ``Multiple dollar multiple expressions``() =
        let source =
            Oak() {
                AnonymousModule() {
                    // $$"""{12}{12}{12}"""
                    InterpolatedStringExpr([ ConstantExpr("12"); ConstantExpr("12"); ConstantExpr("12") ], dollars = 2)

                    InterpolatedStringExpr(
                        [ Expr(FillExpr(Int(12)), 3)
                          Expr(FillExpr(Int(12)), 2)
                          Expr(FillExpr(Int(12)), 1) ],
                        dollars = 2
                    )

                    InterpolatedStringExpr([ Int(12); Int(12); Int(12) ], dollars = 2)
                    InterpolatedStringExpr([ "12"; "12"; "12" ], dollars = 2)
                    InterpolatedStringExpr([ Text("{12}"); Text("{12}"); Text("{12}") ], dollars = 2)
                }
            }
        (*
$$"""{12}{12}{12}"""
$$"""{{{12}}}{{12}}{12}"""
$$"""{12}{12}{12}"""
$$"""{12}{12}{12}"""
$$"""{12}{12}{12}"""
*)

        let res = Gen.mkOak source |> Gen.run

        Assert.NotNull(res)

    [<Fact>]
    let ``Text with multiple dollar multiple expressions``() =
        let source =
            Oak() {
                AnonymousModule() {
                    // $$"""This is a test: {12} This is a test: {12}"""
                    InterpolatedStringExpr(
                        [ Text("This is a test: ")
                          Expr(FillExpr(ConstantExpr("12")), 1)
                          Text(" This is a test: ")
                          Expr(FillExpr(ConstantExpr("12")), 1) ],
                        isVerbatim = true,
                        dollars = 2
                    )

                    InterpolatedStringExpr(
                        [ Text("This is a test: ")
                          Expr(FillExpr(Int(12)), 1)
                          Text(" This is a test: ")
                          Expr(FillExpr(Int(12)), 1) ],
                        isVerbatim = true,
                        dollars = 2
                    )

                    InterpolatedStringExpr(
                        [ Text("This is a test: ")
                          Expr(FillExpr("12"), 1)
                          Text(" This is a test: ")
                          Expr(FillExpr("12"), 1) ],
                        isVerbatim = true,
                        dollars = 2
                    )

                    InterpolatedStringExpr(
                        [ Text("'{a}'"); Text(" is not a valid number") ],
                        isVerbatim = true,
                        dollars = 2
                    )
                }
            }
        (*
$$"""This is a test: {12} This is a test: {12}"""
$$"""This is a test: {12} This is a test: {12}"""
$$"""This is a test: {12} This is a test: {12}"""
$$"""'{a}' is not a valid number"""
*)

        let res = Gen.mkOak source |> Gen.run

        Assert.NotNull(res)

    [<Fact>]
    let ``Mixed Text and Expression with escape sequences``() =
        Oak() {
            AnonymousModule() {
                // $"Line 1\nLine 2 {value}"
                InterpolatedStringExpr([ Text("Line 1\nLine 2 "); Expr(FillExpr("value"), 1) ])

                // $"Tab\tSeparated {value}"
                InterpolatedStringExpr([ Text("Tab\tSeparated "); Expr(FillExpr("value"), 1) ])

                // $"Quote\"in string {value}"
                InterpolatedStringExpr([ Text("Quote\"in string "); Expr(FillExpr("value"), 1) ])
            }
        }
        |> produces
            """
$"Line 1
Line 2 {value}"

$"Tab	Separated {value}"
$"Quote"in string {value}"
"""

    [<Fact>]
    let ``Empty interpolated string``() =
        let source =
            Oak() {
                AnonymousModule() {
                    // $""
                    InterpolatedStringExpr([ Text("") ])

                    // $""
                    InterpolatedStringExpr([ String("") ])

                    // $""""""
                    InterpolatedStringExpr([ Text("") ], isVerbatim = true)

                    // $""""""
                    InterpolatedStringExpr([ String("") ], isVerbatim = true)

                    // $$""
                    InterpolatedStringExpr([ Text("") ], dollars = 2)

                    // $$""
                    InterpolatedStringExpr([ String("") ], dollars = 2)

                    // $$""""""
                    InterpolatedStringExpr([ Text("") ], isVerbatim = true, dollars = 2)

                    // $$""""""
                    InterpolatedStringExpr([ String("") ], isVerbatim = true, dollars = 2)
                }
            }

        (*
$""
$"{""}"
$""""""
$"""{""}"""
$$""""""
$$"""{""}"""
$$""""""
$$"""{""}"""
*)
        let res = Gen.mkOak source |> Gen.run

        Assert.NotNull(res)

    [<Fact>]
    let ``Triple dollar expressions``() =
        let source =
            Oak() {
                AnonymousModule() {
                    // $$$"{{{12}}}"
                    InterpolatedStringExpr([ Expr(FillExpr(Int(12)), 1) ], dollars = 3)

                    // $$$"""{{{12}}}"""
                    InterpolatedStringExpr([ Expr(FillExpr(Int(12)), 1) ], isVerbatim = true, dollars = 3)

                    // $$$"Text with {{{value}}}"
                    InterpolatedStringExpr([ Text("Text with "); Expr(FillExpr("value"), 1) ], dollars = 3)
                }
            }

        (*
$$$"""{12}"""
$$$"""{12}"""
$$$"""Text with {value}"""
*)
        let res = Gen.mkOak source |> Gen.run

        Assert.NotNull(res)

    [<Fact>]
    let ``Nested curly braces in text segments``() =
        Oak() {
            AnonymousModule() {
                // $"Outer {Inner {value}}"
                InterpolatedStringExpr([ Text("Outer {Inner "); Expr(FillExpr("value"), 1); Text("}") ])

                // $"JSON-like { \"prop\": {value} }"
                InterpolatedStringExpr([ Text("JSON-like { \"prop\": "); Expr(FillExpr("value"), 1); Text(" }") ])

                // $"Multiple {{{{nested}}}} braces with {value}"
                InterpolatedStringExpr([ Text("Multiple {{{{nested}}}} braces with "); Expr(FillExpr("value"), 1) ])
            }
        }
        |> produces
            """
$"Outer {Inner {value}}"
$"JSON-like { "prop": {value} }"
$"Multiple {{{{nested}}}} braces with {value}"
"""

    [<Fact>]
    let ``Alignment with non-literal values``() =
        Oak() {
            AnonymousModule() {
                // $"{value,10}"
                InterpolatedStringExpr([ Expr(FillExpr(TupleExpr([ "value"; "10" ])), 1) ])

                // $"{value,-10}"
                InterpolatedStringExpr([ Expr(FillExpr(TupleExpr([ "value"; "-10" ])), 1) ])

                // $"{value,10:F2}"
                InterpolatedStringExpr([ Expr(FillExpr(TupleExpr([ "value"; "10" ]), "F2"), 1) ])
            }
        }
        |> produces
            """
$"{value, 10}"
$"{value, -10}"
$"{value, 10:F2}"
"""

    [<Fact>]
    let ``Complex expressions in interpolation``() =
        Oak() {
            AnonymousModule() {
                // $"Result: {1 + 2 * 3}"
                InterpolatedStringExpr(
                    [ Text("Result: ")
                      Expr(FillExpr(InfixAppExpr(Int(1), "+", InfixAppExpr(Int(2), "*", Int(3)))), 1) ]
                )

                // $"Items: {items |> List.length}"
                InterpolatedStringExpr([ Text("Items: "); Expr(FillExpr(PipeRightExpr("items", "List.length")), 1) ])

                // $"Conditional: {if condition then "Yes" else "No"}"
                InterpolatedStringExpr(
                    [ Text("Conditional: ")
                      Expr(FillExpr(IfThenElseExpr("condition", String("Yes"), String("No"))), 1) ]
                )
            }
        }
        |> produces
            """
$"Result: {1 + 2 * 3}"
$"Items: {items |> List.length}"
$"Conditional: {if condition then "Yes" else "No"}"
"""

    [<Fact>]
    let ``Multiple interpolated strings in sequence``() =
        Oak() {
            AnonymousModule() {
                // Sequential interpolated strings
                Value("message1", InterpolatedStringExpr([ Text("Hello "); Expr(FillExpr("name"), 1) ]))
                Value("message2", InterpolatedStringExpr([ Text("Age: "); Expr(FillExpr("age"), 1) ]))

                // Concatenating interpolated strings
                Value(
                    "combined",
                    InfixAppExpr(
                        InfixAppExpr(
                            InterpolatedStringExpr([ Text("Hello "); Expr(FillExpr("name"), 1) ]),
                            "+",
                            String(" ")
                        ),
                        "+",
                        InterpolatedStringExpr([ Text("Age: "); Expr(FillExpr("age"), 1) ])
                    )
                )
            }
        }
        |> produces
            """
let message1 = $"Hello {name}"
let message2 = $"Age: {age}"
let combined = $"Hello {name}" + " " + $"Age: {age}"
"""
