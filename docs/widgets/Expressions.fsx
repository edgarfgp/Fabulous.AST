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
- [Identifiers and Literals](#identifiers-and-literals)
- [Collections and Tuples](#collections-and-tuples)
- [Application and Operators](#application-and-operators)
- [Lambdas](#lambdas)
- [Conditionals and Pattern Matching](#conditionals-and-pattern-matching)
- [Loops](#loops)
- [Exception Handling](#exception-handling)
- [Other Expressions](#other-expressions)
*)

#r "../../src/Fabulous.AST/bin/Release/netstandard2.1/publish/Fantomas.Core.dll"
#r "../../src/Fabulous.AST/bin/Release/netstandard2.1/publish/Fabulous.AST.dll"
#r "../../src/Fabulous.AST/bin/Release/netstandard2.1/publish/Fantomas.FCS.dll"

open Fabulous.AST
open type Fabulous.AST.Ast

(**
## Identifiers and Literals
`IdentExpr` references an identifier and `InterpolatedStringExpr` builds an
interpolated string (optionally verbatim, or with extra `$` for nested braces):
*)

Oak() {
    AnonymousModule() {
        IdentExpr("value")

        InterpolatedStringExpr(ConstantExpr("12"))

        InterpolatedStringExpr([ "a"; "b"; "c" ], isVerbatim = true)

        InterpolatedStringExpr([ ConstantExpr("12"); ConstantExpr("12") ], isVerbatim = true, dollars = 1)
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
records:
*)

Oak() {
    AnonymousModule() {
        ListExpr([ String("a"); String("b"); String("c") ])

        ArrayExpr([ String("a"); String("b"); String("c") ])

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
`AppExpr` applies a function, `InfixAppExpr` applies a binary operator, and
`ChainExpr` builds a dotted call chain:
*)

Oak() {
    AnonymousModule() {
        AppExpr("printfn", String("Hello, World!"))

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
## Exception Handling
`TryWithExpr` (and the single-clause `TryWithSingleClauseExpr`) and `TryFinallyExpr`:
*)

Oak() {
    AnonymousModule() {
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
`LazyExpr`, `QuotedExpr`, the object expression `ObjExpr`, and named computation
expressions such as `task { ... }`:
*)

Oak() {
    AnonymousModule() {
        LazyExpr(Int(12))

        QuotedExpr(InfixAppExpr(Int(1), "+", Int(2)))

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
