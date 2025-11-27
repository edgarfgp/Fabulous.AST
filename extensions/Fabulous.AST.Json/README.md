# Fabulous.AST.Json

Fabulous.AST.Json is an extension for Fabulous.AST that turns a JSON sample into F# type declarations using the Fantomas Oak-based AST DSL. It infers record types (and aliases) from your JSON and emits clean, formatted F# code you can snapshot-test or paste into your projects.

It is not a runtime deserializer nor a Type Provider. It’s a small, deterministic generator that lives inside the Fabulous.AST DSL, ideal for tests, docs, and ad‑hoc code generation.


Installation

- dotnet add package Fabulous.AST.Json

The Fabulous.AST.Json package targets .NET 8 for the extension assembly and depends on the core Fabulous.AST library (netstandard2.1). See global.json for the SDK used in this repo.


How to use
Basic example generating a single record type from a JSON object:

```fsharp
open Fabulous.AST
open Fabulous.AST.Json
open type Ast

let json = """{ "name": "Alice", "age": 30, "active": true }"""

let source =
    Oak() {
        AnonymousModule() {
            Json(json)
        }
    }
    |> Gen.mkOak
    |> Gen.run

printfn "%s" source
// Produces:
//
// type Root = { name: string; age: int; active: bool }
```

Arrays and nested objects are handled as you’d expect. For a root array, the generator creates an element record and a list alias:

```fsharp
let source =
    Oak() { AnonymousModule() { Json("""[ { "id": 1 } ]""") } }
    |> Gen.mkOak
    |> Gen.run

// type RootItem = { id: int }
// type Root = RootItem list
```

You can customize behavior with modifiers:

```fsharp
open System.Text.Json

let permissive = JsonSerializerOptions(
    AllowTrailingCommas = true,
    ReadCommentHandling = JsonCommentHandling.Skip,
    PropertyNameCaseInsensitive = true
)

let source =
    Oak() {
        AnonymousModule() {
            Json("""
            {
              // comment allowed
              "id": 1,
            }
            """)
                .rootName("Person")
                .serializerOptions(permissive)
                // granular overrides if desired:
                .allowTrailingCommas(true)
                .readCommentHandling(JsonCommentHandling.Skip)
                .nodePropertyNameCaseInsensitive(true)
        }
    }
    |> Gen.mkOak
    |> Gen.run

// type Person = { id: int }
```

Available modifiers (see XML docs in code for details and precedence):
- .rootName(string)
- .serializerOptions(JsonSerializerOptions)
- .nodeOptions(JsonNodeOptions)
- .allowTrailingCommas(bool)
- .readCommentHandling(JsonCommentHandling)
- .maxDepth(int)
- .serializerPropertyNameCaseInsensitive(bool)
- .nodePropertyNameCaseInsensitive(bool)
