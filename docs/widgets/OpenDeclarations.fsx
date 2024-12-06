(**
---
title: OpenDeclarations
category: widgets
index: 10
---
*)

(**
# Open declarations
An import declaration specifies a module or namespace whose elements you can reference without using a fully qualified name.

For details on how the AST node works, please refer to the [Fantomas Core documentation](https://fsprojects.github.io/fantomas/reference/fantomas-core-open.html).

*)
(**
### Constructors

| Constructors                       | Description                                           |
|------------------------------------| ----------------------------------------------------- |
| Open(values: string list)                              | Creates an OpenListNode AST node |
| Open(value: string)                                   | Creates an OpenListNode AST node  |
| OpenType(values: string)                              | Creates an OpenListNode AST node  |
| OpenType(value: string)                               | Creates an OpenListNode AST node  |
*)

#r "../../src/Fabulous.AST/bin/Release/netstandard2.1/publish/Fantomas.Core.dll"
#r "../../src/Fabulous.AST/bin/Release/netstandard2.1/publish/Fabulous.AST.dll"
#r "../../src/Fabulous.AST/bin/Release/netstandard2.1/publish/Fantomas.FCS.dll"

open Fabulous.AST
open type Fabulous.AST.Ast

Oak() {
    AnonymousModule() {
        Open([ "System"; "IO" ])
            .triviaBefore(SingleLine("Open a .NET Framework namespace."))

        Open("Fabulous.AST").triviaAfter(Newline())

        OpenType([ "System.Math" ])
            .triviaBefore(SingleLine("This will expose all accessible static fields and members on the type."))
            .triviaAfter(Newline())

        OpenGlobal("A")
            .triviaBefore(SingleLine("Open from root path only with global specifier"))

        OpenGlobal("B")
        OpenGlobal([ "A"; "B" ])

    }
    |> _.triviaBefore(SingleLine("Import declarations: The open keyword"))

}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)
