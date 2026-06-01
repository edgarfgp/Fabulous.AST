(**
---
title: Interfaces
category: widgets
index: 11
---
*)

(**
# Interfaces

Interfaces specify sets of related members that other classes implement. Define
one as a `TypeDefn` whose members are `AbstractMember`s, and implement it on a
type with `InterfaceWith`.

See the official [F# interfaces documentation](https://learn.microsoft.com/en-us/dotnet/fsharp/language-reference/interfaces) for background.

## Contents
- [Defining an Interface](#defining-an-interface)
- [Abstract Members with Parameters](#abstract-members-with-parameters)
- [Implementing an Interface](#implementing-an-interface)
*)

#r "../../src/Fabulous.AST/bin/Release/netstandard2.1/publish/Fantomas.Core.dll"
#r "../../src/Fabulous.AST/bin/Release/netstandard2.1/publish/Fabulous.AST.dll"
#r "../../src/Fabulous.AST/bin/Release/netstandard2.1/publish/Fantomas.FCS.dll"

open Fabulous.AST
open type Fabulous.AST.Ast

(**
## Defining an Interface
An interface is a type containing only abstract members:
*)

Oak() { AnonymousModule() { TypeDefn("INumeric") { AbstractMember("Add", [ Int(); Int() ], Int()) } } }
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)

(**
## Abstract Members with Parameters
`AbstractMember` accepts named or unnamed parameter types and a return type. The
trailing `true` requests the .NET-style tupled signature:
*)

Oak() {
    AnonymousModule() {
        TypeDefn("ISprintable") { AbstractMember("Print", [ ("format", Int()) ], Unit()) }

        TypeDefn("INumericDotNet") { AbstractMember("Add", [ Int(); Int() ], Int(), true) }
    }
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)

(**
## Implementing an Interface
Implement an interface on a class with `InterfaceWith`, supplying the concrete
members:
*)

Oak() {
    AnonymousModule() {
        TypeDefn("SomeClass", ParenPat(TuplePat([ ParameterPat("x", Int()); ParameterPat("y", Float()) ]))) {
            InterfaceWith("IPrintable") {
                Member("this.Print()", AppExpr("printfn", [ String("%d %f"); Constant("x"); Constant("y") ]))
            }
        }
    }
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)
