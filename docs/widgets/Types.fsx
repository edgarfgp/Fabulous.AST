(**
---
title: Types
category: widgets
index: 9
---
*)

(**
# Types

## Contents
- [Overview](#overview)
- [Primitive Types](#primitive-types)
- [String Literals](#string-literals)
- [Collection Types](#collection-types)
- [Tuple Types](#tuple-types)
- [Option Types](#option-types)
- [Function Types](#function-types)
- [Generic Types](#generic-types)
- [Unit of Measure](#unit-of-measure)
- [Type Abbreviations](#type-abbreviations)
- [Type Constraints](#type-constraints)

## Overview
F# is a strongly-typed language with a rich type system. Types in F# range from simple primitives to complex user-defined types. This document provides examples of how to work with various F# types in the Fabulous.AST library.

## Primitive Types
F# includes various primitive types such as Boolean values, integers, floating-point numbers, characters, and strings.
*)

#r "../../src/Fabulous.AST/bin/Release/netstandard2.1/publish/Fantomas.Core.dll"
#r "../../src/Fabulous.AST/bin/Release/netstandard2.1/publish/Fabulous.AST.dll"
#r "../../src/Fabulous.AST/bin/Release/netstandard2.1/publish/Fantomas.FCS.dll"

open Fabulous.AST
open type Fabulous.AST.Ast

Oak() {
    AnonymousModule() {
        // Boolean
        Value(ConstantPat(Constant "a"), Bool(false), Boolean())

        // Numeric types - integers
        Value(ConstantPat(Constant "b"), Byte(0uy), Byte()) // 8-bit unsigned integer
        Value(ConstantPat(Constant "c"), SByte(1y), SByte()) // 8-bit signed integer
        Value(ConstantPat(Constant "d"), Int16(1s), Int16()) // 16-bit signed integer
        Value(ConstantPat(Constant "e"), UInt16(1us), UInt16()) // 16-bit unsigned integer
        Value(ConstantPat(Constant "f"), Int(1), Int()) // 32-bit signed integer
        Value(ConstantPat(Constant "g"), UInt32(1u), UInt32()) // 32-bit unsigned integer
        Value(ConstantPat(Constant "h"), Int64(1L), Int64()) // 64-bit signed integer
        Value(ConstantPat(Constant "i"), UInt64(1UL), UInt64()) // 64-bit unsigned integer

        // Native integers
        Value(ConstantPat(Constant "j"), IntPtr(nativeint 1), IntPtr()) // Native signed integer
        Value(ConstantPat(Constant "k"), UIntPtr(unativeint 1), UIntPtr()) // Native unsigned integer

        // Floating-point types
        Value(ConstantPat(Constant "l"), Decimal(1.0m), Decimal()) // Decimal (fixed-point number)
        Value(ConstantPat(Constant "m"), Double(4.0), Double()) // 64-bit double-precision float
        Value(ConstantPat(Constant "n"), Single(1.0f), Single()) // 32-bit single-precision float
        Value(ConstantPat(Constant "r"), Float(1.0), Float()) // Alias for double
        Value(ConstantPat(Constant "s"), Float32(1.0f), Float32()) // Alias for single

        // Character and string
        Value(ConstantPat(Constant "o"), Char('c'), Char()) // Unicode character
        Value(ConstantPat(Constant "p"), String("str"), String()) // Unicode text
    }
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)

(**
## String Literals
F# supports multiple ways to represent string literals. Each representation has specific use cases.
*)

Oak() {
    AnonymousModule() {
        // Regular string with escape sequences
        Value("regularString", String("Hello\nWorld!"), String())

        // Verbatim string with @"..." syntax (preserves all whitespace and doesn't process escape sequences)
        Value(
            "verbatimString",
            VerbatimString(
                @"C:\Program Files\App
Multiple lines
    With indentation"
            ),
            String()
        )

        // Triple-quoted string with """...""" syntax (alternative verbatim string format)
        Value("tripleQuotedString", InterpolatedStringExpr("""{"name": "John", "age": 30}"""), String())

        // Interpolated string (requires special support)
        Value("interpolatedString", InterpolatedStringExpr([ Text("Hello, "); Expr(FillExpr("name"), 1) ]), String())
    }
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)

(**
## Collection Types
F# provides several collection types, including arrays, lists, and sequences.
*)

Oak() {
    AnonymousModule() {
        // One-dimensional array
        Value("array1d", ArrayExpr([ Int(1); Int(2); Int(3) ]), Array(Int()))

        // Two-dimensional array
        Value("array2d", AppExpr("Array2D.create", [ Int(2); Int(2); Int(0) ]), Array("int", 2))

        // Lists
        Value("list", ListExpr([ Int(1); Int(2); Int(3) ]), ListPostfix(Int()))

        // Sequence
        Value("sequence", SeqExpr([ Int(1); Int(2); Int(3) ]), SeqPostfix(Int()))

        // Using alternative syntax with postfix notation
        Value("listAlt", ListExpr([ String("a"); String("b") ]), ListPostfix(String()))
        Value("seqAlt", SeqExpr([ Bool(true); Bool(false) ]), SeqPostfix(Boolean()))
    }
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)

(**
## Tuple Types
Tuples group multiple values together in an ordered structure.
*)

Oak() {
    AnonymousModule() {
        // Simple tuple (int * string)
        Value("simpleTuple", TupleExpr([ Int(1); String("hello") ]), Tuple([ Int(); String() ]))

        // Triple tuple (int * bool * string)
        Value("tripleTuple", TupleExpr([ Int(42); Bool(true); String("hello") ]), Tuple([ Int(); Boolean(); String() ]))

        // Nested tuples
        Value(
            "nestedTuple",
            TupleExpr(
                [ ConstantExpr(Int(1))
                  TupleExpr([ ConstantExpr(String("nested")); ConstantExpr(Bool(true)) ]) ]
            ),
            Tuple([ Int(); Tuple([ String(); Boolean() ]) ])
        )

        // Struct tuple (value type)
        Value("structTuple", StructTupleExpr([ Int(1); String("value") ]), StructTuple([ Int(); String() ]))
    }
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)

(**
## Option Types
Option types represent values that might not exist.
*)

Oak() {
    AnonymousModule() {
        // Option with Some value
        Value("someValue", AppExpr("Some", Int(42)), OptionPostfix(Int()))

        // Option with None value
        Value("noValue", Constant("None"), OptionPostfix(String()))

        // Using the option in a match expression
        Value(
            "optionMatch",
            MatchExpr(
                "someValue",
                [ MatchClauseExpr(LongIdentPat("Some", "x"), Constant("x"))
                  MatchClauseExpr(Constant("None"), Int(0)) ]
            ),
            Int()
        )

        // Alternative syntax
        Value("someAlt", AppExpr("Some", String("text")), OptionPostfix(String()))
    }
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)

(**
## Function Types
F# is a functional-first language where functions are first-class values.
*)

Oak() {
    AnonymousModule() {
        // Simple function (int -> string)
        Value("stringify", LambdaExpr(NamedPat("x"), AppExpr("string", Constant("x"))), Funs([ Int() ], String()))

        // Function with multiple parameters (int -> int -> int)
        Value(
            "add",
            LambdaExpr([ NamedPat("x"); NamedPat("y") ], InfixAppExpr("x", "+", "y")),
            Funs([ Int(); Int() ], Int())
        )

        // Higher-order function that takes a function as parameter
        Value(
            "applyTwice",
            LambdaExpr(
                ParenPat(
                    TuplePat(
                        [ ParameterPat(NamedPat("f"), Funs([ LongIdent("'a") ], LongIdent("'a")))
                          ParameterPat(NamedPat("x"), LongIdent("'a")) ]
                    )
                ),
                AppExpr("f", [ AppExpr("f", [ "x" ]) ])
            ),
            Funs([ Funs([ LongIdent("'a") ], LongIdent("'a")); LongIdent("'a") ], LongIdent("'a"))
        )
    }
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"
// produces the following code:
(*** include-output ***)

(**
## Generic Types
F# supports generic types with type parameters.
*)

Oak() {
    AnonymousModule() {
        // Generic list of strings
        Value("stringList", ListExpr([ String("a"); String("b") ]), ListPrefix(String()))

        // Generic dictionary with string keys and int values
        Value("dictionary", AppExpr("Dictionary"), AppPrefix(LongIdent("Dictionary"), [ String(); Int() ]))

        // Generic Result type (for success/failure scenarios)
        Value("result", AppExpr("Ok", Int(42)), ResultPrefix(Int(), String()))

        // Using a custom generic type
        Value("customGeneric", AppExpr("CustomType", String("data")), AppPrefix("CustomType", [ String() ]))
    }
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)

(**
## Unit of Measure
F# supports units of measure for type-safe numeric calculations.
*)

Oak() {
    AnonymousModule() {
        // Simple measurement
        Value("distance", ConstantExpr(ConstantMeasure("10.0", "m")), AppPrefix(Float(), "m"))

        // Compound measurement
        Value(
            "speed",
            ConstantExpr(ConstantMeasure("55.0", MeasureDivide("km", "h"))),
            AppPrefix(Float(), Tuple([ "km"; "h" ], "/"))
        )

        // Squared units
        Value(
            "area",
            ConstantExpr(ConstantMeasure("100.0", MeasurePower("m", Integer("2")))),
            AppPrefix(Float(), MeasurePowerType("m", Integer("2")))
        )

        // Calculations with units
        Value(
            "calculatedSpeed",
            InfixAppExpr(ConstantExpr(ConstantMeasure("100.0", "km")), "/", ConstantExpr(ConstantMeasure("2.0", "h"))),
            AppPrefix(Float(), Tuple([ "km"; "h" ], "/"))
        )
    }
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)

(**
## Type Abbreviations
Type abbreviations provide alternative names for existing types.
*)

Oak() {
    AnonymousModule() {
        // Simple type abbreviation
        Abbrev("UserId", Int())

        // Type abbreviation for a more complex type
        Abbrev("UserMap", AppPrefix("Dictionary", [ String(); Int() ]))

        // Using type abbreviations
        Value("userId", Int(42), LongIdent("UserId"))

        // Type abbreviation with a unit of measure
        Abbrev("Distance", "m")
        Value("myDistance", ConstantExpr(ConstantMeasure("50.0", "m")), LongIdent("Distance"))
    }
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)

(**
## Type Constraints
F# allows you to constrain type parameters to have certain properties.
*)

Oak() {
    AnonymousModule() {
        // Function with equality constraint
        Function(
            "areEqual",
            ParenPat(
                TuplePat(
                    [ ParameterPat(NamedPat("a"), WithGlobal("'T", ConstraintSingle("'T", "equality")))
                      ParameterPat(NamedPat("b"), "'T") ]
                )
            ),
            InfixAppExpr("a", "=", "b")
        )

        // Function with comparison constraint
        Function(
            "max",
            ParenPat(
                TuplePat(
                    [ ParameterPat(NamedPat("a"), WithGlobal("'T", ConstraintSingle("'T", "comparison")))
                      ParameterPat(NamedPat("b"), "'T") ]
                )
            ),
            IfThenElifExpr([ IfThenExpr(InfixAppExpr("a", ">", "b"), ConstantExpr("a")) ], ConstantExpr("b"))
        )
    }
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)
