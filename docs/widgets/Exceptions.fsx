(**
---
title: Exceptions
category: widgets
index: 19
---
*)

(**
# Exceptions

## Contents
- [Overview](#overview)
- [Basic Usage](#basic-usage)
- [Exceptions with Fields](#exceptions-with-fields)
- [Exception Members](#exception-members)
*)

#r "../../src/Fabulous.AST/bin/Release/netstandard2.1/publish/Fantomas.Core.dll"
#r "../../src/Fabulous.AST/bin/Release/netstandard2.1/publish/Fabulous.AST.dll"
#r "../../src/Fabulous.AST/bin/Release/netstandard2.1/publish/Fantomas.FCS.dll"

open Fabulous.AST
open type Fabulous.AST.Ast

(**
## Overview
`ExceptionDefn` declares a custom exception type. In its simplest form it takes
just a name.

## Basic Usage
*)

Oak() { AnonymousModule() { ExceptionDefn("ParseError") } }
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)

(**
## Exceptions with Fields
Carry data on the exception by passing a single `Field`, a list of fields, or a
list of type names:
*)

Oak() {
    AnonymousModule() {
        ExceptionDefn("NotFound", Field("key", String()))

        ExceptionDefn("ValidationError", [ Field("field", String()); Field("code", Int()) ])

        ExceptionDefn("Failure", [ "string"; "int" ])
    }
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)

(**
## Exception Members
Add members with `.members()`:
*)

Oak() {
    AnonymousModule() {
        ExceptionDefn("AppError", Field("message", String())).members() {
            Member(ConstantPat(Constant("DefaultMessage")), ConstantExpr(String("unknown"))).toStatic()
        }
    }
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)
