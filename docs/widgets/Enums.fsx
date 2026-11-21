(**
---
title: Enums
category: widgets
index: 17
---
*)

(**
# Enums

## Contents
- [Overview](#Overview)
- [Basic Usage](#Basic-Usage)
- [Enum Cases](#Enum-Cases)
- [Attributes and XML Documentation](#Attributes-and-XML-Documentation)
*)

#r "../../src/Fabulous.AST/bin/Release/netstandard2.1/publish/Fantomas.Core.dll"
#r "../../src/Fabulous.AST/bin/Release/netstandard2.1/publish/Fabulous.AST.dll"
#r "../../src/Fabulous.AST/bin/Release/netstandard2.1/publish/Fantomas.FCS.dll"

open Fabulous.AST
open type Fabulous.AST.Ast

(**
## Overview
An enumeration is a type with a fixed set of named integer constants. Build one
with `Enum` and one `EnumCase` per value.

## Basic Usage
*)

Oak() {
    AnonymousModule() {
        Enum("Color") {
            EnumCase("Red", Int(0))
            EnumCase("Green", Int(1))
            EnumCase("Blue", Int(2))
        }
    }
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)

(**
## Enum Cases
Each case pairs a name with a constant value. The value can be a constant widget
(`Int`, `Char`, ...) or a full `ConstantExpr`:
*)

Oak() {
    AnonymousModule() {
        Enum("FileAccess") {
            EnumCase("Read", Int(1))
            EnumCase("Write", Int(2))
            EnumCase("ReadWrite", ConstantExpr(Int 3))
        }
    }
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)

(**
## Attributes and XML Documentation
Add attributes and documentation with the unified modifiers:
*)

Oak() {
    AnonymousModule() {
        Enum("Color") {
            EnumCase("Red", Int(0))
            EnumCase("Green", Int(1))
            EnumCase("Blue", Int(2))
        }
        |> _.attribute(Attribute("Flags"))
        |> _.xmlDocs([ "The primary colors" ])
    }
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)
