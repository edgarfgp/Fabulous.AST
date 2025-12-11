(**
---
title: Build Extension
category: extensions
index: 2
---
*)

(**
# Build-Time Type Generation

Automatically generate F# types from JSON files at build time using Fabulous.AST.Build.

This extension provides an MSBuild task that watches JSON files and generates F# record types during compilation.

## Installation

```bash
dotnet add package Fabulous.AST.Build
```

> **💡 Tip:** For programmatic control over generation, see the [JSON Extension](JSON.html) tutorial.

## Quick Start

### 1. Add a JSON schema file

Create `schemas/user.json`:

```json
{
    "id": 1,
    "name": "Alice",
    "email": "alice@example.com",
    "isActive": true
}
```

### 2. Configure your project

Add to your `.fsproj`:

```xml
<ItemGroup>
  <FabulousAstJson Include="schemas/user.json" />
</ItemGroup>

<ItemGroup>
  <Compile Include="Generated/user.Generated.fs" />
</ItemGroup>
```

### 3. Build

```bash
dotnet build
```

This generates `Generated/user.Generated.fs`:

```fsharp
type Root = {
    id: int
    name: string
    email: string
    isActive: bool
}
```

## Configuration Options

### Custom Root Type Name

Override the default `Root` type name:

```xml
<FabulousAstJson Include="schemas/user.json" RootName="User" />
```

### Add a Namespace

Wrap types in a namespace:

```xml
<FabulousAstJson Include="schemas/user.json"
                 RootName="User"
                 Namespace="MyApp.Models" />
```

Generates:

```fsharp
namespace MyApp.Models

type User = { id: int; name: string; email: string; isActive: bool }
```

### Use a Module

Use `ModuleName` instead of `Namespace`:

```xml
<FabulousAstJson Include="schemas/user.json"
                 RootName="User"
                 ModuleName="MyApp.Models" />
```

Generates:

```fsharp
module MyApp.Models

type User = { id: int; name: string; email: string; isActive: bool }
```

### Custom Output Directory

Change the output location:

```xml
<PropertyGroup>
  <FabulousAstJsonOutputDir>Types/</FabulousAstJsonOutputDir>
</PropertyGroup>
```

### Custom Output Filename

Override the default `{name}.Generated.fs` pattern:

```xml
<FabulousAstJson Include="schemas/user.json" OutputFileName="UserTypes.fs" />
```

## Multiple Files

### Individual Configuration

```xml
<ItemGroup>
  <FabulousAstJson Include="schemas/user.json"
                   RootName="User"
                   Namespace="MyApp.Models" />
  <FabulousAstJson Include="schemas/product.json"
                   RootName="Product"
                   Namespace="MyApp.Models" />
</ItemGroup>

<ItemGroup>
  <Compile Include="Generated/user.Generated.fs" />
  <Compile Include="Generated/product.Generated.fs" />
</ItemGroup>
```

### Glob Patterns

Process all JSON files in a directory:

```xml
<ItemGroup>
  <FabulousAstJson Include="schemas/**/*.json" />
</ItemGroup>
```

## Type Inference Examples

### Nested Objects

**Input (`company.json`):**
```json
{
    "name": "Acme Corp",
    "address": {
        "street": "123 Main St",
        "city": "London"
    }
}
```

**Output:**
```fsharp
type Address = { street: string; city: string }
type Company = { name: string; address: Address }
```

### Arrays

**Input (`users.json`):**
```json
[
    { "id": 1, "name": "Alice" },
    { "id": 2, "name": "Bob" }
]
```

**Output:**
```fsharp
type UsersItem = { id: int; name: string }
type Users = UsersItem list
```

### Optional Fields

Fields missing in some array objects become `option` types:

**Input:**
```json
[
    { "id": 1, "name": "Alice", "nickname": "Ali" },
    { "id": 2, "name": "Bob" }
]
```

**Output:**
```fsharp
type RootItem = { id: int; name: string; nickname: string option }
type Root = RootItem list
```

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

```json
{ "2faEnabled": true }
```

Generates: `_2faEnabled: bool`

### Reserved Keywords

F# keywords are escaped with double backticks:

```json
{ "type": "admin" }
```

Generates: ``` ``type``: string ```

## Configuration Reference

### Project Properties

| Property | Default | Description |
|----------|---------|-------------|
| `FabulousAstJsonOutputDir` | `Generated/` | Output directory |
| `EnableFabulousAstJsonGeneration` | `true` | Enable/disable generation |

### Item Metadata

| Metadata | Default | Description |
|----------|---------|-------------|
| `RootName` | `Root` | Root type name |
| `Namespace` | _(empty)_ | Namespace wrapper |
| `ModuleName` | _(empty)_ | Module wrapper |
| `OutputFileName` | `{InputName}.Generated.fs` | Output filename |

## Incremental Builds

The task uses content hashing for efficient builds:

- Files regenerate only when JSON content changes
- Configuration changes trigger regeneration
- Generated files include hash comments for verification

## Troubleshooting

### File Not Updating

```bash
dotnet clean && dotnet build
```

### Disable Generation

Via project property:
```xml
<EnableFabulousAstJsonGeneration>false</EnableFabulousAstJsonGeneration>
```

Via command line:
```bash
dotnet build -p:EnableFabulousAstJsonGeneration=false
```

## Next Steps

- Learn about [programmatic generation](JSON.html) with Fabulous.AST.Json
- Explore the [Fabulous.AST DSL](../widgets/Oak.html) for more code generation options
*)
