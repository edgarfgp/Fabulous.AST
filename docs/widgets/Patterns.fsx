(**
---
title: Patterns
category: widgets
index: 8
---
*)

(**
# Patterns

Patterns appear on the left of bindings, in `match` clauses, and in function
parameters. Each F# pattern form has a widget — this page groups them by kind.

## Contents
- [Named, Or, As, and Ands Patterns](#named-or-as-and-ands-patterns)
- [Tuple Patterns](#tuple-patterns)
- [List Patterns](#list-patterns)
- [Record Patterns](#record-patterns)
- [Type-test Patterns](#type-test-patterns)
- [Parameter Patterns with Attributes](#parameter-patterns-with-attributes)
*)

#r "../../src/Fabulous.AST/bin/Release/netstandard2.1/publish/Fantomas.Core.dll"
#r "../../src/Fabulous.AST/bin/Release/netstandard2.1/publish/Fabulous.AST.dll"
#r "../../src/Fabulous.AST/bin/Release/netstandard2.1/publish/Fantomas.FCS.dll"

open Fabulous.AST
open type Fabulous.AST.Ast

(**
## Named, Or, As, and Ands Patterns
`OrPat` matches either alternative, `AsPat` binds the whole match to a name, and
`AndsPat` requires all to match:
*)

Oak() {
    AnonymousModule() {
        Value(OrPat(NamedPat("A"), "B"), ConstantExpr(Constant("C")))

        Value(AsPat(NamedPat("A"), "B"), ConstantExpr(Constant("C")))

        Value(AndsPat([ "A"; "B" ]), ConstantExpr(Constant("C")))
    }
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)

(**
## Tuple Patterns
`TuplePat` destructures a tuple; `StructTuplePat` destructures a struct tuple:
*)

Oak() {
    AnonymousModule() {
        Value(TuplePat([ "a"; "b" ]), TupleExpr([ Constant("1"); Constant("2") ]))

        Value(StructTuplePat([ NamedPat("e"); NamedPat("f") ]), StructTupleExpr([ Int(1); Int(2) ]))
    }
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)

(**
## List Patterns
`ListPat` matches a list of elements; `ListConsPat` matches head-and-tail:
*)

Oak() {
    AnonymousModule() {
        Value(ListPat([ NamedPat("c"); NamedPat("d") ]), ListExpr([ String("a"); String("b") ]))

        Value(ListConsPat(NamedPat("g"), NamedPat("h")), ListExpr([ Int(1) ]))
    }
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)

(**
## Record Patterns
`RecordPat` destructures a record by field with `RecordFieldPat`:
*)

Oak() {
    AnonymousModule() {
        Value(
            RecordPat([ RecordFieldPat("A", ConstantPat(Int(3))) ]),
            RecordExpr([ RecordFieldExpr("A", ConstantExpr(Int 5)) ])
        )
    }
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)

(**
## Type-test Patterns
`IsInstPat` matches when the value is an instance of a given type — typically in
a `match` clause:
*)

Oak() {
    AnonymousModule() {
        MatchExpr(Constant("System.Object()"), [ MatchClauseExpr(IsInstPat(String()), ConstantExpr(Int(12))) ])
    }
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)

(**
## Parameter Patterns with Attributes
You can add attributes to parameter patterns using the `.attribute()` or `.attributes()` modifiers.
This is useful for adding attributes to constructor parameters or method parameters.
*)

Oak() {
    AnonymousModule() {
        // Class with attributed constructor parameter
        TypeDefn("Class", Constructor(ParameterPat("c", Int()).attribute(Attribute("Obsolete")))) {
            // Method with attributed parameter
            Member("this.First", ParenPat(ParameterPat("a", String()).attribute(Attribute("Obsolete"))), UnitExpr())

            // Method with attributed parameter using function type
            Member(
                "this.Second",
                ParenPat(ParameterPat("a", Funs(String(), Int())).attribute(Attribute("A"))),
                UnitExpr()
            )
        }

        // Class with multiple attributed constructor parameters
        TypeDefn(
            "MyClass",
            Constructor(
                TuplePat(
                    [ ParameterPat("a", Int()).attribute(Attribute("Obsolete"))
                      ParameterPat("b", String()).attribute(Attribute("Required")) ]
                )
            )
        ) {
            Member("this.Value", ConstantExpr(Int(0)))
        }

        // Parameter with multiple attributes
        TypeDefn(
            "AnotherClass",
            Constructor(ParameterPat("c", Int()).attributes([ Attribute("Obsolete"); Attribute("Required") ]))
        ) {
            Member("this.Value", ConstantExpr(Int(0)))
        }
    }
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)
