# Fabulous.AST.Json

A code generation library that transforms JSON samples into F# type definitions using the Fabulous.AST DSL.

## What is it?

Fabulous.AST.Json is a **programmatic F# type generator** that:

- Takes a JSON sample as input
- Infers F# record types, nested types, and type aliases
- Outputs clean, Fantomas-formatted F# code

**It is NOT:**
- A runtime JSON deserializer (use `System.Text.Json` or `Newtonsoft.Json` for that)
- A Type Provider (types are generated as source code, not at compile-time)

**Use cases:**
- Generating F# types from API response samples
- Creating type-safe models from JSON schemas
- Test fixtures and documentation
- Custom code generation tooling

> **💡 Tip:** If you want automatic build-time generation from JSON files, use [Fabulous.AST.Json.Build](https://www.nuget.org/packages/Fabulous.AST.Json.Build) instead.

## Installation

```bash
dotnet add package Fabulous.AST.Json
```

**Requirements:** .NET 8.0 or later

## How to use

### Basic example

Generate a record type from a JSON object:

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
```

**Output:**
```fsharp
type Root = { name: string; age: int; active: bool }
```

### Arrays and nested objects

Arrays generate an element type and a list alias:

```fsharp
let json = """[{ "id": 1, "name": "Item 1" }, { "id": 2, "name": "Item 2" }]"""

let source =
    Oak() { AnonymousModule() { Json(json) } }
    |> Gen.mkOak
    |> Gen.run
```

**Output:**
```fsharp
type RootItem = { id: int; name: string }
type Root = RootItem list
```

Nested objects become nested record types:

```fsharp
let json = """{ "user": { "name": "Alice", "address": { "city": "London" } } }"""

let source =
    Oak() { AnonymousModule() { Json(json) } }
    |> Gen.mkOak
    |> Gen.run
```

**Output:**
```fsharp
type Address = { city: string }
type User = { name: string; address: Address }
type Root = { user: User }
```

### Customization with modifiers

Customize the root type name and JSON parsing options:

```fsharp
open System.Text.Json

let options = JsonSerializerOptions(
    AllowTrailingCommas = true,
    ReadCommentHandling = JsonCommentHandling.Skip
)

let source =
    Oak() {
        AnonymousModule() {
            Json("""{ "id": 1, "email": "alice@example.com" }""")
                .rootName("User")
                .serializerOptions(options)
        }
    }
    |> Gen.mkOak
    |> Gen.run
```

**Output:**
```fsharp
type User = { id: int; email: string }
```

### Available modifiers

| Modifier | Description |
|----------|-------------|
| `.rootName(string)` | Set the name of the root type (default: `"Root"`) |
| `.documentOptions(JsonDocumentOptions)` | Provide `JsonDocumentOptions` used for parsing |
| `.documentAllowTrailingCommas(bool)` | Allow trailing commas in JSON |
| `.documentCommentHandling(JsonCommentHandling)` | Handle JSON comments |
| `.documentMaxDepth(int)` | Maximum nesting depth |
| `.serializerOptions(JsonSerializerOptions)` | Seed related document options from a serializer config |
| `.nodePropertyNameCaseInsensitive(bool)` | Case-insensitive node property matching |

## Type inference rules

| JSON value | F# type |
|------------|---------|
| `"string"` | `string` |
| `123` | `int` |
| `123.45` | `float` |
| `true`/`false` | `bool` |
| `null` | `string` |
| `[...]` | `ElementType list` |
| `{...}` | Record type |

## Related packages

- **[Fabulous.AST](https://www.nuget.org/packages/Fabulous.AST)** - Core DSL for generating F# code
- **[Fabulous.AST.Json.Build](https://www.nuget.org/packages/Fabulous.AST.Json.Build)** - MSBuild task for automatic build-time generation
