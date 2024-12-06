(**
---
title: Interfaces
category: widgets
index: 11
---
*)

(**
# Interfaces

For details on how the AST node works, please refer to the [Fantomas documentation](https://fsprojects.github.io/fantomas/reference/fantomas-core-syntaxoak-moduleornamespacenode.html).
See also official [documentation](https://learn.microsoft.com/en-us/dotnet/fsharp/language-reference/modules) for a comparison between the two.
*)

#r "../../src/Fabulous.AST/bin/Release/netstandard2.1/publish/Fantomas.Core.dll"
#r "../../src/Fabulous.AST/bin/Release/netstandard2.1/publish/Fabulous.AST.dll"
#r "../../src/Fabulous.AST/bin/Release/netstandard2.1/publish/Fantomas.FCS.dll"

open Fabulous.AST
open type Fabulous.AST.Ast

Oak() {

    AnonymousModule() {
        TypeDefn("ISprintable") { AbstractMember("Print", [ ("format", Int()) ], Unit()) }
        |> _.triviaBefore(SingleLine("Interfaces specify sets of related members that other classes implement."))
    }
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)
