# Fabulous.AST.Json

Generate F# types from JSON samples using the Fabulous.AST DSL.

## Overview

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

> **💡 Tip:** For automatic build-time generation from JSON files, use [Fabulous.AST.Build](../Fabulous.AST.Build/README.md) instead.

## Installation

```bash
dotnet add package Fabulous.AST.Json
```

**Requirements:** .NET 8.0 or later

## Tutorial

### Step 1: Basic Type Generation

The simplest use case is generating a record type from a JSON object:

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

### Step 2: Custom Root Type Name

By default, the root type is named `Root`. Use `.rootName()` to customize it:

```fsharp
let json = """{ "id": 1, "email": "alice@example.com" }"""

let source =
    Oak() {
        AnonymousModule() {
            Json(json).rootName("User")
        }
    }
    |> Gen.mkOak
    |> Gen.run
```

**Output:**
```fsharp
type User = { id: int; email: string }
```

### Step 3: Nested Objects

Nested JSON objects become nested record types:

```fsharp
let json = """
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

let source =
    Oak() {
        AnonymousModule() {
            Json(json).rootName("Response")
        }
    }
    |> Gen.mkOak
    |> Gen.run
```

**Output:**
```fsharp
type Address = { city: string; country: string }
type User = { name: string; address: Address }
type Response = { user: User }
```

### Step 4: Arrays

Arrays generate an element type and a list alias:

```fsharp
let json = """
[
    { "id": 1, "name": "Item 1" },
    { "id": 2, "name": "Item 2" }
]
"""

let source =
    Oak() {
        AnonymousModule() {
            Json(json).rootName("Products")
        }
    }
    |> Gen.mkOak
    |> Gen.run
```

**Output:**
```fsharp
type ProductsItem = { id: int; name: string }
type Products = ProductsItem list
```

### Step 5: Optional Fields

When analyzing arrays of objects, fields that are missing or null in some objects become `option` types:

```fsharp
let json = """
[
    { "id": 1, "name": "Alice", "nickname": "Ali" },
    { "id": 2, "name": "Bob" }
]
"""

let source =
    Oak() {
        AnonymousModule() {
            Json(json).rootName("Users")
        }
    }
    |> Gen.mkOak
    |> Gen.run
```

**Output:**
```fsharp
type UsersItem = { id: int; name: string; nickname: string option }
type Users = UsersItem list
```

### Step 6: Relaxed JSON Parsing

Handle JSON with comments and trailing commas:

```fsharp
let json = """
{
    // This is a comment
    "id": 1,
    "name": "Test",  // trailing comma
}
"""

let source =
    Oak() {
        AnonymousModule() {
            Json(json)
                .documentAllowTrailingCommas(true)
                .documentCommentHandling(System.Text.Json.JsonCommentHandling.Skip)
        }
    }
    |> Gen.mkOak
    |> Gen.run
```

### Step 7: Case-Insensitive Property Names

When JSON has inconsistent casing, use case-insensitive matching:

```fsharp
let json = """
[
    { "ID": 1, "Name": "First" },
    { "id": 2, "name": "Second" }
]
"""

let source =
    Oak() {
        AnonymousModule() {
            Json(json).nodePropertyNameCaseInsensitive(true)
        }
    }
    |> Gen.mkOak
    |> Gen.run
```

### Step 8: Adding to a Namespace or Module

Wrap generated types in a namespace:

```fsharp
let json = """{ "id": 1, "name": "Alice" }"""

let source =
    Oak() {
        Namespace("MyApp.Models") {
            Json(json).rootName("User")
        }
    }
    |> Gen.mkOak
    |> Gen.run
```

**Output:**
```fsharp
namespace MyApp.Models

type User = { id: int; name: string }
```

Or in a module:

```fsharp
let source =
    Oak() {
        Module("MyApp.Models") {
            Json(json).rootName("User")
        }
    }
    |> Gen.mkOak
    |> Gen.run
```

**Output:**
```fsharp
module MyApp.Models

type User = { id: int; name: string }
```

## Type Inference Rules

| JSON Value | F# Type |
|------------|---------|
| `"string"` | `string` |
| `123` | `int` |
| `9999999999` (large integer) | `int64` |
| `123.45` | `float` |
| `true` / `false` | `bool` |
| `null` | `obj` |
| `[...]` | `ElementType list` |
| `{...}` | Record type |

## Special Field Name Handling

### Leading Digits

JSON fields starting with digits are prefixed with `_`:

```json
{ "2faEnabled": true, "3rdPartyId": "abc" }
```

Generates:
```fsharp
type Root = { _2faEnabled: bool; _3rdPartyId: string }
```

### F# Reserved Keywords

JSON fields that are F# keywords are escaped with double backticks:

```json
{ "type": "admin", "class": "premium" }
```

Generates:
```fsharp
type Root = { ``type``: string; ``class``: string }
```

## API Reference

### Available Modifiers

| Modifier | Description |
|----------|-------------|
| `.rootName(string)` | Set the name of the root type (default: `"Root"`) |
| `.documentOptions(JsonDocumentOptions)` | Provide `JsonDocumentOptions` for parsing |
| `.documentAllowTrailingCommas(bool)` | Allow trailing commas in JSON |
| `.documentCommentHandling(JsonCommentHandling)` | Handle JSON comments |
| `.documentMaxDepth(int)` | Maximum nesting depth |
| `.serializerOptions(JsonSerializerOptions)` | Seed document options from a serializer config |
| `.nodePropertyNameCaseInsensitive(bool)` | Case-insensitive property matching |

## Related Packages

- **[Fabulous.AST](https://www.nuget.org/packages/Fabulous.AST)** - Core DSL for generating F# code
- **[Fabulous.AST.Build](https://www.nuget.org/packages/Fabulous.AST.Build)** - MSBuild task for automatic build-time generation
