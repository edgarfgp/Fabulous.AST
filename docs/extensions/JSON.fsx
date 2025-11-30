(**
---
title: JSON Extension
category: extensions
index: 1
---
*)

(**
# JSON

Generate F# types from a JSON sample using the Json widget provided by the Fabulous.AST.Json extension.

The widget parses a JSON string and emits record and alias declarations that describe the shape of the data.
*)

#r "../../src/Fabulous.AST/bin/Release/netstandard2.1/publish/Fantomas.Core.dll"
#r "../../src/Fabulous.AST/bin/Release/netstandard2.1/publish/Fantomas.FCS.dll"
#r "../../src/Fabulous.AST/bin/Release/netstandard2.1/publish/Fabulous.AST.dll"
#r "../../extensions/Fabulous.AST.Json/bin/Release/net8.0//Fabulous.AST.Json.dll"

open Fabulous.AST
open Fabulous.AST.Json
open type Fabulous.AST.Ast

(**
## Basic usage

Pass a JSON string to Json and render the generated F# code with Gen.run:
*)

Oak() { AnonymousModule() { Json("{ \"name\": \"Alice\", \"age\": 30, \"active\": true }") } }
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces:
(*** include-output ***)

(**
## Common options

- rootName: override the root type name (default is "Root").
- documentAllowTrailingCommas / documentCommentHandling: enable relaxed parsing.
- nodePropertyNameCaseInsensitive: compare property names case-insensitively.
*)

(** Root name **)
Oak() { AnonymousModule() { Json("{\"id\":1}").rootName("Person") } }
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces:
(*** include-output ***)

(** Allow comments and trailing commas **)
let jsonWithComments =
    """
{
  // a comment
  "id": 1,
}
"""

Oak() {
    AnonymousModule() {
        Json(jsonWithComments)
            .documentAllowTrailingCommas(true)
            .documentCommentHandling(System.Text.Json.JsonCommentHandling.Skip)
    }
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces:
(*** include-output ***)

(** Case-insensitive property names **)
Oak() { AnonymousModule() { Json("[ { \"ID\": 1 }, { \"id\": 2 } ]").nodePropertyNameCaseInsensitive(true) } }
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces:
(*** include-output ***)
