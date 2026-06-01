(**
---
title: OpenDeclarations
category: widgets
index: 10
---
*)

(**
# Open Declarations

An open declaration lets you reference the elements of a module or namespace
without a fully qualified name. `Open` imports a namespace or module, `OpenType`
imports a type's static members, and `OpenGlobal` opens from the root with the
`global` specifier.

## Contents
- [Opening Namespaces and Modules](#opening-namespaces-and-modules)
- [Opening a Type](#opening-a-type)
- [Global Opens](#global-opens)
*)

#r "../../src/Fabulous.AST/bin/Release/netstandard2.1/publish/Fantomas.Core.dll"
#r "../../src/Fabulous.AST/bin/Release/netstandard2.1/publish/Fabulous.AST.dll"
#r "../../src/Fabulous.AST/bin/Release/netstandard2.1/publish/Fantomas.FCS.dll"

open Fabulous.AST
open type Fabulous.AST.Ast

(**
## Opening Namespaces and Modules
Pass a single name or a list of path segments:
*)

Oak() {
    AnonymousModule() {
        Open("Fabulous.AST")

        Open([ "System"; "IO" ])
    }
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)

(**
## Opening a Type
`OpenType` exposes the accessible static members and fields of a type:
*)

Oak() { AnonymousModule() { OpenType([ "System.Math" ]) } }
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)

(**
## Global Opens
`OpenGlobal` opens from the root path only, emitting the `global` specifier:
*)

Oak() {
    AnonymousModule() {
        OpenGlobal("A")

        OpenGlobal([ "A"; "B" ])
    }
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)
