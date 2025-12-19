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
    <PackageReference Include="Fabulous.AST.Build" Version="2.0.0" />
  </ItemGroup>

  <!-- Configure JSON file for type generation -->
  <ItemGroup>
    <FabulousAstJson Include="schemas/user.json" />
  </ItemGroup>
</Project>
```

> **Note:** Generated files are automatically included in compilation. No manual `<Compile Include="...">` is needed.

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

### Step 5: Add a Module

Wrap generated types in a file-level module using `ModuleName`:

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

### Step 6: Multiple JSON Files

Process multiple schemas with different configurations:

```xml
<ItemGroup>
  <FabulousAstJson Include="schemas/user.json"
                   RootName="User"
                   ModuleName="MyApp.Models.User" />
  <FabulousAstJson Include="schemas/product.json"
                   RootName="Product"
                   ModuleName="MyApp.Models.Product" />
  <FabulousAstJson Include="schemas/order.json"
                   RootName="CustomerOrder"
                   ModuleName="MyApp.Orders" />
</ItemGroup>
```

> **Note:** Each file must have a unique module name since file-level modules cannot share the same fully-qualified name.

### Step 7: Custom Output Directory

Change where generated files are placed:

```xml
<PropertyGroup>
  <FabulousAstOutputFolder>Types</FabulousAstOutputFolder>
</PropertyGroup>
```

### Step 8: Glob Patterns

Process all JSON files in a directory:

```xml
<ItemGroup>
  <FabulousAstJson Include="schemas/**/*.json" />
</ItemGroup>
```

### Step 9: Custom Output Filename

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
| `FabulousAstOutputFolder` | `Generated` | Output folder for generated files (relative to project directory) |
| `EnableFabulousAstJson` | `true` | Enable/disable generation (useful for CI) |

### Item Metadata

| Metadata | Default | Description |
|----------|---------|-------------|
| `RootName` | `Root` | Name of the root type |
| `ModuleName` | _(empty)_ | File-level module name (e.g., `MyApp.Models`) |
| `OutputFileName` | `{InputName}.g.fs` | Custom output filename |

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
- Configuration changes (RootName, ModuleName, etc.) trigger regeneration
- Generated files include a hash comment for verification

## IDE Integration

Generated files are automatically included in compilation and placed before your source files, ensuring proper compile order.

### IDE Recognition

Since generated files are added at project load time, most IDEs will recognize them after the first build. If your IDE doesn't see the generated types:

1. Build the project once (`dotnet build`)
2. Reload/refresh the project in your IDE

### Tips

- **Keep generated files in source control** - Avoids first-build issues and ensures CI builds work immediately
- **Don't delete generated files** - Incremental build only regenerates when JSON changes
- **Multi-targeting works automatically** - Generation runs once before all target framework builds

## Troubleshooting

### IDE Not Seeing Generated Files

1. Build the project: `dotnet build`
2. Reload/refresh the project in your IDE
3. Ensure the file exists in the `Generated/` folder

### Generated File Not Updating

```bash
dotnet clean && dotnet build
```

### Disable Generation

```xml
<EnableFabulousAstJson>false</EnableFabulousAstJson>
```

Or via command line:
```bash
dotnet build -p:EnableFabulousAstJson=false
```

## Related Packages

- **[Fabulous.AST](https://www.nuget.org/packages/Fabulous.AST)** - Core DSL for generating F# code
- **[Fabulous.AST.Json](https://www.nuget.org/packages/Fabulous.AST.Json)** - Programmatic API for JSON-to-F# generation
