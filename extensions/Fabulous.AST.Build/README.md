# Fabulous.AST.Build

Automatically generate F# types from JSON files at build time.

## Overview

Fabulous.AST.Build is an **MSBuild task** that:

- Watches JSON files in your project
- Automatically generates F# record types during build
- Supports incremental builds (only regenerates when JSON changes)
- Integrates seamlessly with MSBuild and .NET CLI

**Use cases:**
- Generate F# types from API response samples
- Keep type definitions in sync with JSON schemas
- Automate type generation in CI/CD pipelines

> **💡 Tip:** For programmatic control over generation, use [Fabulous.AST.Json](../Fabulous.AST.Json/README.md) directly.

## Installation

```bash
dotnet add package Fabulous.AST.Build
```

**Requirements:** .NET 8.0 or later

## Tutorial

### Step 1: Create a JSON Schema File

Create a folder for your JSON schemas and add a sample file.

**`schemas/user.json`:**
```json
{
    "id": 1,
    "name": "Alice",
    "email": "alice@example.com",
    "isActive": true
}
```

### Step 2: Configure Your Project

Add the `FabulousAstJson` item to your `.fsproj`:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Fabulous.AST.Build" Version="1.0.0" />
  </ItemGroup>

  <!-- Configure JSON file for type generation -->
  <ItemGroup>
    <FabulousAstJson Include="schemas/user.json" />
  </ItemGroup>

  <!-- Include the generated file in compilation -->
  <ItemGroup>
    <Compile Include="Generated/user.Generated.fs" />
  </ItemGroup>
</Project>
```

### Step 3: Build Your Project

```bash
dotnet build
```

This generates `Generated/user.Generated.fs`:

```fsharp
// Auto-generated from schemas/user.json
// Hash: abc123...
// Do not edit manually - changes will be overwritten

type User = {
    id: int
    name: string
    email: string
    isActive: bool
}
```

### Step 4: Custom Root Type Name

By default, the root type name is derived from the filename. Override it with `RootName`:

```xml
<ItemGroup>
  <FabulousAstJson Include="schemas/api-response.json" RootName="ApiResponse" />
</ItemGroup>
```

### Step 5: Add a Namespace

Wrap generated types in a namespace:

```xml
<ItemGroup>
  <FabulousAstJson Include="schemas/user.json"
                   RootName="User"
                   Namespace="MyApp.Models" />
</ItemGroup>
```

**Output:**
```fsharp
namespace MyApp.Models

type User = {
    id: int
    name: string
    email: string
    isActive: bool
}
```

### Step 6: Use a Module Instead

Use `ModuleName` for a module wrapper:

```xml
<ItemGroup>
  <FabulousAstJson Include="schemas/user.json"
                   RootName="User"
                   ModuleName="MyApp.Models" />
</ItemGroup>
```

**Output:**
```fsharp
module MyApp.Models

type User = {
    id: int
    name: string
    email: string
    isActive: bool
}
```

### Step 7: Multiple JSON Files

Process multiple schemas with different configurations:

```xml
<ItemGroup>
  <FabulousAstJson Include="schemas/user.json"
                   RootName="User"
                   Namespace="MyApp.Models" />
  <FabulousAstJson Include="schemas/product.json"
                   RootName="Product"
                   Namespace="MyApp.Models" />
  <FabulousAstJson Include="schemas/order.json"
                   RootName="CustomerOrder"
                   Namespace="MyApp.Orders" />
</ItemGroup>

<ItemGroup>
  <Compile Include="Generated/user.Generated.fs" />
  <Compile Include="Generated/product.Generated.fs" />
  <Compile Include="Generated/order.Generated.fs" />
</ItemGroup>
```

### Step 8: Custom Output Directory

Change where generated files are placed:

```xml
<PropertyGroup>
  <FabulousAstJsonOutputDir>Types/</FabulousAstJsonOutputDir>
</PropertyGroup>
```

### Step 9: Glob Patterns

Process all JSON files in a directory:

```xml
<ItemGroup>
  <FabulousAstJson Include="schemas/**/*.json" />
</ItemGroup>
```

### Step 10: Custom Output Filename

Override the default `{filename}.Generated.fs` pattern:

```xml
<ItemGroup>
  <FabulousAstJson Include="schemas/user.json"
                   OutputFileName="UserTypes.fs" />
</ItemGroup>
```

## Configuration Reference

### Project Properties

| Property | Default | Description |
|----------|---------|-------------|
| `FabulousAstJsonOutputDir` | `$(MSBuildProjectDirectory)\Generated\` | Output directory for generated files |
| `EnableFabulousAstJsonGeneration` | `true` | Enable/disable generation (useful for CI) |

### Item Metadata

| Metadata | Default | Description |
|----------|---------|-------------|
| `RootName` | `Root` | Name of the root type |
| `Namespace` | _(empty)_ | Namespace to wrap types in |
| `ModuleName` | _(empty)_ | Module to wrap types in (alternative to Namespace) |
| `OutputFileName` | `{InputName}.Generated.fs` | Custom output filename |

## Type Inference

The generator infers F# types from JSON values:

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

### Arrays Become List Types

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

### Nested Objects Become Nested Types

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

### Optional Fields

Fields missing or null in some array objects become `option` types:

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

## Special Field Name Handling

### Leading Digits

JSON fields starting with digits are prefixed with `_`:

```json
{ "2faEnabled": true }
```

Generates:
```fsharp
type Root = { _2faEnabled: bool }
```

### Reserved Keywords

F# keywords are escaped with double backticks:

```json
{ "type": "admin", "class": "premium" }
```

Generates:
```fsharp
type Root = { ``type``: string; ``class``: string }
```

## Incremental Builds

The task uses content hashing for smart rebuilds:

- Files are only regenerated when JSON content changes
- Configuration changes (RootName, Namespace, etc.) trigger regeneration
- Generated files include a hash comment for verification

## Troubleshooting

### Generated File Not Updating

1. Ensure the file is included as `FabulousAstJson`:
   ```xml
   <FabulousAstJson Include="path/to/file.json" />
   ```

2. Ensure the generated file is in `Compile`:
   ```xml
   <Compile Include="Generated/file.Generated.fs" />
   ```

3. Try a clean rebuild:
   ```bash
   dotnet clean && dotnet build
   ```

### Task Assembly Not Found

If you see "Skipping generation because task assembly not found":

1. Ensure you've restored packages: `dotnet restore`
2. For local development, build Fabulous.AST.Build first

### Disable Generation

Disable generation for CI or specific builds:

```xml
<PropertyGroup>
  <EnableFabulousAstJsonGeneration>false</EnableFabulousAstJsonGeneration>
</PropertyGroup>
```

Or via command line:
```bash
dotnet build -p:EnableFabulousAstJsonGeneration=false
```

## Related Packages

- **[Fabulous.AST](https://www.nuget.org/packages/Fabulous.AST)** - Core DSL for generating F# code
- **[Fabulous.AST.Json](https://www.nuget.org/packages/Fabulous.AST.Json)** - Programmatic API for JSON-to-F# generation
