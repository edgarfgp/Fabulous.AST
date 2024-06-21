(**
---
title: Patterns
category: widgets
index: 3
---
*)

(**
# Patterns
*)

#r "../../src/Fabulous.AST/bin/Release/netstandard2.1/publish/Fantomas.Core.dll"
#r "../../src/Fabulous.AST/bin/Release/netstandard2.1/publish/Fabulous.AST.dll"
#r "../../src/Fabulous.AST/bin/Release/netstandard2.1/publish/Fantomas.Core.dll"
#r "../../src/Fabulous.AST/bin/Release/netstandard2.1/publish/Fantomas.FCS.dll"

open System
open Fabulous.AST
open Fantomas.Core
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
|> CodeFormatter.FormatOakAsync
|> Async.RunSynchronously
|> printfn "%s"

(**
Will output the following code:
*)

type Union =
    | A
    | B
    | C

type Record = { A: int }

let A | B = C

let A as B = C

let A & B = C

let a, b = 1, 2

let { A = 3 } = { A = 5 }

let [ c; d ] = [ "a"; "b" ]

let struct (e, f) = struct (1, 2)

match System.Object() with
| :? string -> 12

let g :: h = [ 1 ]
