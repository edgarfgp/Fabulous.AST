(**
---
title: Namespaces and Modules
category: widgets
index: 2
---
*)

(**
# Namespaces

A namespace lets you organize code into areas of related functionality by enabling you to attach a name to a grouping of F# program elements. Namespaces are typically top-level elements in F# files.

*)

(**
## Constructors
| Constructors                                      | Description |
|---------------------------------------------------|-------------|
| Namespace(name: string,) | Creates a namespace with the specified name. |
| TopLevelModule(name: string,) | Creates a top-level module with the specified name. |
| AnonymousModule() | Creates an anonymous module. |

## Properties
| Properties            | Description |
|-----------------------|-------------|
| toRecursive() | Makes the namespace or module recursive. |
| toPrivate() | Makes the namespace or module private. |
| toInternal() | Makes the namespace or module internal. |
| toPublic() | Makes the namespace or module public. |

*)

(**

## Usage

*)

#r "../../src/Fabulous.AST/bin/Release/netstandard2.1/publish/Fantomas.Core.dll"
#r "../../src/Fabulous.AST/bin/Release/netstandard2.1/publish/Fabulous.AST.dll"
#r "../../src/Fabulous.AST/bin/Release/netstandard2.1/publish/Fantomas.Core.dll"
#r "../../src/Fabulous.AST/bin/Release/netstandard2.1/publish/Fantomas.FCS.dll"

open Fabulous.AST
open Fantomas.Core
open type Fabulous.AST.Ast

Oak() {
    Namespace("MyNamespace") { Record("Person") { Field("name", "string") } }
    (*
    namespace MyNamespace

    type Person = { name: string }
    *)

    TopLevelModule("MyModule") { Record("Person") { Field("name", "string") } }

    (*
    module MyModule

    // type Person = { name: string }
    *)
    AnonymousModule() { Record("Person") { Field("name", "string") } }

    // type Person = { name: string }

    Namespace("MyNamespace") { NestedModule("MyModule") { Record("Person") { Field("name", "string") } } }

(*
    namespace MyNamespace

    module MyModule =
        type Person = { name: string }
    *)
}
|> Gen.mkOak
|> CodeFormatter.FormatOakAsync
|> Async.RunSynchronously
|> printfn "%s"
