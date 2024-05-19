﻿(**
---
title: Organizing Code
category: widgets
index: 2
---
*)

(**
# Namespaces

Both namespaces and modules are used for organizing the code in a hierarchical, logical way.

*)

(**
## Constructors
| Constructors                                      | Description |
|---------------------------------------------------|-------------|
| Namespace(name: string) | Creates a namespace with the specified name. |
| TopLevelModule(name: string) | Creates a top-level module with the specified name. |
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
    TopLevelModule("MyModule") { Record("Person1") { Field("name", "string") } }
    AnonymousModule() { Record("Person2") { Field("name", "string") } }
    Namespace("MyNamespace") { NestedModule("MyModule") { Record("Person") { Field("name", "string") } } }
}
|> Gen.mkOak
|> CodeFormatter.FormatOakAsync
|> Async.RunSynchronously
|> printfn "%s"

(**
Will output the following code:
*)

(**

namespace MyNamespace

type Person = { name: string }
module MyModule

type Person1 = { name: string }
type Person2 = { name: string }
namespace MyNamespace

module MyModule =
    type Person = { name: string }

*)
