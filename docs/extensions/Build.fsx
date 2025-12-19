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
```

> **Note:** Generated files are automatically included in compilation. No manual `<Compile Include="...">` is needed.

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

### Add a Module

Wrap types in a file-level module:

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

> **Note:** Each file must have a unique module name since file-level modules cannot share the same fully-qualified name.

### Custom Output Directory

Change the output location:

```xml
<PropertyGroup>
  <FabulousAstOutputFolder>Types</FabulousAstOutputFolder>
</PropertyGroup>
```

### Custom Output Filename

Override the default `{name}.g.fs` pattern:

```xml
<FabulousAstJson Include="schemas/user.json" OutputFileName="UserTypes.fs" />
```

## Multiple Files

### Individual Configuration

```xml
<ItemGroup>
  <FabulousAstJson Include="schemas/user.json"
                   RootName="User"
                   ModuleName="MyApp.Models.User" />
  <FabulousAstJson Include="schemas/product.json"
                   RootName="Product"
                   ModuleName="MyApp.Models.Product" />
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

|| Property | Default | Description |
||----------|---------|-------------|
|| `FabulousAstOutputFolder` | `Generated` | Output folder for generated files (relative to project directory) |
|| `EnableFabulousAstJson` | `true` | Enable/disable generation (set to `false` to skip) |

> **Note:** `EnableFabulousAstJson` defaults to `true`, so you only need to set it explicitly if you want to disable generation.

### Item Metadata

|| Metadata | Default | Description |
||----------|---------|-------------|
|| `RootName` | `Root` | Root type name |
|| `ModuleName` | _(empty)_ | File-level module name (e.g., `MyApp.Models`) |
|| `OutputFileName` | `{InputName}.g.fs` | Output filename |

## Incremental Builds

The task uses content hashing for efficient builds:

- Files regenerate only when JSON content changes
- Configuration changes trigger regeneration
- Generated files include hash comments for verification

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

### File Not Updating

```bash
dotnet clean && dotnet build
```

### Disable Generation

Via project property:
```xml
<EnableFabulousAstJson>false</EnableFabulousAstJson>
```

Via command line:
```bash
dotnet build -p:EnableFabulousAstJson=false
```

## Next Steps

- Learn about [programmatic generation](JSON.html) with Fabulous.AST.Json
- Explore the [Fabulous.AST DSL](../widgets/Oak.html) for more code generation options
*)
