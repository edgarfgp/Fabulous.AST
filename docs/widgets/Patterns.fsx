(**
---
title: Patterns
category: widgets
index: 8
---
*)

(**
# Patterns
*)

#r "../../src/Fabulous.AST/bin/Release/netstandard2.1/publish/Fantomas.Core.dll"
#r "../../src/Fabulous.AST/bin/Release/netstandard2.1/publish/Fabulous.AST.dll"
#r "../../src/Fabulous.AST/bin/Release/netstandard2.1/publish/Fantomas.FCS.dll"

open Fabulous.AST
open type Fabulous.AST.Ast

Oak() {
    AnonymousModule() {
        Union("Union") {
            UnionCase("A")
            UnionCase("B")
            UnionCase("C")
        }

        Record("Record") { Field("A", Int()) }

        Value(OrPat(NamedPat("A"), "B"), ConstantExpr(Constant("C")))

        Value(AsPat(NamedPat("A"), "B"), ConstantExpr(Constant("C")))

        Value(AndsPat([ "A"; "B" ]), ConstantExpr(Constant("C")))

        Value(TuplePat([ "a"; "b" ]), TupleExpr([ Constant("1"); Constant("2") ]))

        Value(
            RecordPat([ RecordFieldPat("A", ConstantPat(Int(3))) ]),
            RecordExpr([ RecordFieldExpr("A", ConstantExpr(Int 5)) ])
        )

        Value(ListPat([ NamedPat("c"); NamedPat("d") ]), ListExpr([ String("a"); String("b") ]))

        Value(StructTuplePat([ NamedPat("e"); NamedPat("f") ]), StructTupleExpr([ Int(1); Int(2) ]))

        MatchExpr(Constant("System.Object()"), [ MatchClauseExpr(IsInstPat(String()), ConstantExpr(Int(12))) ])

        Value(ListConsPat(NamedPat("g"), NamedPat("h")), ListExpr([ Int(1) ]))

    }
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)

(**
## ParameterPat with Attributes
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
