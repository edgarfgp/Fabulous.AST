(**
---
title: JSON Extension
category: extensions
index: 1
---
*)

(**
# JSON Type Generator

Generate F# types from JSON samples using the `Json` widget provided by Fabulous.AST.Json.

This extension parses JSON strings and emits record and alias declarations that describe the shape of the data.

## Installation

```bash
dotnet add package Fabulous.AST.Json
```

> **💡 Tip:** For automatic build-time generation, see the [Build Extension](Build.html) tutorial.
*)

#r "../../src/Fabulous.AST/bin/Release/netstandard2.1/publish/Fantomas.Core.dll"
#r "../../src/Fabulous.AST/bin/Release/netstandard2.1/publish/Fantomas.FCS.dll"
#r "../../src/Fabulous.AST/bin/Release/netstandard2.1/publish/Fabulous.AST.dll"
#r "../../extensions/Fabulous.AST.Json/bin/Release/net8.0//Fabulous.AST.Json.dll"

open Fabulous.AST
open Fabulous.AST.Json
open type Fabulous.AST.Ast

(**
## Step 1: Basic Type Generation

The simplest use case - generate a record from a JSON object:
*)

Oak() {
    AnonymousModule() {
        let json =
            """
{
    "name": "Alice",
    "age": 30,
    "active": true
}
"""

        Json(json)
    }
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces:
(*** include-output ***)

(**
## Step 2: Custom Root Type Name

Override the default `Root` name with `.rootName()`:
*)

Oak() {
    AnonymousModule() {
        let json = """{ "id": 1, "email": "alice@example.com" }"""
        Json(json).rootName("User")
    }
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces:
(*** include-output ***)

(**
## Step 3: Nested Objects

Nested JSON objects become nested record types:
*)

Oak() {
    AnonymousModule() {
        let json =
            """
{
    "user": {
        "name": "Alice",
        "address": {
            "city": "London",
            "country": "UK"
        }
    }
}
"""

        Json(json).rootName("Response")
    }
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces:
(*** include-output ***)

(**
## Step 4: Arrays

Arrays generate an element type and a list alias:
*)

Oak() {
    AnonymousModule() {
        let json =
            """
[
    { "id": 1, "name": "Item 1" },
    { "id": 2, "name": "Item 2" }
]
"""

        Json(json).rootName("Products")
    }
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces:
(*** include-output ***)

(**
## Step 5: Optional Fields

Fields missing or null in some array elements become `option` types:
*)

Oak() {
    AnonymousModule() {
        let json =
            """
[
    { "id": 1, "name": "Alice", "nickname": "Ali" },
    { "id": 2, "name": "Bob" }
]
"""

        Json(json).rootName("Users")
    }
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces:
(*** include-output ***)

(**
## Step 6: Relaxed JSON Parsing

Handle JSON with comments and trailing commas:
*)

let jsonWithComments =
    """
{
  // a comment
  "id": 1,
  "name": "Test",
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

(**
## Step 7: Case-Insensitive Property Names

Merge properties with inconsistent casing:
*)

Oak() {
    AnonymousModule() {
        let json =
            """
[
    { "ID": 1, "Name": "First" },
    { "id": 2, "name": "Second" }
]
"""

        Json(json).nodePropertyNameCaseInsensitive(true)
    }
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces:
(*** include-output ***)

(**
## Step 8: Namespaces and Modules

Wrap generated types in a namespace:
*)

Oak() {
    Namespace("MyApp.Models") {
        let json = """{ "id": 1, "name": "Alice" }"""
        Json(json).rootName("User")
    }
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces:
(*** include-output ***)

(**
Or in a module:
*)

Oak() {
    Module("MyApp.Models") {
        let json = """{ "id": 1, "name": "Alice" }"""
        Json(json).rootName("User")
    }
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces:
(*** include-output ***)

(**
## Type Inference Rules

| JSON Value | F# Type |
|------------|---------|
| `"string"` | `string` |
| `123` | `int` |
| `9999999999` | `int64` |
| `123.45` | `float` |
| `true`/`false` | `bool` |
| `null` | `obj` |
| `[...]` | `ElementType list` |
| `{...}` | Record type |

## Special Field Name Handling

### Leading Digits

Fields starting with digits are prefixed with `_`:
*)

Oak() {
    AnonymousModule() {
        let json = """{ "2faEnabled": true, "3rdPartyId": "abc" }"""
        Json(json)
    }
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces:
(*** include-output ***)

(**
### Reserved Keywords

F# keywords are escaped with double backticks:
*)

Oak() {
    AnonymousModule() {
        let json = """{ "type": "admin", "class": "premium" }"""
        Json(json)
    }
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces:
(*** include-output ***)

(**
## API Reference

| Modifier | Description |
|----------|-------------|
| `.rootName(string)` | Set root type name (default: `"Root"`) |
| `.documentOptions(JsonDocumentOptions)` | Provide parsing options |
| `.documentAllowTrailingCommas(bool)` | Allow trailing commas |
| `.documentCommentHandling(JsonCommentHandling)` | Handle JSON comments |
| `.documentMaxDepth(int)` | Maximum nesting depth |
| `.serializerOptions(JsonSerializerOptions)` | Seed options from serializer config |
| `.nodePropertyNameCaseInsensitive(bool)` | Case-insensitive property matching |

## Next Steps

- Learn about [automatic build-time generation](Build.html) with Fabulous.AST.Build
- Explore the [Fabulous.AST DSL](../widgets/Oak.html) for more code generation options
*)
