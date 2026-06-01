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
wrote the builder: rename every occurrence of a function call, convert a union to
a record, strip XML docs, and so on.

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
`Rewrite.expr` visits every `Expr` and applies your function after each node's
children are rebuilt (bottom-up). Here we rename a function call. The widget DSL
emits constants for raw identifier text (`ConstantExpr(Constant "x")` →
`Expr.Constant`), so a real rewrite covers both `Expr.Ident` and `Expr.Constant`:
*)

let renameIdent (oldName: string) (newName: string) (e: Expr) : Expr =
    match e with
    | Expr.Ident n when n.Text = oldName -> Expr.Ident(SingleTextNode(newName, Range.Zero))
    | Expr.Constant(Constant.FromText n) when n.Text = oldName ->
        Expr.Constant(Constant.FromText(SingleTextNode(newName, Range.Zero)))
    | _ -> e

Oak() { AnonymousModule() { Value("greet", AppExpr(ConstantExpr(Constant "println"), [ Constant "msg" ])) } }
|> Rewrite.expr(renameIdent "println" "printfn")
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
let renamed = oak |> Rewrite.exprInOak (renameIdent "println" "printfn")
let asRecord = oak |> Rewrite.typeDefnInOak unionToRecord
```
*)
