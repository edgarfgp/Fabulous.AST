namespace Fabulous.AST.Tests

open Fabulous.AST

open type Ast
open Xunit

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
