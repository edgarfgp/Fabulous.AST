(**
---
title: Type Extensions and Signatures
category: widgets
index: 22
---
*)

(**
# Type Extensions and Signatures

## Contents
- [Overview](#Overview)
- [Intrinsic Type Extensions](#Intrinsic-Type-Extensions)
- [Optional Type Extensions](#Optional-Type-Extensions)
- [Generic Type Extensions](#Generic-Type-Extensions)
- [Module Abbreviations](#Module-Abbreviations)
- [External Functions](#External-Functions)
- [Signature Files](#Signature-Files)
- [Value Signatures](#Value-Signatures)
- [Member Signatures](#Member-Signatures)

## Overview
Type extensions add members to an existing type with `type X with ...`, module abbreviations give a module a shorter name, and `extern` declarations call native functions.
Signature declarations (`val` and member signatures) describe a module's public surface; they are what you write in a `.fsi` signature file.

## Intrinsic Type Extensions
`Augmentation(name)` creates a `type Name with` block; when it is in the same file and namespace as the type, the members become part of the type itself:
*)

#r "../../src/Fabulous.AST/bin/Release/netstandard2.1/publish/Fantomas.Core.dll"
#r "../../src/Fabulous.AST/bin/Release/netstandard2.1/publish/Fabulous.AST.dll"
#r "../../src/Fabulous.AST/bin/Release/netstandard2.1/publish/Fantomas.FCS.dll"

open Fabulous.AST
open type Fabulous.AST.Ast

Oak() {
    AnonymousModule() {
        Record("Point") {
            Field("X", Float())
            Field("Y", Float())
        }

        Augmentation("Point") {
            Member("this.Length", AppExpr("sqrt", ParenExpr(InfixAppExpr("this.X * this.X", "+", "this.Y * this.Y"))))

            Member("Origin", RecordExpr([ RecordFieldExpr("X", Float(0.0)); RecordFieldExpr("Y", Float(0.0)) ]))
                .toStatic()
        }
        |> _.xmlDocs([ "Geometry helpers for Point." ])
    }
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)

(**
## Optional Type Extensions
An extension in a different module from the type is an optional extension; it is visible only where its module is opened, so it is usually placed in an `[<AutoOpen>]` module:
*)

Oak() {
    AnonymousModule() {
        Open("System")

        Module("StringExtensions") {
            Augmentation("String") {
                Member("this.Shout", UnitPat(), "this.ToUpperInvariant() + \"!\"")

                Member("IsBlank", ParenPat(ParameterPat("s", String())), AppExpr("String.IsNullOrWhiteSpace", "s"))
                    .toStatic()
            }
        }
        |> _.attribute(Attribute("AutoOpen"))
    }
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)

(**
The extended type can also be given by its full name, such as `Augmentation("System.String")`, which generates `type System.String with`.

## Generic Type Extensions
Use `typeParams` to extend a generic type:
*)

Oak() {
    AnonymousModule() {
        Augmentation("List") {
            Member(
                "this.SecondOrNone",
                MatchExpr("this", [ MatchClauseExpr("_ :: x :: _", "Some x"); MatchClauseExpr("_", "None") ])
            )
        }
        |> _.typeParams(PostfixList("'T"))
    }
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)

(**
## Module Abbreviations
`ModuleAbbrev(alias, moduleName)` creates `module Alias = Full.Module.Name`:
*)

Oak() {
    AnonymousModule() {
        ModuleAbbrev("L", "Microsoft.FSharp.Collections.List")

        Value(
            "doubled",
            AppExpr("L.map", [ ParenExpr(Constant("fun x -> x * 2")); ListExpr([ Int(1); Int(2); Int(3) ]) ])
        )
    }
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)

(**
## External Functions
`ExternBinding(returnType, name, parameters)` creates an `extern` declaration for a native function; each parameter is an `ExternBindingPat(type, name)`, and the `DllImport` attribute is added with `attribute`:
*)

Oak() {
    AnonymousModule() {
        Open("System.Runtime.InteropServices")

        ExternBinding(
            "int",
            "MessageBox",
            [ ExternBindingPat("nativeint", "hWnd")
              ExternBindingPat("string", "text")
              ExternBindingPat("string", "caption")
              ExternBindingPat("uint32", "options") ]
        )
            .attribute(
                Attribute(
                    "DllImport",
                    ParenExpr(
                        TupleExpr(
                            [ ConstantExpr(String("user32.dll"))
                              InfixAppExpr("CharSet", "=", "CharSet.Unicode") ]
                        )
                    )
                )
            )

        ExternBinding("uint32", "GetCurrentProcessId")
            .attribute(Attribute("DllImport", ParenExpr(String("kernel32.dll"))))
            .xmlDocs([ "Returns the id of the calling process." ])
    }
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)

(**
Parameters accept attributes too, for example `ExternBindingPat("nativeint", "handle").attribute(Attribute("In"))` produces `[<In>] nativeint handle`.
`ExternBindingPat(type)` without a name gives a parameter that only states its type (`extern int add(int, int)`).
To declare the function inside a class, yield the `ExternBinding` in the class body and call `toStatic()`, which produces `static extern ...`.
Attributes on the return type go between `extern` and the type; add them with `returnAttribute` or `returnAttributes`, for example `.returnAttribute(Attribute("MarshalAs", ParenExpr(Constant("UnmanagedType.Bool"))))` produces `extern [<MarshalAs(UnmanagedType.Bool)>] bool ...`.
A `[<return: ...>]` attribute before the declaration works too; see [Attribute Targets](AttributesAndComments.html#Attribute-Targets).

## Signature Files
A signature file (`.fsi`) sits next to an implementation file (`.fs`) and declares which values, functions and types the implementation exposes; anything left out becomes private to the file.
Generating one is useful when you also generate the implementation, or when you want to keep a library's public API in one reviewable place.
`Gen.run` prints whatever declarations the `Oak` contains, so a signature file is an `Oak` built from signature declarations (`Val`, `SigMember`, abstract members and type definitions), written to a `.fsi` file.

## Value Signatures
`Val(name, type)` creates a `val` declaration; it supports `xmlDocs`, `attributes`, `typeParams`, `toMutable`, `toInlined` and the access modifiers.
A literal also gives its value with `literalValue` (`[<Literal>] val MaxSize: int = 100`):
*)

Oak() {
    Namespace("MyLib.Math") {
        Val("pi", Float())

        Val("add", Funs([ "int"; "int" ], "int"))
            .xmlDocs(
                Summary("Adds two integers.").parameters([ ("a", "The first value."); ("b", "The second value.") ])
            )

        Val("tryParse", Funs([ "string" ], "int option"))

        Val("identity", Funs([ "'T" ], "'T")).typeParams(PostfixList("'T"))

        Val("MaxSize", Int()).attribute(Attribute("Literal")).literalValue(Int(100))
    }
    |> _.toImplicit()
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)

(**
## Member Signatures
`SigMember(Val(keywords, name, type))` creates a member signature inside a type; the keyword list (`"member"`, `[ "static"; "member" ]`, `"new"`, ...) is printed before the name, and the optional `hasGetter`/`hasSetter` arguments add `with get, set`:
*)

Oak() {
    Namespace("MyLib") {
        TypeDefn("Counter") {
            SigMember(Val("new", "", Funs([ "int" ], "Counter")))
            SigMember(Val("member", "Increment", Funs([ "unit" ], "unit")))
            SigMember(Val("member", "Count", Int()))
            SigMember(Val("member", "Step", Int()), true, true)
            SigMember(Val([ "static"; "member" ], "Zero", LongIdent("Counter")))
        }

        Module("Counter") { Val("create", Funs([ "int" ], "Counter")) }
    }
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)

(**
Abstract members in interface and class signatures use `AbstractMember`, as shown on the [Interfaces](Interfaces.html) page.
*)
