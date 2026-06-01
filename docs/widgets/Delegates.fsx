(**
---
title: Delegates
category: widgets
index: 18
---
*)

(**
# Delegates

## Contents
- [Overview](#overview)
- [Basic Usage](#basic-usage)
- [Tupled and Named Parameters](#tupled-and-named-parameters)
- [Access Modifiers](#access-modifiers)
*)

#r "../../src/Fabulous.AST/bin/Release/netstandard2.1/publish/Fantomas.Core.dll"
#r "../../src/Fabulous.AST/bin/Release/netstandard2.1/publish/Fabulous.AST.dll"
#r "../../src/Fabulous.AST/bin/Release/netstandard2.1/publish/Fantomas.FCS.dll"

open Fabulous.AST
open type Fabulous.AST.Ast

(**
## Overview
A delegate names a function signature. `Delegate` takes the type name, the
parameter type(s), and the return type.

## Basic Usage
A single parameter and a return type:
*)

Oak() { AnonymousModule() { Delegate("IntTransform", Int(), Int()) } }
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)

(**
## Tupled and Named Parameters
Pass a list of types (or type names) for several parameters, and use
`SignatureParameter` to name them:
*)

Oak() {
    AnonymousModule() {
        Delegate("BinaryOp", [ "int"; "int" ], "int")

        Delegate("NamedArgs", Tuple([ SignatureParameter("a", Int()); SignatureParameter("b", Int()) ]), Int())

        Delegate("Curried", [ Paren(Tuple([ Int(); Int() ])); Paren(Tuple([ Int(); Int() ])) ], Int())
    }
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)

(**
## Access Modifiers
Set accessibility with `toPrivate`, `toInternal`, or `toPublic`:
*)

Oak() { AnonymousModule() { Delegate("InternalOp", [ "int"; "int" ], "int") |> _.toInternal() } }
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)
