(**
---
title: Migration Guide to v2.0.0
category: docs
index: 1
---
*)

(**
# Migration Guide to v2.0.0

This guide covers the breaking changes introduced in Fabulous.AST 2.0.0 and shows how to migrate your code.

## Overview of Breaking Changes

1. **Return type changes**: `MemberDefn`, `TypeDefn`, and `ModuleDecl` builders now return unified types instead of specific node types
2. **EscapeHatch requirement**: Raw SyntaxOak nodes must be wrapped with `EscapeHatch()` when yielding into collections
3. **Unified modifiers**: Per-node modifiers have been replaced with unified modifier extensions
4. **NestedModule return type**: `Module` builder now returns `CollectionBuilder<ModuleDecl, ModuleDecl>`

## 1. Return Type Changes

### MemberDefn Builders

Member definition builders now return `WidgetBuilder<MemberDefn>` instead of specific types like `WidgetBuilder<BindingNode>`.

**Before (v1.x):**
*)

#r "../src/Fabulous.AST/bin/Release/netstandard2.1/publish/Fantomas.Core.dll"
#r "../src/Fabulous.AST/bin/Release/netstandard2.1/publish/Fabulous.AST.dll"
#r "../src/Fabulous.AST/bin/Release/netstandard2.1/publish/Fantomas.FCS.dll"

open Fabulous.AST
open type Fabulous.AST.Ast

// In v1.x, Member returned WidgetBuilder<BindingNode>
// let method: WidgetBuilder<BindingNode> = Member("this.DoSomething()", ConstantExpr(Int 42))

(**
**After (v2.0):**

The same code now returns `WidgetBuilder<MemberDefn>`, which is compatible with all member collection contexts:
*)

Oak() {
    AnonymousModule() {
        TypeDefn("MyClass", UnitPat()) {
            // Member now returns WidgetBuilder<MemberDefn>
            Member("this.DoSomething()", ConstantExpr(Int 42))
            // Property also returns WidgetBuilder<MemberDefn>
            Member("this.Value", ConstantExpr(String "hello"))
        }
    }
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)

(**
### TypeDefn Builders

Type definition builders now return `WidgetBuilder<TypeDefn>` instead of specific types like `TypeDefnRecordNode`.

**Before (v1.x):**
*)

// In v1.x, Record returned CollectionBuilder with TypeDefnRecordNode
// let record: CollectionBuilder<TypeDefnRecordNode, FieldNode> = Record("Person") { ... }

(**
**After (v2.0):**

The same code now returns `CollectionBuilder<TypeDefn, FieldNode>`:
*)

Oak() {
    AnonymousModule() {
        // Record now returns CollectionBuilder<TypeDefn, FieldNode>
        Record("Person") {
            Field("Name", String())
            Field("Age", Int())
        }

        // Enum returns WidgetBuilder<TypeDefn>
        Enum("Color") {
            EnumCase("Red", Int(0))
            EnumCase("Green", Int(1))
        }

        // Abbrev returns WidgetBuilder<TypeDefn>
        Abbrev("MyInt", Int())
    }
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)

(**
### ModuleDecl Builders

Module declaration builders now return `WidgetBuilder<ModuleDecl>` instead of specific types.

**Before (v1.x):**
*)

// In v1.x, Open returned a specific type
// let openDecl: WidgetBuilder<OpenListNode> = Open("System")

(**
**After (v2.0):**
*)

Oak() {
    AnonymousModule() {
        // Open now returns WidgetBuilder<ModuleDecl>
        Open("System")
        Open("System.Collections.Generic")

        Value("x", Int(42))
    }
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)

(**
## 2. EscapeHatch for Raw SyntaxOak Nodes

When you need to inject raw SyntaxOak nodes into collections, you must now wrap them with `EscapeHatch()`.

**Before (v1.x):**
*)

open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

// In v1.x, you could yield raw nodes directly in some contexts
// AnonymousModule() {
//     TypeDefn.Abbrev(myAbbrevNode)  // This worked in v1.x
// }

(**
**After (v2.0):**
*)

// Create a raw TypeDefnAbbrevNode
let abbrevNode =
    TypeDefnAbbrevNode(
        TypeNameNode(
            None,
            None,
            SingleTextNode("type", Range.Zero),
            Some(SingleTextNode("MyFloat", Range.Zero)),
            IdentListNode([ IdentifierOrDot.Ident(SingleTextNode("=", Range.Zero)) ], Range.Zero),
            None,
            [],
            None,
            None,
            None,
            Range.Zero
        ),
        Type.LongIdent(IdentListNode([ IdentifierOrDot.Ident(SingleTextNode.Create("float")) ], Range.Zero)),
        [],
        Range.Zero
    )

Ast.Oak() {
    AnonymousModule() {
        // Use the DSL widget normally
        Abbrev("MyInt", Int())

        // Wrap raw SyntaxOak nodes with EscapeHatch
        EscapeHatch(TypeDefn.Abbrev(abbrevNode))
    }
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)

(**
### EscapeHatch with ModuleDecl
*)

let bindingNode =
    BindingNode(
        None,
        None,
        MultipleTextsNode([ SingleTextNode("let", Range.Zero) ], Range.Zero),
        false,
        None,
        None,
        Choice1Of2(IdentListNode([ IdentifierOrDot.Ident(SingleTextNode("y", Range.Zero)) ], Range.Zero)),
        None,
        List.Empty,
        None,
        SingleTextNode("=", Range.Zero),
        Expr.Constant(Constant.FromText(SingleTextNode("99", Range.Zero))),
        Range.Zero
    )

Ast.Oak() {
    AnonymousModule() {
        Value("x", Int(42))

        // Wrap raw ModuleDecl nodes with EscapeHatch
        EscapeHatch(ModuleDecl.TopLevelBinding(bindingNode))
    }
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)

(**
### EscapeHatch with Enum Cases
*)

let enumCaseNode =
    EnumCaseNode(
        None,
        None,
        None,
        SingleTextNode("Black", Range.Zero),
        SingleTextNode("=", Range.Zero),
        Expr.Constant(Constant.FromText(SingleTextNode("3", Range.Zero))),
        Range.Zero
    )

Ast.Oak() {
    AnonymousModule() {
        Enum("Colors") {
            EnumCase("Red", Int(0))
            EnumCase("Green", Int(1))
            EnumCase("Blue", Int(2))

            // Wrap raw EnumCaseNode with EscapeHatch
            EscapeHatch(enumCaseNode)
        }
    }
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)

(**
### EscapeHatch with Expr nodes
*)

let exprNode = Expr.Constant(Constant.FromText(SingleTextNode("name", Range.Zero)))

Ast.Oak() {
    AnonymousModule() {
        TypeDefn("Person", UnitPat()) {
            // Wrap raw Expr nodes with EscapeHatch when needed
            Member(ConstantPat(Constant("this.Name")), EscapeHatch(exprNode))
        }
    }
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)

(**
## 3. Unified Modifiers

Modifiers are now unified across widget types. Instead of per-node specific modifiers, use the common modifier extensions.

### MemberDefn Modifiers

Available modifiers for `WidgetBuilder<MemberDefn>`:
- `xmlDocs` - Add XML documentation
- `attributes` / `attribute` - Add attributes
- `toPrivate` / `toPublic` / `toInternal` - Set accessibility
- `toMutable` - Make mutable
- `toInlined` - Make inline
- `toStatic` - Make static
- `typeParams` - Add type parameters
*)

Ast.Oak() {
    AnonymousModule() {
        TypeDefn("MyClass", UnitPat()) {
            Member("this.PublicMethod()", ConstantExpr(Int 1))

            Member("this.PrivateMethod()", ConstantExpr(Int 2)) |> _.toPrivate()

            Member("this.InlineMethod()", ConstantExpr(Int 3)) |> _.toInlined()

            Member("this.DocumentedMethod()", ConstantExpr(Int 4))
            |> _.xmlDocs([ "This is a documented method" ])
        }
    }
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)

(**
### TypeDefn Modifiers

Available modifiers for `WidgetBuilder<TypeDefn>`:
- `members` - Add members to the type
- `xmlDocs` - Add XML documentation
- `attributes` / `attribute` - Add attributes
- `typeParams` - Add type parameters
- `toPrivate` / `toPublic` / `toInternal` - Set accessibility
- `toRecursive` - Make recursive
*)

Ast.Oak() {
    AnonymousModule() {
        Record("Person") {
            Field("Name", String())
            Field("Age", Int())
        }
        |> _.xmlDocs([ "Represents a person" ])
        |> _.attribute(Attribute("Serializable"))

        TypeDefn("GenericClass", UnitPat()) { Member("this.Value", ConstantExpr(String "")) }
        |> _.typeParams(PostfixList([ "'a"; "'b" ]))
        |> _.toPrivate()
    }
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)

(**
### ModuleDecl Modifiers

Available modifiers for Module/NestedModule:
- `xmlDocs` - Add XML documentation
- `attributes` / `attribute` - Add attributes
- `toPrivate` / `toPublic` / `toInternal` - Set accessibility
- `toRecursive` - Make recursive
*)

Ast.Oak() {
    AnonymousModule() {
        Module("MyModule") { Value("x", Int(42)) }
        |> _.xmlDocs([ "My module documentation" ])
        |> _.attribute(Attribute("AutoOpen"))

        Module("PrivateModule") { Value("secret", Int(999)) } |> _.toPrivate()

        Module("RecursiveModule") { Value("data", Int(1)) } |> _.toRecursive()
    }
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)

(**
## 4. NestedModule Return Type Change

The `Module` builder now returns `CollectionBuilder<ModuleDecl, ModuleDecl>` instead of a specific nested module type.
This allows for more flexible composition but may require adjustments if you were storing the result in a typed variable.

**Before (v1.x):**
*)

// In v1.x:
// let myModule: WidgetBuilder<NestedModuleNode> = Module("MyModule") { ... }

(**
**After (v2.0):**
*)

// The Module builder works the same way in usage, just with a different return type
Ast.Oak() {
    AnonymousModule() {
        Module("OuterModule") {
            Value("x", Int(1))

            Module("InnerModule") { Value("y", Int(2)) }
        }
    }
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)

(**
## 5. Complete Migration Example

Here's a complete example showing a type that uses multiple v2.0 features:
*)

Ast.Oak() {
    Namespace("MyApp.Domain") {
        Module("Types") {
            // Type with unified modifiers
            Record("Customer") {
                Field("Id", Int())
                Field("Name", String())
                Field("Email", String())
            }
            |> _.xmlDocs([ "Represents a customer in the system" ])
            |> _.attribute(Attribute("Serializable"))

            // Enum with unified modifiers
            Enum("OrderStatus") {
                EnumCase("Pending", Int(0))
                EnumCase("Processing", Int(1))
                EnumCase("Completed", Int(2))
                EnumCase("Cancelled", Int(3))
            }
            |> _.attribute(Attribute("FlagsAttribute"))

            // Class with members using unified modifiers
            TypeDefn("OrderService", Constructor(ParameterPat("customerId", Int()))) {
                Member("this.CustomerId", ConstantExpr("customerId"))

                Member("this.Process()", ConstantExpr(String "Processing"))
                |> _.xmlDocs([ "Processes the order" ])
            }
        }
        |> _.attribute(Attribute("AutoOpen"))
    }
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)

(**
## Summary

When migrating to v2.0.0:

1. **No code changes needed** for most DSL usage - the return type changes are transparent
2. **Wrap raw SyntaxOak nodes** with `EscapeHatch()` when yielding into collections
3. **Use unified modifiers** like `toPrivate()`, `xmlDocs()`, `attribute()` which work consistently across all widget types
4. **Update type annotations** if you explicitly typed variables with specific node types like `WidgetBuilder<BindingNode>` - change them to `WidgetBuilder<MemberDefn>`

The v2.0 changes provide a cleaner, more consistent API while maintaining full compatibility with the Fantomas Oak AST.
*)
