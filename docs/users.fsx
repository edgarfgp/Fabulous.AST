(**
---
title: Users
category: docs
index: 1
---

# Projects Using Fabulous.AST

This page showcases projects and tools that leverage Fabulous.AST for F# code generation. If you're using Fabulous.AST in your project, we'd love to hear about it!

## Contents
- [Featured Projects](#featured-projects)
- [Community Projects](#community-projects)
- [Tools and Libraries](#tools-and-libraries)
- [Add Your Project](#add-your-project)

## Featured Projects

### Code Generation Tools

Projects that use Fabulous.AST to generate F# code programmatically.

**Example: Domain-Specific Language Generators**
- **Description**: Tools that transform domain models or configuration files into F# code
- **Use Case**: Generating F# record types, discriminated unions, and modules from external schemas
- **Benefits**: Type-safe code generation with proper formatting and structure

### Developer Productivity Tools

Tools that enhance developer workflow by automating code creation.

**Example: Scaffolding Generators**
- **Description**: Project templates and code scaffolding tools
- **Use Case**: Creating boilerplate code for common F# patterns
- **Benefits**: Consistent code structure and reduced manual typing

## Community Projects

We encourage the community to share their projects using Fabulous.AST. Here are some examples of how the library can be used:

### 1. Configuration-to-Code Generators

Transform configuration files (JSON, YAML, XML) into F# types and modules:

```fsharp
// Example: Generate F# records from JSON schema
open Fabulous.AST
open type Fabulous.AST.Ast

let generateRecordFromSchema schemaName fields =
    Oak() {
        AnonymousModule() {
            Record(schemaName) {
                for field in fields do
                    Field(field.Name, field.Type)
            }
        }
    }
```

### 2. API Client Generators

Generate F# client code from API specifications:

```fsharp
// Example: Generate API client methods
let generateApiClient endpoints =
    Oak() {
        AnonymousModule() {
            for endpoint in endpoints do
                Function(endpoint.Name) {
                    Parameter("request", endpoint.RequestType)
                    ReturnType(endpoint.ResponseType)
                    // Method body...
                }
        }
    }
```

### 3. DSL Implementations

Create domain-specific languages that compile to F# code:

```fsharp
// Example: SQL-like DSL that generates F# query functions
let generateQueryFunction tableName columns =
    Oak() {
        AnonymousModule() {
            Function($"query{tableName}") {
                for col in columns do
                    Parameter(col.Name.ToLower(), col.Type)
                // Query logic...
            }
        }
    }
```

## Tools and Libraries

### Build Tools Integration

Examples of integrating Fabulous.AST with build processes:

- **MSBuild Tasks**: Custom build tasks that generate F# code during compilation
- **FAKE Scripts**: Build scripts that use Fabulous.AST for code generation
- **Source Generators**: .NET source generators powered by Fabulous.AST

### IDE Extensions

Developer tools that enhance the F# development experience:

- **VS Code Extensions**: Extensions that generate F# code snippets
- **Visual Studio Add-ins**: Tools for automated code creation
- **Command-line Utilities**: CLI tools for code generation workflows

## Add Your Project

We'd love to feature your project that uses Fabulous.AST! Here's how you can contribute:

### Submission Guidelines

1. **Project Description**: Provide a clear description of your project and how it uses Fabulous.AST
2. **Use Case**: Explain the problem your project solves
3. **Code Examples**: Include relevant code snippets showing Fabulous.AST usage
4. **Links**: Provide links to your project repository or documentation

### How to Submit

- **GitHub Issue**: Open an issue in the [Fabulous.AST repository](https://github.com/edgarfgp/Fabulous.AST/issues) with the "user-showcase" label
- **Pull Request**: Submit a PR adding your project to this page
- **Discussion**: Join the discussion in our community channels

### Example Template

When submitting your project, please use this template:

```markdown
### Project Name

**Description**: Brief description of your project
**Repository**: [Link to repository]
**Documentation**: [Link to docs (if available)]
**Use Case**: How you use Fabulous.AST in your project

**Code Example**:
```fsharp
// Your Fabulous.AST code example here
```

**Benefits**: What benefits you've gained from using Fabulous.AST
```

## Getting Started with Fabulous.AST

If you're inspired by these examples and want to start using Fabulous.AST in your own projects:

1. **Install the package**: `dotnet add package Fabulous.AST`
2. **Read the documentation**: Visit our [main documentation](index.html)
3. **Explore examples**: Check out the [widgets documentation](widgets/) for detailed usage examples
4. **Join the community**: Participate in discussions and share your experiences

## Success Stories

*We're collecting success stories from the community. If Fabulous.AST has helped your project, please share your experience!*

### Benefits Reported by Users

- **Reduced Development Time**: Automated code generation saves hours of manual work
- **Consistent Code Quality**: Generated code follows F# best practices and formatting standards
- **Maintainability**: Changes to generation logic automatically propagate to all generated code
- **Type Safety**: Compile-time validation ensures generated code is correct

### Common Use Cases

1. **Schema-to-Code Generation**: Converting external schemas to F# types
2. **Boilerplate Reduction**: Automating repetitive code patterns
3. **DSL Implementation**: Building domain-specific languages that compile to F#
4. **API Integration**: Generating client code from API specifications
5. **Configuration Management**: Converting configuration to strongly-typed F# code

---

*This page is maintained by the Fabulous.AST community. Contributions and updates are welcome!*
*)