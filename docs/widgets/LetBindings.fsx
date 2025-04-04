(**
---
title: Let Bindings
category: widgets
index: 13
---
*)

(**
# Let Bindings

## Contents
- [Overview](#overview)
- [Basic Usage](#basic-usage)
- [Let Bindings for Values](#let-bindings-for-values)
- [Let Bindings with Patterns](#let-bindings-with-patterns)
- [Function Bindings](#function-bindings)
- [Type Annotations](#type-annotations)
- [Mutable and Inlined Bindings](#mutable-and-inlined-bindings)
- [Let Bindings in Classes](#let-bindings-in-classes)
- [Type Parameters in Let Bindings](#type-parameters-in-let-bindings)
- [Attributes on Let Bindings](#attributes-on-let-bindings)
- [XML Documentation](#xml-documentation)
- [Access Modifiers](#access-modifiers)
- [Scope and Accessibility](#scope-and-accessibility)
- [Advanced Examples](#advanced-examples)

## Overview
A *binding* associates an identifier with a value or function. In F#, you use the `let` keyword to bind a name to a value or function. In Fabulous.AST, let bindings are represented using the `Value` widget for module-level declarations and `LetOrUseExpr` for expressions within functions.

## Basic Usage
Create a let binding at the module level using the `Value` widget:
*)

#r "../../src/Fabulous.AST/bin/Release/netstandard2.1/publish/Fantomas.Core.dll"
#r "../../src/Fabulous.AST/bin/Release/netstandard2.1/publish/Fabulous.AST.dll"
#r "../../src/Fabulous.AST/bin/Release/netstandard2.1/publish/Fantomas.FCS.dll"

open Fabulous.AST
open type Fabulous.AST.Ast

Oak() { AnonymousModule() { Value("x", Int(42)) } }
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)

(**
## Let Bindings for Values
Here are examples of simple let bindings for values:
*)

Oak() {
    AnonymousModule() {
        // Simple value binding
        Value("x", Int(1))

        // Multi-line expression with indentation
        Value(
            "message",
            AppExpr("String.concat", [ ConstantExpr(String(" ")); ListExpr([ String("Hello,"); String("World!") ]) ])
        )

        // Binding with type annotation
        Value("y", Float(3.14159), Float())

        // Binding with string value
        Value("greeting", String("Hello, World!"))

        // Binding with array expression
        Value("numbers", ArrayExpr([ Int(1); Int(2); Int(3); Int(4) ]))

        // Binding with verbatim string
        Value("z", ConstantExpr(VerbatimString("Some\nMultiline\nString")))
    }
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)

(**
## Let Bindings with Patterns
You can use patterns in let bindings. Here's an example with a tuple pattern:
*)

Oak() {
    AnonymousModule() {
        // Using a tuple pattern with TuplePat
        Value(TuplePat([ ParameterPat("a"); ParameterPat("b") ]), TupleExpr([ Int(1); Int(2) ]))

        // Simpler syntax with NamedPat
        Value(
            TuplePat([ NamedPat("x"); NamedPat("y"); NamedPat("z") ]),
            TupleExpr([ ConstantExpr(Int(1)); ConstantExpr(Int(2)); ConstantExpr(Int(3)) ])
        )

        // Using the bindings in the module
        Value("sum", InfixAppExpr("a", "+", "b"))
    }
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)

(**
## Function Bindings
Function bindings follow the rules for value bindings, but include parameters:
*)

Oak() {
    AnonymousModule() {
        // Simple function with one parameter
        Function("add1", ParameterPat("x"), InfixAppExpr("x", "+", Int(1)))

        // Function with multiple parameters
        Function("add", [ ParameterPat("a"); ParameterPat("b") ], InfixAppExpr("a", "+", "b"))

        // Function with a tuple parameter
        Function("addTuple", ParenPat(TuplePat([ NamedPat("a"); NamedPat("b") ])), InfixAppExpr("a", "+", "b"))

        // Function with a more complex body
        Function(
            "cylinderVolume",
            [ ParameterPat("radius"); ParameterPat("length") ],
            InfixAppExpr("length", "*", InfixAppExpr("Math.PI", "*", InfixAppExpr("radius", "*", "radius")))
        )

        // Function with a simpler syntax
        Function("greet", "name", AppExpr("printfn", String("Hello, %s!")))
    }
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)

(**
## Type Annotations
You can add type annotations to value and function bindings:
*)

Oak() {
    AnonymousModule() {
        // Value with type annotation
        Value("pi", Float(3.14159), Float())

        // Value with type widget
        Value("count", Int(42), Int())

        // Function with parameter type annotations and return type
        Function(
            "add",
            ParenPat(TuplePat([ ParameterPat("a", Int()); ParameterPat("b", Int()) ])),
            InfixAppExpr("a", "+", "b"),
            Int()
        )

        // Function with type annotation using widgets
        Function(
            "concat",
            ParenPat(TuplePat([ ParameterPat("a", String()); ParameterPat("b", String()) ])),
            InfixAppExpr("a", "+", "b")
        )
    }
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)

(**
## Mutable and Inlined Bindings
You can create mutable and inlined bindings:
*)

Oak() {
    AnonymousModule() {
        // Mutable value binding
        Value("counter", Int(0)).toMutable()

        // Mutable value with type annotation
        Value("total", Float(0.0), Float()).toMutable()

        // Inlined function
        Function("double", ParameterPat("x"), InfixAppExpr("x", "+", "x")).toInlined()

        // Inlined function with type parameters
        Function("identity", ParameterPat("x"), ConstantExpr("x"), LongIdent("'T -> 'T"))
            .toInlined()
            .typeParams(PostfixList("'T"))

    }
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)

(**
## Let Bindings in Classes
Let bindings can appear in class types but not in structure or record types. They define private fields and members for that class type:
*)

Oak() {
    AnonymousModule() {
        TypeDefn("MyClass", Constructor(UnitPat())) {
            // Let bindings in the class define private fields
            Value("field1", Int(1))
            Value("field2", String("hello"))

            // Methods can use these fields
            Member("this.GetField1", ConstantExpr("field1"))
            Member("this.GetField2", ConstantExpr("field2"))
        }
    }
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)

(**
## Type Parameters in Let Bindings
Let bindings at the module level can have explicit type parameters:
*)

Oak() {
    AnonymousModule() {
        // Generic function with type parameter
        Function("id", ParameterPat("x"), ConstantExpr("x"), LongIdent("'T -> 'T")).typeParams(PostfixList("'T"))

        // Value with type parameters
        Value("defaultValue", ConstantExpr("None")).typeParams(PostfixList("'T"))

        // Value with multiple type parameters
        Value("defaultPair", TupleExpr([ ConstantExpr("None"); ConstantExpr("None") ]))
            .typeParams(PostfixList([ "'T"; "'U" ]))

        // Using the generic function
        Value("result", AppExpr("id", Int(42)))
    }
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)

(**
## Attributes on Let Bindings
You can apply attributes to top-level let bindings in a module:
*)

Oak() {
    AnonymousModule() {
        // Single attribute
        Value("outdatedFunction", LambdaExpr(UnitPat(), String("this is old"))).attribute(Attribute("Obsolete"))

        // Multiple attributes
        Value("configValue", String("default")).attributes([ Attribute("EditorBrowsable"); Attribute("Obsolete") ])

        // Attribute with arguments
        Function("conditionalFunction", ParameterPat("x"), ConstantExpr("x"))
            .attribute(Attribute("Conditional", String("DEBUG")))

        // Attribute with expression arguments
        Value("maxValue", Int(100))
            .attribute(Attribute("Obsolete", ParenExpr(ConstantExpr(String("Use newMaxValue instead")))))
    }
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)

(**
## XML Documentation
You can add XML documentation to let bindings:
*)

Oak() {
    AnonymousModule() {
        // Simple XML comment
        Value("pi", Float(3.14159)).xmlDocs([ "The mathematical constant π (pi)" ])

        // Multi-line XML documentation
        Function("add", [ ParameterPat("a"); ParameterPat("b") ], InfixAppExpr("a", "+", "b"))
            .xmlDocs([ "Adds two numbers together"; "Returns the sum of the inputs" ])

        // Using structured XML documentation
        Function("multiply", [ ParameterPat("a"); ParameterPat("b") ], InfixAppExpr("a", "*", "b"))
            .xmlDocs(Summary("Multiplies two numbers together"))
    }
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)

(**
## Access Modifiers
You can specify access modifiers for let bindings:
*)

Oak() {
    AnonymousModule() {
        // Public binding (default)
        Value("publicValue", Int(1)).toPublic()

        // Private binding
        Value("privateValue", Int(2)).toPrivate()

        // Internal binding
        Value("internalValue", Int(3)).toInternal()

        // Access modifiers on functions
        Function("publicFunction", ParameterPat("x"), ConstantExpr("x")).toPublic()

        Function("privateFunction", ParameterPat("x"), ConstantExpr("x")).toPrivate()

        Function("internalFunction", ParameterPat("x"), ConstantExpr("x")).toInternal()
    }
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)

(**
## Scope and Accessibility
The scope of an entity declared with a let binding is limited to the portion of the containing scope (such as a function, module, file or class) after the binding appears. In a module, a let-bound value or function is accessible to clients of a module as long as the module is accessible, but let bindings in a class are private to the class:
*)

Oak() {
    AnonymousModule() {
        // Let bindings in a module
        Value("x", Int(42))

        // Defining a module with internal bindings
        Module("Math") {
            Value("pi", Float(3.14159))
            Function("square", ParameterPat("x"), InfixAppExpr("x", "*", "x"))
        }

        // Access module functions with qualified names
        Value("area", InfixAppExpr(Float(2.0), "*", AppExpr("Math.square", Float(4.0))))

        // Open the module to use unqualified names
        Open("Math")
        Value("circleArea", InfixAppExpr("pi", "*", AppExpr("square", Float(3.0))))
    }
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)

(**
Inside a function, you can use let expressions to create local bindings. In Fabulous.AST, this is done using `LetOrUseExpr` inside the function body:
*)

Oak() {
    AnonymousModule() {
        Function(
            "calculateArea",
            [ ParameterPat("width"); ParameterPat("height") ],
            CompExprBodyExpr(
                [ LetOrUseExpr(Value("area", InfixAppExpr("width", "*", "height")))
                  OtherExpr(AppExpr("printfn", [ ConstantExpr(String("Area: %d")); ConstantExpr("area") ])) ]
            )
        )
    }
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)

(**
## Advanced Examples
Here are some more complex examples of let bindings:
*)

Oak() {
    AnonymousModule() {
        // Function with nested let bindings
        Function(
            "calculateVolume",
            ParameterPat("radius"),
            CompExprBodyExpr(
                [ LetOrUseExpr(Value("pi", Double(3.14159)))
                  LetOrUseExpr(Value("radiusSquared", InfixAppExpr("radius", "*", "radius")))
                  LetOrUseExpr(
                      Value(
                          "sphereVolume",
                          InfixAppExpr(Float(4.0 / 3.0), "*", InfixAppExpr("pi", "*", "radiusSquared"))
                      )
                  )
                  OtherExpr("sphereVolume") ]
            )
        )

        // Using array values
        Value(
            "GdmtSubcommands",
            ArrayExpr(
                [ ConstantExpr(String("ControlFlow"))
                  ConstantExpr(String("Core"))
                  ConstantExpr(String("Expressions"))
                  ConstantExpr(String("LetBindings")) ]
            )
        )

        // Using function with Object methods
        Value("res", AppLongIdentAndSingleParenArgExpr([ "conn"; "Open" ], ConstantExpr(ConstantUnit())))
    }
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)
