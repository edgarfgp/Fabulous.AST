# Fabulous.AST.Build

An MSBuild task that automatically generates F# types from JSON files at build time.

## What is it?

Fabulous.AST.Build is a **build-time code generator** that:

- Watches JSON files in your project
- Automatically generates F# record types during build
- Supports incremental builds (only regenerates when JSON changes)
- Integrates seamlessly with MSBuild and .NET CLI

**Use cases:**
- Generate F# types from API response samples
- Keep type definitions in sync with JSON schemas
- Automate type generation in CI/CD pipelines

> **💡 Tip:** If you need programmatic control over generation, use [Fabulous.AST.Json](https://www.nuget.org/packages/Fabulous.AST.Json) directly.

## Installation

```bash
dotnet add package Fabulous.AST.Json.Build
```

**Requirements:** .NET 8.0 or later

## How to use

### 1. Add JSON files to your project

Create a JSON file with sample data, e.g., `Schemas/user.json`:

```json
{
  "id": 1,
  "name": "Alice",
  "email": "alice@example.com",
  "isActive": true
}
```

### 2. Configure your .fsproj

Add `FabulousAstJson` items to your project file:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Fabulous.AST.Json.Build" Version="1.0.0" />
  </ItemGroup>

  <!-- Configure JSON files for type generation -->
  <ItemGroup>
    <FabulousAstJson Include="Schemas/user.json" />
  </ItemGroup>

  <!-- Include the generated file in compilation -->
  <ItemGroup>
    <Compile Include="Generated/user.Generated.fs" />
  </ItemGroup>
</Project>
```

### 3. Build your project

```bash
dotnet build
```

The task generates `Generated/user.Generated.fs`:

```fsharp
// Auto-generated from Schemas/user.json
// Do not edit manually

type User = { 
    id: int
    name: string
    email: string
    isActive: bool 
}
```

## Configuration options

### Custom output directory

```xml
<PropertyGroup>
  <FabulousAstJsonOutputDir>Types</FabulousAstJsonOutputDir>
</PropertyGroup>
```

Generated files will be placed in the `Types/` directory.

### Custom root type name

By default, the root type name is derived from the JSON filename. Override it with `RootName`:

```xml
<ItemGroup>
  <FabulousAstJson Include="Schemas/api-response.json" RootName="ApiResponse" />
</ItemGroup>
```

### Multiple JSON files

```xml
<ItemGroup>
  <FabulousAstJson Include="Schemas/user.json" />
  <FabulousAstJson Include="Schemas/product.json" RootName="Product" />
  <FabulousAstJson Include="Schemas/order.json" RootName="CustomerOrder" />
</ItemGroup>

<ItemGroup>
  <Compile Include="Generated/user.Generated.fs" />
  <Compile Include="Generated/product.Generated.fs" />
  <Compile Include="Generated/order.Generated.fs" />
</ItemGroup>
```

### Using glob patterns

```xml
<ItemGroup>
  <FabulousAstJson Include="Schemas/**/*.json" />
</ItemGroup>
```

## Generated output

### From objects

**Input (`user.json`):**
```json
{ "name": "Alice", "age": 30 }
```

**Output (`user.Generated.fs`):**
```fsharp
type User = { name: string; age: int }
```

### From arrays

**Input (`users.json`):**
```json
[{ "id": 1, "name": "Alice" }, { "id": 2, "name": "Bob" }]
```

**Output (`users.Generated.fs`):**
```fsharp
type UsersItem = { id: int; name: string }
type Users = UsersItem list
```

### Nested objects

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

**Output (`company.Generated.fs`):**
```fsharp
type CompanyAddress = { street: string; city: string }
type Company = { name: string; address: CompanyAddress }
```

## Incremental builds

The task uses content hashing to detect changes. Files are only regenerated when:
- The JSON file content changes
- The generated file is missing
- Configuration options change

This ensures fast incremental builds.

## Troubleshooting

### Generated file not updating

1. Ensure the JSON file is included as `FabulousAstJson`:
   ```xml
   <FabulousAstJson Include="path/to/file.json" />
   ```

2. Check that the generated file is included in compilation:
   ```xml
   <Compile Include="Generated/file.Generated.fs" />
   ```

3. Try a clean rebuild:
   ```bash
   dotnet clean && dotnet build
   ```

### Build errors in generated code

Check that your JSON is valid. The generator requires well-formed JSON input.

## Related packages

- **[Fabulous.AST](https://www.nuget.org/packages/Fabulous.AST)** - Core DSL for generating F# code
- **[Fabulous.AST.Json](https://www.nuget.org/packages/Fabulous.AST.Json)** - Programmatic API for JSON-to-F# generation
