namespace Fabulous.AST.Tests

open Fabulous.AST

open Xunit

open type Ast

module Constant =
    [<Fact>]
    let ``Constant widgets``() =
        Oak() {
            AnonymousModule() {
                let isTrue = true
                ConstantExpr(Bool(isTrue))
                ConstantExpr(Byte(0uy))
                ConstantExpr(SByte(0y))
                ConstantExpr(Int16(0s))
                ConstantExpr(UInt16(0us))
                ConstantExpr(Int(0))
                ConstantExpr(UInt32(0u))
                ConstantExpr(Int64(0L))
                ConstantExpr(UInt64(0UL))
                ConstantExpr(IntPtr(System.IntPtr.Zero))
                ConstantExpr(UIntPtr(System.UIntPtr.Zero))
                ConstantExpr(Decimal(4.0m))
                ConstantExpr(Double(4.0))
                ConstantExpr(Single(4.0f))
                ConstantExpr(String("hello"))
                ConstantExpr(Char('c'))
                ConstantExpr(Float32(4.0f))
                ConstantExpr(Constant("A"))
                ConstantExpr(Backticks("I'm a constant"))
                ConstantExpr(ConstantUnit())

                ConstantExpr(ConstantMeasure("2", "cm"))
                ConstantExpr(ConstantMeasure(Int(2), "m"))
                ConstantExpr(ConstantMeasure("2", MeasureSingle("km")))
                ConstantExpr(ConstantMeasure("2", MeasureOperator("/", "cm", "m")))
                ConstantExpr(ConstantMeasure("2", MeasureDivide("cm", "m")))
                ConstantExpr(ConstantMeasure("55.0f", MeasurePower(MeasureSingle("miles"), Integer("hour"))))

                ConstantExpr(
                    ConstantMeasure(
                        "1.0",
                        MeasureDivide(MeasureSingle("ml"), MeasurePower(MeasureSingle "cm", Integer(3)))
                    )
                )

                ConstantExpr(
                    ConstantMeasure(
                        "1.0",
                        MeasureOperator("/", MeasureSingle("ml"), MeasurePower(MeasureSingle "cm", Integer(3)))
                    )
                )

                ConstantExpr(
                    ConstantMeasure(
                        "1.0",
                        MeasureDivide(MeasureSingle("ml"), MeasurePower(MeasureSingle "cm", Negate(Integer(1))))
                    )
                )

                ConstantExpr(
                    ConstantMeasure(
                        "1.0",
                        MeasureDivide(
                            MeasureSingle("ml"),
                            MeasurePower(MeasureSingle "cm", Rational("foo", "/", "bar"))
                        )
                    )
                )

                ConstantExpr(ConstantMeasure("1.0", MeasureMultiple([ "cm"; "/"; "m" ])))

                ConstantExpr(
                    ConstantMeasure("1.0", MeasureSeq([ MeasureSingle "cm"; MeasureSingle "/"; MeasureSingle "m" ]))
                )

                ConstantExpr(
                    ConstantMeasure(
                        "1.0",
                        MeasureParen(MeasureDivide(MeasureSingle "ml", MeasurePower(MeasureSingle "cm", Integer(3))))
                    )
                )

                ConstantExpr(ConstantMeasure("1.0", MeasureParen("ml / cm^3")))
            }
        }
        |> produces
            """
true
0uy
0y
0s
0us
0
0u
0L
0UL
nativeint 0
unativeint 0
4.0m
4
4.0f
"hello"
'c'
4.0f
A
```I'm a constant```
()
2<cm>
2<m>
2<km>
2<cm / m>
2<cm / m>
55.0f<miles^hour>
1.0<ml / cm^3>
1.0<ml / cm^3>
1.0<ml / cm^-1>
1.0<ml / cm^(foo/bar)>
1.0<cm/m>
1.0<cm / m>
1.0<(ml / cm^3)>
1.0<(ml / cm^3)>
"""

    [<Fact>]
    let ``Basic string escaping``() =
        Oak() { AnonymousModule() { AppExpr("printfn", String("Hello World")) } }
        |> produces
            """
printfn "Hello World"

"""

    [<Fact>]
    let ``String with newline``() =
        Oak() { AnonymousModule() { AppExpr("printfn", String("Hello\nWorld")) } }
        |> produces
            """
printfn "Hello\nWorld"

"""

    [<Fact>]
    let ``String with quotes``() =
        Oak() { AnonymousModule() { AppExpr("printfn", String("Hello \"World\"")) } }
        |> produces
            """
printfn "Hello \"World\""
"""

    [<Fact>]
    let ``String with multiple special characters``() =
        Oak() { AnonymousModule() { AppExpr("printfn", String("Tab\there\rNewline\nQuotes\"'")) } }
        |> produces
            """
printfn "Tab\there\rNewline\nQuotes\"\'"
"""

    [<Fact>]
    let ``String with backslashes``() =
        Oak() { AnonymousModule() { AppExpr("printfn", String("Path\\to\\file")) } }
        |> produces
            """
printfn "Path\\to\\file"
"""

    [<Fact>]
    let ``String with Unicode line separators``() =
        Oak() { AnonymousModule() { AppExpr("printfn", String("Line1\u2028Line2\u2029Line3")) } }
        |> produces
            """
printfn "Line1\nLine2 Line3"
"""

    [<Fact>]
    let ``String with null character``() =
        Oak() { AnonymousModule() { AppExpr("printfn", String("Start\u0000End")) } }
        |> produces
            """
printfn "Start\0End"
"""

    [<Fact>]
    let ``Empty string``() =
        Oak() { AnonymousModule() { AppExpr("printfn", String("")) } }
        |> produces
            """
printfn ""
"""

    [<Fact>]
    let ``String with multiple consecutive newlines``() =
        Oak() { AnonymousModule() { AppExpr("printfn", String("Line1\n\nLine3")) } }
        |> produces
            """
printfn "Line1\n\nLine3"
"""

    [<Fact>]
    let ``Complex string with mixed special characters and newlines``() =
        Oak() { AnonymousModule() { AppExpr("printfn", String("Path\\to\n\"file\"\nwith\r\t special\n\\chars")) } }
        |> produces
            """
printfn "Path\\to\n\"file\"\nwith\r\t special\n\\chars"
    """

    [<Fact>]
    let ``Escaped string``() =
        Oak() { AnonymousModule() { AppExpr("printfn", String("Hello\n\"World\"")) } }
        |> produces
            """
    printfn "Hello\n\"World\""
    """

    [<Fact>]
    let ``String with various edge cases``() =
        Oak() { AnonymousModule() { AppExpr("printfn", String("'Start'\u0001\u0002\t\r\n🌟\u001F{0}%s'End'")) } }
        |> produces
            """
printfn "\'Start\'\t\r\n🌟{0}%s\'End\'"
    """

    [<Fact>]
    let ``String with only special characters``() =
        Oak() { AnonymousModule() { AppExpr("printfn", String("\n\r\t\0")) } }
        |> produces
            """
printfn "\n\r\t\\0"
    """
