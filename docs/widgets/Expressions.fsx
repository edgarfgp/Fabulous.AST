(**
---
title: Expressions
category: widgets
index: 7
---
*)

(**
# Expressions

Expressions are the building blocks of values, member bodies, and statements. The
DSL exposes one widget per F# expression form — this page groups them by family.

## Contents
- [Identifiers and Literals](#Identifiers-and-Literals)
- [Collections and Tuples](#Collections-and-Tuples)
- [Application and Operators](#Application-and-Operators)
- [Pipelines and Operators](#Pipelines-and-Operators)
- [Method Chains](#Method-Chains)
- [Lambdas](#Lambdas)
- [Conditionals and Pattern Matching](#Conditionals-and-Pattern-Matching)
- [Loops](#Loops)
- [Indexing and Slicing](#Indexing-and-Slicing)
- [Mutation](#Mutation)
- [Type Annotations and Type Application](#Type-Annotations-and-Type-Application)
- [Exception Handling](#Exception-Handling)
- [Other Expressions](#Other-Expressions)
*)

#r "../../src/Fabulous.AST/bin/Release/netstandard2.1/publish/Fantomas.Core.dll"
#r "../../src/Fabulous.AST/bin/Release/netstandard2.1/publish/Fabulous.AST.dll"
#r "../../src/Fabulous.AST/bin/Release/netstandard2.1/publish/Fantomas.FCS.dll"

open Fabulous.AST
open type Fabulous.AST.Ast

(**
## Identifiers and Literals
`IdentExpr` references an identifier, `NullExpr` emits the `null` literal, and
`InterpolatedStringExpr` builds an interpolated string, optionally triple-quoted
(`isTripleQuoted`), verbatim (`isVerbatim`, `$@"..."`), or with extra `$` for nested braces (`dollars`):
*)

Oak() {
    AnonymousModule() {
        IdentExpr("value")

        NullExpr()

        InterpolatedStringExpr(ConstantExpr("12"))

        InterpolatedStringExpr([ "a"; "b"; "c" ], isTripleQuoted = true)

        InterpolatedStringExpr([ ConstantExpr("12"); ConstantExpr("12") ], isTripleQuoted = true, dollars = 1)

        InterpolatedStringExpr([ Text(@"C:\Users\"); Expr(FillExpr("name"), 1) ], isVerbatim = true)
    }
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)

(**
## Collections and Tuples
Lists, arrays, sequences, tuples (and their struct variants), and anonymous
records. `EmptyListExpr` and `EmptyArrayExpr` are shorthands for `[]` and `[||]`:
*)

Oak() {
    AnonymousModule() {
        ListExpr([ String("a"); String("b"); String("c") ])

        EmptyListExpr()

        ArrayExpr([ String("a"); String("b"); String("c") ])

        EmptyArrayExpr()

        SeqExpr([ String("a"); String("b") ])

        TupleExpr([ ConstantExpr(Int 1); ConstantExpr(Int 2); ConstantExpr(Int 3) ])

        StructTupleExpr([ ConstantExpr(Int 1); ConstantExpr(Int 2) ])

        AnonRecordExpr([ RecordFieldExpr("A", Int(1)); RecordFieldExpr("B", Int(2)) ])

        AnonStructRecordExpr([ RecordFieldExpr("A", Int(1)); RecordFieldExpr("B", Int(2)) ])
    }
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)

(**
## Application and Operators
`AppExpr` applies a function, `AppSingleParenArgExpr` applies a function to a single
parenthesized argument in method-call style, `InfixAppExpr` applies a binary operator,
and `ChainExpr` builds a dotted call chain (see [Method Chains](#Method-Chains)):
*)

Oak() {
    AnonymousModule() {
        AppExpr("printfn", String("Hello, World!"))

        AppSingleParenArgExpr(IdentExpr("Console.WriteLine"), ParenExpr(String("Hello")))

        InfixAppExpr(Int(1), "+", Int(2))

        ChainExpr(
            [ ChainLinkExpr(String("string"))
              ChainLinkDot()
              ChainLinkExpr(OptVarExpr("Length")) ]
        )
    }
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)

(**
## Pipelines and Operators
- `PipeRightExpr` builds a forward pipe `x |> f`; nest them to chain several steps.
- `PipeLeftExpr` builds a backward pipe `f <| x`.
- `PrefixAppExpr` applies a prefix operator such as `-` or `~~~`.
- `ParenFunctionNameWithStarExpr` references an operator containing `*` as a function, adding the spaces that keep `( * )` from being read as a comment.
- `DynamicExpr` builds a dynamic lookup `obj?name`, which requires a user-defined `?` operator.
*)

Oak() {
    AnonymousModule() {
        PipeRightExpr("xs", AppExpr("List.map", "f"))

        PipeRightExpr(PipeRightExpr("xs", AppExpr("List.filter", "isEven")), AppExpr("List.map", "square"))

        PipeLeftExpr(AppExpr("printfn", String("%d")), InfixAppExpr("1", "+", "2"))

        PrefixAppExpr("-", "x")

        PrefixAppExpr("~~~", "flags")

        AppExpr(ParenFunctionNameWithStarExpr("*"), [ "2"; "3" ])

        DynamicExpr("obj", "Name")
    }
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)

(**
## Method Chains
`ChainExpr` takes a receiver followed by `ChainLinkDot()` + one link per step:
- `ChainLinkIdentifier` is a plain name such as the receiver or a property.
- `ChainLinkAppParen` is a method call with a parenthesized argument, like `Append("a")`.
- `ChainLinkAppUnit` is a method call with unit, like `ToString()`.
- `ChainLinkIndex` is an indexer step, like `.[0]`.
*)

Oak() {
    AnonymousModule() {
        ChainExpr(
            [ ChainLinkIdentifier("builder")
              ChainLinkDot()
              ChainLinkAppParen("Append", ConstantExpr(String("a")))
              ChainLinkDot()
              ChainLinkAppUnit("ToString") ]
        )

        ChainExpr(
            [ ChainLinkIdentifier("person")
              ChainLinkDot()
              ChainLinkIdentifier("Name")
              ChainLinkDot()
              ChainLinkAppUnit("ToUpper") ]
        )

        ChainExpr(
            [ ChainLinkIdentifier("items")
              ChainLinkDot()
              ChainLinkIndex(Int(0))
              ChainLinkDot()
              ChainLinkIdentifier("Name") ]
        )
    }
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)

(**
## Lambdas
`LambdaExpr` for a bare lambda, `ParenLambdaExpr` for a parenthesized one, and
`MatchLambdaExpr` for the `function` form:
*)

Oak() {
    AnonymousModule() {
        LambdaExpr(UnitPat(), Int(1))

        ParenLambdaExpr([ ConstantPat("a"); ConstantPat("b") ], ConstantExpr("a"))

        MatchLambdaExpr([ MatchClauseExpr("a", Int(3)) ])
    }
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)

(**
Lambdas passed as the last argument of a call:
- `AppWithLambdaExpr` applies a function to its arguments followed by a `fun` lambda.
- `AppWithMatchLambdaExpr` does the same with a `function` lambda.
- `DotLambdaExpr` builds the F# 8 shorthand `_.Property`.
*)

Oak() {
    AnonymousModule() {
        AppWithLambdaExpr("Array.init", [ "5" ], [ "i" ], "i * i")

        PipeRightExpr("xs", AppWithLambdaExpr("List.map", [ "x" ], "x * 2"))

        AppWithMatchLambdaExpr(
            ConstantExpr("List.choose"),
            [],
            [ MatchClauseExpr("Some x", "Some(x + 1)"); MatchClauseExpr("None", "None") ]
        )

        PipeRightExpr("people", AppExpr("List.map", DotLambdaExpr("Name")))
    }
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)

(**
## Conditionals and Pattern Matching
`IfThenElseExpr`, the multi-branch `IfThenElifExpr`, and `MatchExpr` with a list
of `MatchClauseExpr`:
*)

Oak() {
    AnonymousModule() {
        IfThenElseExpr(Bool(true), String("a"), String("b"))

        IfThenElifExpr(
            [ IfThenExpr(
                  InfixAppExpr(ConstantExpr(Constant "x"), "=", ConstantExpr(Int 1)),
                  ConstantExpr(String("one"))
              )
              ElIfThenExpr(
                  InfixAppExpr(ConstantExpr(Constant "x"), "=", ConstantExpr(Int 2)),
                  ConstantExpr(String("two"))
              ) ],
            ConstantExpr(String("other"))
        )

        MatchExpr(
            Int(1),
            [ MatchClauseExpr(Int(1), String("a"))
              MatchClauseExpr(WildPat(), String("b")) ]
        )
    }
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)

(**
Use `ElseIfThenExpr` instead of `ElIfThenExpr` to emit `else if` rather than `elif`:
*)

Oak() {
    AnonymousModule() {
        IfThenElifExpr(
            [ IfThenExpr(InfixAppExpr("x", "<", Int(0)), ConstantExpr(String("negative")))
              ElseIfThenExpr(InfixAppExpr("x", "=", Int(0)), ConstantExpr(String("zero"))) ],
            ConstantExpr(String("positive"))
        )
    }
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)

(**
## Loops
`ForEachDoExpr`, the counted `ForToExpr` / `ForDownToExpr`, and `WhileExpr`:
*)

Oak() {
    AnonymousModule() {
        ForEachDoExpr("i", ListExpr([ Int(1); Int(2); Int(3) ]), AppExpr("printf", String("%i")))

        ForToExpr("i", ConstantExpr("1"), ConstantExpr("10"), ConstantExpr(ConstantUnit()))

        ForDownToExpr("i", ConstantExpr("10"), ConstantExpr("1"), ConstantExpr(ConstantUnit()))

        WhileExpr(ConstantExpr(Bool(true)), ConstantExpr(Int(0)))
    }
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)

(**
## Indexing and Slicing
- `IndexWithoutDotExpr` indexes with the F# 6 syntax `arr[i]`; its index can be a key, a range, or a tuple for multi-dimensional arrays.
- `DotIndexedGetExpr` indexes with the older `arr.[i]` syntax.
- `IndexRangeExpr(from, to)`, `IndexFromRangeExpr` and `IndexToRangeExpr` build the slices `1..3`, `2..` and `..2`.
- `IndexRangeWildcardExpr` is the `*` wildcard that takes a whole dimension.
- `IndexFromEndExpr` counts from the end with `^` (this needs `--langversion:preview` at the time of writing).
- `TripleNumberIndexRangeExpr` builds a stepped range `start..step..finish`.
- `NestedIndexWithoutDotExpr` indexes a value and applies the result to an argument.
*)

Oak() {
    AnonymousModule() {
        IndexWithoutDotExpr(IdentExpr("dict"), String("key"))

        DotIndexedGetExpr("arr", Int(0))

        IndexWithoutDotExpr(IdentExpr("arr"), IndexRangeExpr("1", "3"))

        DotIndexedGetExpr("arr", IndexRangeExpr("1", "3"))

        IndexWithoutDotExpr(IdentExpr("arr"), IndexFromRangeExpr("2"))

        IndexWithoutDotExpr(IdentExpr("arr"), IndexToRangeExpr("2"))

        IndexWithoutDotExpr(IdentExpr("grid"), TupleExpr([ IndexRangeWildcardExpr(); ConstantExpr("0") ]))

        IndexWithoutDotExpr(IdentExpr("arr"), IndexFromEndExpr("1"))

        ListExpr([ TripleNumberIndexRangeExpr("0", "2", "10") ])

        NestedIndexWithoutDotExpr("handlers", "0", "event")
    }
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)

(**
## Mutation
- `SetExpr` assigns a mutable value with `<-`.
- `DotIndexedSetExpr` assigns through an indexer, `arr.[i] <- v`.
- `NamedIndexedPropertySetExpr` assigns through a named indexed property, `list.Item(0) <- v`.
- `DotNamedIndexedPropertySetExpr` does the same with the receiver and property name passed separately. Wrap the index in `ParenExpr`, or it is printed without parentheses.
*)

Oak() {
    AnonymousModule() {
        SetExpr(IdentExpr("count"), InfixAppExpr("count", "+", Int(1)))

        DotIndexedSetExpr("arr", Int(0), Int(42))

        NamedIndexedPropertySetExpr("list.Item", ParenExpr(Int(0)), Int(42))

        DotNamedIndexedPropertySetExpr(IdentExpr("dict"), "Item", ParenExpr(String("key")), Int(1))
    }
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)

(**
## Type Annotations and Type Application
- `TypedExpr` annotates a value with `:`, upcasts with `:>` or downcasts with `:?>`; the operator is passed as a string.
- `TypeAppExpr` supplies explicit type arguments, like `typeof<int>`.
- `TyparExpr` references a type parameter as an expression, e.g. to call a static member on `'T`.
- `TraitCallExpr` invokes a member through a statically resolved type parameter constraint.
*)

Oak() {
    AnonymousModule() {
        TypedExpr(ConstantExpr("42"), ":", Int())

        ParenExpr(TypedExpr("x", ":>", "obj"))

        ParenExpr(TypedExpr("o", ":?>", "string"))

        TypeAppExpr("typeof", "int")

        TypeAppExpr("Map.empty", [ "string"; "int" ])

        ChainExpr(
            [ ChainLinkIdentifier(TyparExpr("'T"))
              ChainLinkDot()
              ChainLinkIdentifier("Zero") ]
        )

        ParenExpr(
            TraitCallExpr(
                Paren("^N"),
                SigMember(Val([ "static"; "member" ], "Parse", Funs("string", "^N"))),
                ConstantExpr(String("42"))
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
## Exception Handling
`TryWithExpr` takes a list of `MatchClauseExpr` handlers, or an exception name and a
single handler; `TryWithSingleClauseExpr` and `TryFinallyExpr` cover the other forms:
*)

Oak() {
    AnonymousModule() {
        TryWithExpr(
            AppExpr("int", "input"),
            [ MatchClauseExpr(IsInstPat("FormatException"), Int(0))
              MatchClauseExpr(AsPat(IsInstPat("OverflowException"), NamedPat("ex")), AppExpr("failwith", "ex.Message")) ]
        )

        TryWithExpr(AppExpr("File.ReadAllText", "path"), "ex", ConstantExpr(String("")))

        TryWithSingleClauseExpr(Int(12), MatchClauseExpr(WildPat(), FailWithExpr(String("Not implemented"))))

        TryFinallyExpr(Int(12), Int(12))
    }
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)

(**
## Other Expressions
`LazyExpr`, `QuotedExpr` (and `UntypedQuotedExpr` for `<@@ ... @@>`), `BeginEndExpr` (a `begin ... end` block), `InheritRecordExpr`
(the `{ inherit Base(...); Field = ... }` form used in explicit constructors), the object
expression `ObjExpr`, and named computation expressions such as `task { ... }`:
*)

Oak() {
    AnonymousModule() {
        LazyExpr(Int(12))

        BeginEndExpr(AppExpr("printfn", String("hello")))

        InheritRecordExpr(InheritParen("Exception", ConstantExpr("message")), [ RecordFieldExpr("Code", "code") ])

        QuotedExpr(InfixAppExpr(Int(1), "+", Int(2)))

        UntypedQuotedExpr(InfixAppExpr(Int(1), "+", Int(2)))

        NamedComputationExpr(ConstantExpr(Constant "task"), String("a"))

        ObjExpr(LongIdent("System.Object"), ConstantExpr(ConstantUnit())) {
            Member("x.ToString()", ConstantExpr(String("F#")))
        }
    }
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)

(**
An object expression can implement more interfaces than its own type: put `InterfaceWith(...)`
blocks after its members. It needs at least one member of its own, since Fantomas prints one with
only extra interfaces in a form the compiler rejects:
*)

Oak() {
    AnonymousModule() {
        Value(
            "resource",
            ObjExpr("System.IDisposable") {
                Member("_.Dispose()", ConstantExpr(ConstantUnit()))

                InterfaceWith("System.IFormattable") {
                    Member(
                        "_.ToString(format: string, provider: System.IFormatProvider)",
                        ConstantExpr(String("resource"))
                    )
                }
            }
        )
    }
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)
