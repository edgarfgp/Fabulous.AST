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
- [Overview](#Overview)
- [Primitive Types](#Primitive-Types)
- [String Literals](#String-Literals)
- [Collection Types](#Collection-Types)
- [Tuple Types](#Tuple-Types)
- [Option Types](#Option-Types)
- [Function Types](#Function-Types)
- [Generic Types](#Generic-Types)
- [Prefix and Postfix Type Syntax](#Prefix-and-Postfix-Type-Syntax)
- [Anonymous Record Types](#Anonymous-Record-Types)
- [Wildcard Types](#Wildcard-Types)
- [Nested Generic Types](#Nested-Generic-Types)
- [Type Provider Static Arguments](#Type-Provider-Static-Arguments)
- [Unit of Measure](#Unit-of-Measure)
- [Type Abbreviations](#Type-Abbreviations)
- [Explicit Type Parameters](#Explicit-Type-Parameters)
- [Flexible and Intersection Types](#Flexible-and-Intersection-Types)
- [Type Constraints](#Type-Constraints)
- [Nullable Reference Types](#Nullable-Reference-Types)

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
## Prefix and Postfix Type Syntax
Most generic types can be written in prefix form (`option<int>`) or postfix form (`int option`).
The `*Prefix` and `*Postfix` helpers generate either form, and `AppPostfix` applies any type in postfix position.
`Obj()`, `Option()` and `Seq()` are shorthands for the `obj`, `option` and `seq` type names.
*)

Oak() {
    AnonymousModule() {
        Open("System.Threading.Tasks")

        // Prefix form: array<int>, option<string>, voption<int>, seq<int>
        Value("scores", ArrayExpr([ Int(90); Int(85) ]), ArrayPrefix(Int()))
        Value("nickname", Constant("None"), OptionPrefix(String()))
        Value("cachedId", Constant("ValueSome 42"), VOptionPrefix(Int()))
        Value("evens", SeqExpr([ Int(2); Int(4) ]), SeqPrefix(Int()))

        // Postfix form: int voption, int option, string seq, int option list
        Value("lastId", Constant("ValueNone"), VOptionPostfix(Int()))
        Value("retries", Constant("Some 3"), AppPostfix(Int(), Option()))
        Value("names", SeqExpr([ String("Ada"); String("Grace") ]), AppPostfix(String(), Seq()))
        Value("readings", ListExpr([ Constant("Some 1"); Constant("None") ]), AppPostfix(OptionPostfix(Int()), List()))

        // Async<'T> and Task<'T>
        Value("fetch", AppExpr("async.Return", String("data")), AsyncPrefix(String()))
        Value("save", AppExpr("Task.FromResult", Bool(true)), TaskPrefix(Boolean()))

        // obj
        Value("boxed", AppExpr("box", Int(42)), Obj())
    }
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)

(**
## Anonymous Record Types
`AnonRecord` generates an anonymous record type from a list of field names and types.
*)

Oak() {
    AnonymousModule() {
        Value(
            "person",
            AnonRecordExpr([ RecordFieldExpr("Name", String("Ada")); RecordFieldExpr("Age", Int(36)) ]),
            AnonRecord([ "Name", String(); "Age", Int() ])
        )

        Function("describe", ParenPat(ParameterPat("p", AnonRecord([ "Name", "string"; "Age", "int" ]))), "p.Name")
    }
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)

(**
`StructAnonRecord` generates a struct anonymous record type:
*)

Oak() {
    AnonymousModule() { Function("toPoint", ParameterPat("p", StructAnonRecord([ "X", Float(); "Y", Float() ])), "p") }
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)

(**
## Wildcard Types
`Anon("_")` generates the `_` wildcard type, which asks the compiler to infer that part of the type.
*)

Oak() {
    AnonymousModule() {
        Value("ids", ListExpr([ Int(1); Int(2) ]), AppPrefix(List(), Anon("_")))

        Function("count", ParenPat(ParameterPat("xs", AppPostfix(Anon("_"), Seq()))), AppExpr("Seq.length", "xs"))
    }
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)

(**
## Nested Generic Types
`LongIdentApp` refers to a type nested inside a generic type, such as `Dictionary<string, int>.KeyCollection`.
*)

Oak() {
    AnonymousModule() {
        Open("System.Collections.Generic")

        Function(
            "keysOf",
            ParenPat(ParameterPat("d", AppPrefix("Dictionary", [ String(); Int() ]))),
            "d.Keys",
            LongIdentApp(AppPrefix("Dictionary", [ String(); Int() ]), "KeyCollection")
        )
    }
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)

(**
## Type Provider Static Arguments
Type providers take static arguments: `StaticConstant` generates a positional argument and `StaticConstantNamed` a named one.
*)

Oak() {
    AnonymousModule() {
        Abbrev(
            "Sales",
            AppPrefix(
                "CsvProvider",
                [ StaticConstant(String("sales.csv"))
                  StaticConstantNamed("Separators", StaticConstant(String(";")))
                  StaticConstantNamed("HasHeaders", StaticConstant(Bool(true))) ]
            )
        )
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
## Explicit Type Parameters
`TyparDecl` declares a type parameter. Wrap declarations in `PostfixList` for the `<'T>` form
or `SinglePrefix` for the ML-style `'a Name` form, and attach them with `.typeParams(...)`.
*)

Oak() {
    AnonymousModule() {
        // let first<'T> ...
        Function("first", ParenPat(ParameterPat("xs", ListPostfix("'T"))), AppExpr("List.head", "xs"))
            .typeParams(PostfixList(TyparDecl("'T")))

        // type 'a Bag = ...
        Abbrev("Bag", ListPostfix("'a")) |> _.typeParams(SinglePrefix("'a"))

        // Type parameter with an attribute
        Record("Vector") {
            Field("X", AppPrefix(Float(), "'u"))
            Field("Y", AppPrefix(Float(), "'u"))
        }
        |> _.typeParams(PostfixList(TyparDecl("'u").attribute(Attribute("Measure"))))
    }
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)

(**
`PrefixList` generates the ML-style prefix form, `type ('a, 'b) Pair`, and `Var` refers to a type
variable such as `'a`:
*)

Oak() { AnonymousModule() { Abbrev("Pair", Tuple([ Var("'a"); Var("'b") ])).typeParams(PrefixList([ "'a"; "'b" ])) } }
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)

(**
Constraints on `PostfixList` type parameters go inside the angle brackets
(`PostfixList(decls, constraints)`). For prefix type parameters, or to list the constraints after
the name instead, use `.constraints(...)`:
*)

Oak() {
    AnonymousModule() {
        Record("Box") { Field("Value", "'T") }
        |> _.typeParams(PrefixList("'T"))
        |> _.constraints(ConstraintSingle("'T", "equality"))
    }
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)

(**
## Flexible and Intersection Types
`HashConstraint` generates a flexible type (`#seq<int>`), `WithSubType` an inline subtype constraint (`'T :> IDisposable`),
and `Intersection` combines several constraints on one type parameter with `&`.
*)

Oak() {
    AnonymousModule() {
        // Accepts any subtype of seq<int>
        Function(
            "total",
            ParenPat(ParameterPat("xs", HashConstraint(AppPrefix(Seq(), [ Int() ])))),
            AppExpr("Seq.sum", "xs")
        )

        // Inline subtype constraint
        Function(
            "close",
            ParenPat(ParameterPat("resource", WithSubType(SubtypeOf("'T", "System.IDisposable")))),
            "resource.Dispose()"
        )

        // 'T must implement both interfaces
        Function(
            "compareResources",
            ParenPat(
                TuplePat(
                    [ ParameterPat(
                          "a",
                          Intersection(
                              [ LongIdent("'T")
                                HashConstraint("System.IDisposable")
                                HashConstraint("System.IComparable") ]
                          )
                      )
                      ParameterPat("b", "'T") ]
                )
            ),
            AppExpr("compare", [ "a"; "b" ])
        )
    }
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)

(**
A type parameter can carry several flexible constraints, joined with `&`, by passing them to `TyparDecl`:
*)

Oak() {
    AnonymousModule() {
        Function("useAndDispose", ParameterPat("x", "'T"), "x")
            .typeParams(
                PostfixList(
                    TyparDecl("'T", [ HashConstraint("System.IComparable"); HashConstraint("System.IDisposable") ])
                )
            )
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

(**
Constraints can also be declared on explicit type parameters with `PostfixList(decls, constraints)`.
`ConstraintNotStruct`, `SubtypeOf`, `EnumOrDelegate`, `Supports` (member constraints) and `WhereSelf`
(self-constrained interfaces with static abstract members) each generate one kind of constraint.
For a member constraint over several type parameters, combine them with `Or` inside `Paren`.
*)

Oak() {
    AnonymousModule() {
        // 'T: not struct
        Function("cache", ParenPat(ParameterPat("x", "'T")), "x")
            .typeParams(PostfixList(TyparDecl("'T"), ConstraintNotStruct("'T")))

        // 'T :> System.IDisposable
        Function("disposeAll", ParenPat(ParameterPat("xs", ListPostfix("'T"))), "for x in xs do x.Dispose()")
            .typeParams(PostfixList(TyparDecl("'T"), SubtypeOf("'T", "System.IDisposable")))

        // 'T: enum<int>
        Function("toValue", ParenPat(ParameterPat("x", "'T")), AppExpr("LanguagePrimitives.EnumToValue", "x"))
            .typeParams(PostfixList(TyparDecl("'T"), EnumOrDelegate("'T", "enum", Int())))

        // 'T: delegate<obj * System.EventArgs, unit>
        Function("subscribe", ParenPat(ParameterPat("handler", "'T")), "handler")
            .typeParams(
                PostfixList(
                    TyparDecl("'T"),
                    EnumOrDelegate("'T", "delegate", [ Tuple([ Obj(); LongIdent("System.EventArgs") ]); Unit() ])
                )
            )

        // ^T: (member Length: int)
        Function("length", ParenPat(ParameterPat("x", "^T")), "(^T: (member Length: int) x)")
            .typeParams(PostfixList(TyparDecl("^T"), Supports("^T", SigMember(Val("member", "Length", Int())))))
            .toInlined()

        // (^T or ^U): (static member (+): ^T * ^U -> ^T)
        Function(
            "add",
            ParenPat(TuplePat([ ParameterPat("a", "^T"); ParameterPat("b", "^U") ])),
            InfixAppExpr("a", "+", "b")
        )
            .typeParams(
                PostfixList(
                    [ TyparDecl("^T"); TyparDecl("^U") ],
                    [ Supports(
                          Paren(Or("^T", "^U")),
                          SigMember(Val([ "static"; "member" ], "(+)", Funs(Tuple([ "^T"; "^U" ]), "^T")))
                      ) ]
                )
            )
            .toInlined()

        // 'T when System.Numerics.INumber<'T>
        Function("zeroOf", UnitPat(), "'T.Zero")
            .typeParams(PostfixList(TyparDecl("'T"), WhereSelf(AppPrefix("System.Numerics.INumber", [ "'T" ]))))
    }
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)

(**
## Nullable Reference Types
With nullable reference types enabled (F# 9, `--checknulls+`), `TypeOrNull` generates a `T | null` type
and `WhereNotSupportsNull` generates the `'T: not null` constraint.
*)

Oak() {
    AnonymousModule() {
        Function(
            "lengthOrZero",
            ParenPat(ParameterPat("s", TypeOrNull(String()))),
            MatchExpr("s", [ MatchClauseExpr(NullPat(), Int(0)); MatchClauseExpr("s", "s.Length") ])
        )

        Function("describe", ParenPat(ParameterPat("x", "'T")), "x.ToString()")
            .typeParams(PostfixList(TyparDecl("'T"), WhereNotSupportsNull("'T")))
    }
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)
