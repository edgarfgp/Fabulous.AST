(**
---
title: Computation Expressions
category: widgets
index: 20
---
*)

(**
# Computation Expressions

## Contents
- [Overview](#overview)
- [Basic Usage](#basic-usage)
- [Named Computation Expressions](#named-computation-expressions)
- [Multi-statement Bodies](#multi-statement-bodies)
*)

#r "../../src/Fabulous.AST/bin/Release/netstandard2.1/publish/Fantomas.Core.dll"
#r "../../src/Fabulous.AST/bin/Release/netstandard2.1/publish/Fabulous.AST.dll"
#r "../../src/Fabulous.AST/bin/Release/netstandard2.1/publish/Fantomas.FCS.dll"

open Fabulous.AST
open type Fabulous.AST.Ast

(**
## Overview
A computation expression wraps a body in `{ ... }`. `ComputationExpr` produces an
anonymous block; `NamedComputationExpr` prefixes it with a builder name such as
`seq`, `async`, or `task`.

## Basic Usage
*)

Oak() { AnonymousModule() { Value("block", ComputationExpr(ConstantExpr(String "a"))) } }
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)

(**
## Named Computation Expressions
Pass the builder name to `NamedComputationExpr`:
*)

Oak() {
    AnonymousModule() {
        Value("greeting", NamedComputationExpr("async", ConstantExpr(String "hello")))

        Value("work", NamedComputationExpr("task", ConstantExpr(String "done")))
    }
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)

(**
## Multi-statement Bodies
Use `CompExprBodyExpr` to place several statements in the body:
*)

Oak() {
    AnonymousModule() {
        Value("numbers", NamedComputationExpr("seq", CompExprBodyExpr([ "yield 1"; "yield 2"; "yield 3" ])))
    }
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)
