(**
---
title: Rewrite
category: widgets
index: 16
---
*)

(**
# Rewrite

## Overview
`Rewrite` applies a transformation to every node of a given kind reachable from a
Fantomas Oak — through type definitions, member bodies, patterns, types and
attributes. It is handy for cross-cutting passes that aren't tied to where you
wrote the builder: fold constants, migrate a deprecated API everywhere, wrap
calls with instrumentation, convert a union to a record, and so on.

The functions are curried with the target last, so they drop into the usual
pipeline. The `WidgetBuilder<Oak>` forms return the same builder when nothing
changed (reference equality is preserved on unchanged subtrees).

| Function | Signature |
|---|---|
| `Rewrite.expr` | `(Expr -> Expr) -> WidgetBuilder<Oak> -> WidgetBuilder<Oak>` |
| `Rewrite.exprInOak` | `(Expr -> Expr) -> Oak -> Oak` |
| `Rewrite.typeDefn` | `(TypeDefn -> TypeDefn) -> WidgetBuilder<Oak> -> WidgetBuilder<Oak>` |
| `Rewrite.typeDefnInOak` | `(TypeDefn -> TypeDefn) -> Oak -> Oak` |
*)

#r "../../src/Fabulous.AST/bin/Release/netstandard2.1/publish/Fantomas.Core.dll"
#r "../../src/Fabulous.AST/bin/Release/netstandard2.1/publish/Fabulous.AST.dll"
#r "../../src/Fabulous.AST/bin/Release/netstandard2.1/publish/Fantomas.FCS.dll"

open Fabulous.AST
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text
open type Fabulous.AST.Ast

(**
## Rewriting expressions
`Rewrite.expr` applies your function to **every** `Expr` in the tree (bottom-up,
children first). That makes it the right tool for cross-cutting changes that
aren't tied to any single builder call — the kind of thing other ecosystems
reach for AST passes to do. A couple of small helpers for building raw `Expr`
nodes, used by the examples below:
*)

let ident(name: string) : Expr =
    Expr.Constant(Constant.FromText(SingleTextNode(name, Range.Zero)))

let intExpr(value: int) : Expr =
    Expr.Constant(Constant.FromText(SingleTextNode(string value, Range.Zero)))

let (|IntLit|_|)(e: Expr) : int option =
    match e with
    | Expr.Constant(Constant.FromText n) ->
        match System.Int32.TryParse n.Text with
        | true, v -> Some v
        | _ -> None
    | _ -> None

(**
### Optimize: constant folding
Generators often emit naive, uniform expressions (`x * 1 + 0`) because that is
the easiest thing to produce from data. A folding pass tidies them up afterwards
— the same idea as an optimizing compiler's constant-folding pass (LLVM, GCC) or
a JavaScript minifier. The generator stays simple; the cleanup is one reusable
pass over the result. Because the walk is bottom-up, nested redundancy collapses
in a single pass.
*)

let foldConstants(e: Expr) : Expr =
    match e with
    | Expr.InfixApp n ->
        match n.Operator.Text, n.LeftHandSide, n.RightHandSide with
        | "+", lhs, IntLit 0
        | "+", IntLit 0, lhs
        | "*", lhs, IntLit 1
        | "*", IntLit 1, lhs -> lhs
        | "+", IntLit a, IntLit b -> intExpr(a + b)
        | "*", IntLit a, IntLit b -> intExpr(a * b)
        | _ -> e
    | _ -> e

Oak() {
    AnonymousModule() {
        Value("area", InfixAppExpr(InfixAppExpr(ConstantExpr(Constant "width"), "*", Int(1)), "+", Int(0)))

        Value("total", InfixAppExpr(Int(2), "+", Int(3)))
    }
}
|> Rewrite.expr foldConstants
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)

(**
### Migrate an API (codemod)
Rewrite a non-idiomatic call into its better form wherever it appears — the same
job a [jscodeshift](https://github.com/facebook/jscodeshift) React codemod, a
Scalafix rule, or `go fix` does. Here `List.length xs = 0` (which is O(n)) becomes
`List.isEmpty xs` (O(1)) across the whole tree:
*)

let useIsEmpty(e: Expr) : Expr =
    match e with
    | Expr.InfixApp n when n.Operator.Text = "=" ->
        match n.LeftHandSide, n.RightHandSide with
        | Expr.App app, IntLit 0 ->
            match app.FunctionExpr with
            | Expr.Constant(Constant.FromText f) when f.Text = "List.length" ->
                Expr.App(ExprAppNode(ident "List.isEmpty", List.ofSeq app.Arguments, Range.Zero))
            | _ -> e
        | _ -> e
    | _ -> e

Oak() {
    AnonymousModule() {
        Value(
            "isEmpty",
            InfixAppExpr(AppExpr(ConstantExpr(Constant "List.length"), [ ConstantExpr(Constant "xs") ]), "=", Int(0))
        )
    }
}
|> Rewrite.expr useIsEmpty
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)

(**
### Instrument calls
Weave a cross-cutting concern into generated code without touching every call
site — the way OpenTelemetry's auto-instrumentation or an AspectJ aspect does.
Here every `compute` call is wrapped with a timing helper. (No infinite loop: the
walk is a single pass, so the newly-created `withTiming` node is not revisited.)
*)

let instrument(e: Expr) : Expr =
    match e with
    | Expr.App app ->
        match app.FunctionExpr with
        | Expr.Constant(Constant.FromText f) when f.Text = "compute" ->
            let wrapped =
                Expr.Paren(
                    ExprParenNode(SingleTextNode("(", Range.Zero), e, SingleTextNode(")", Range.Zero), Range.Zero)
                )

            Expr.App(ExprAppNode(ident "withTiming", [ wrapped ], Range.Zero))
        | _ -> e
    | _ -> e

Oak() {
    AnonymousModule() { Value("result", AppExpr(ConstantExpr(Constant "compute"), [ ConstantExpr(Constant "input") ])) }
}
|> Rewrite.expr instrument
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)

(**
## Rewriting type definitions
`Rewrite.typeDefn` visits each `TypeDefn`, so a pass can replace one shape with
another. This converts a single-case union into the equivalent record by reusing
the case's fields (a `UnionCaseNode`'s `Fields` are already `FieldNode`s):
*)

let unionToRecord(td: TypeDefn) : TypeDefn =
    match td with
    | TypeDefn.Union n when n.UnionCases.Length = 1 ->
        let itd = n :> ITypeDefn

        TypeDefn.Record(
            TypeDefnRecordNode(
                itd.TypeName,
                n.Accessibility,
                SingleTextNode("{", Range.Zero),
                n.UnionCases.Head.Fields,
                SingleTextNode("}", Range.Zero),
                itd.Members,
                Range.Zero
            )
        )
    | _ -> td

Oak() { AnonymousModule() { Union("Point") { UnionCase("Point", [ Field("X", Int()); Field("Y", Int()) ]) } } }
|> Rewrite.typeDefn unionToRecord
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)

(**
## Working with raw Oak nodes
When you already hold a raw `Oak` (rather than a `WidgetBuilder<Oak>`), use the
`InOak` variants. They take the same transformation and return a rewritten `Oak`:

```fsharp
let oak = Gen.mkOak widget
let optimized = oak |> Rewrite.exprInOak foldConstants
let asRecord = oak |> Rewrite.typeDefnInOak unionToRecord
```
*)
