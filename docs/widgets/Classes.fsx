(**
---
title: Classes
category: widgets
index: 15
---
*)

(**
# Classes

## Contents
- [Overview](#Overview)
- [Basic Usage](#Basic-Usage)
- [Constructors](#Constructors)
- [Let and Do Bindings](#Let-and-Do-Bindings)
- [Self Identifiers](#Self-Identifiers)
- [Members](#Members)
- [Generic Type Parameters](#Generic-Type-Parameters)
- [Access Modifiers](#Access-Modifiers)
- [Attributes](#Attributes)
- [Inheritance](#Inheritance)
- [Abstract and Default Members](#Abstract-and-Default-Members)
- [Explicit Fields and Constructors](#Explicit-Fields-and-Constructors)
- [Mutually Recursive Types](#Mutually-Recursive-Types)
- [Struct Classes](#Struct-Classes)
- [Class, Struct and Interface Blocks](#Class-Struct-and-Interface-Blocks)

## Overview
Classes in F# are types that represent objects that can have properties, methods, and events. Classes are the primary type concept that supports object-oriented programming in F#. In Fabulous.AST, classes are created using the `TypeDefn` widget.

## Basic Usage
Create a class with the `TypeDefn` widget:
*)

#r "../../src/Fabulous.AST/bin/Release/netstandard2.1/publish/Fantomas.Core.dll"
#r "../../src/Fabulous.AST/bin/Release/netstandard2.1/publish/Fabulous.AST.dll"
#r "../../src/Fabulous.AST/bin/Release/netstandard2.1/publish/Fantomas.FCS.dll"

open Fabulous.AST
open type Fabulous.AST.Ast

Oak() {
    AnonymousModule() {
        TypeDefn("Person", Constructor()) {
            Member("this.Name", String("John"))
            Member("this.Age", Int(30))
        }
    }
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)

(**
## Constructors
The constructor is code that creates an instance of the class type. In an F# class, there is always a primary constructor whose arguments are described in the parameter list that follows the type name. Additional constructors can be added using the `Constructor` member.
*)

Oak() {
    AnonymousModule() {
        // Class with a parameterless primary constructor
        TypeDefn("Person1", Constructor()) {
            Member("this.Name", String(""))
            Member("this.Age", Int(0))
        }

        // Class with parameters in the primary constructor
        TypeDefn("Person2", Constructor(TuplePat([ ParameterPat("name", String()); ParameterPat("age", Int()) ]))) {
            Member("this.Name", ConstantExpr("name"))
            Member("this.Age", ConstantExpr("age"))
        }

        // Class with an additional constructor
        TypeDefn("MyClass", Constructor(TuplePat([ ParameterPat("x", Int()); ParameterPat("y", Int()) ]))) {
            DoExpr(AppExpr("printfn", [ String("%d %d"); Constant("x"); Constant("y") ]))

            // Additional parameterless constructor that calls the primary one
            Constructor(UnitPat(), AppExpr("MyClass", ParenExpr(TupleExpr([ Constant("0"); Constant("0") ]))))
        }
    }
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)

(**
`TypeDefn` is a thin wrapper over `BaseTypeDefn(name, constructor)`, which takes the primary
constructor as a `voption`. It is handy when the constructor is only known at run time. This
generates the same class with and without a primary constructor:
*)

let classWith (name: string) (ctor: WidgetBuilder<Fantomas.Core.SyntaxOak.ImplicitConstructorNode> voption) =
    BaseTypeDefn(name, ctor) { Member("Version", String("1.0")).toStatic() }

Oak() {
    AnonymousModule() {
        classWith "Service" (ValueSome(Constructor()))
        classWith "Settings" ValueNone
    }
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)

(**
## Let and Do Bindings
The `let` and `do` bindings in a class definition form the body of the primary class constructor, and therefore they run whenever a class instance is created. Let bindings can be used to create private fields or local variables, and do expressions execute initialization code.
*)

Oak() {
    AnonymousModule() {
        TypeDefn("Person", Constructor(ParenPat(ParameterPat("dataIn", Int())))) {
            // Let binding creates a private field
            LetBindings([ Value("data", "dataIn") ])

            // LetBinding adds a single binding
            LetBinding(Value("created", "System.DateTime.Now"))

            // Do binding runs initialization code
            DoExpr(AppExpr("printfn", [ String("Created Person with data %d"); Constant("data") ]))

            // Method that can access the private field
            Member(
                "this.PrintMessage()",
                AppExpr("printf", [ String("Creating Person with Data %d"); Constant("data") ])
            )

            // Property that returns the private field
            Member("this.Data", ConstantExpr("data"))
        }

        // Example with mutable fields
        TypeDefn("Student", UnitPat()) {
            // Mutable private fields
            Value("_name", String("")).toMutable()
            Value("_grade", Int(0)).toMutable()

            // Properties with getters and setters
            Member(
                "this.Name",
                Getter(ConstantExpr("_name")),
                Setter(NamedPat("value"), ConstantExpr("_name <- value"))
            )

            Member(
                "this.Grade",
                Getter(ConstantExpr("_grade")),
                Setter(NamedPat("value"), ConstantExpr("_grade <- value"))
            )
        }
    }
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)

(**
## Self Identifiers
A *self identifier* is a name that represents the current instance, similar to `this` in C# or `Me` in Visual Basic. In Fabulous.AST, you can refer to the class instance with identifiers like `this` in member declarations.
*)

Oak() {
    AnonymousModule() {
        TypeDefn("Counter", Constructor().alias("self")) {
            // Private field
            Value("count", Int(0)).toMutable()

            // Methods using the self identifier
            Member("this.Count", ConstantExpr("count"))

            Member("this.Increment()", ConstantExpr("count <- count + 1"))

            Member("this.Add(amount)", ConstantExpr("count <- count + amount"))

            // Static method
            Member("Create()", NewExpr("Counter", UnitExpr())).toStatic()
        }
    }
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)

(**
## Members
Classes can have various types of members including methods, properties, and events. In Fabulous.AST, these are added using the `Member` widget.
*)

Oak() {
    AnonymousModule() {
        TypeDefn("Calculator", Constructor()) {
            // Private mutable field (let bindings come before members)
            Value("_result", Float(0.0)).toMutable()

            // Instance methods
            Member("this.Add(a, b)", InfixAppExpr("a", "+", "b"))

            Member("this.Multiply(a, b)", InfixAppExpr("a", "*", "b"))

            // Property with explicit getter
            Member("this.Pi", Getter(Float(3.14159)))

            // Property with getter and setter
            Member(
                "this.Result",
                Getter(ConstantExpr("_result")),
                Setter(NamedPat("value"), ConstantExpr("_result <- value"))
            )

            // Static method
            Member("Square(x)", InfixAppExpr("x", "*", "x")).toStatic()
        }
    }
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)

(**
## Generic Type Parameters
Generic type parameters allow classes to work with different types. In Fabulous.AST, you can add type parameters to a class using the `typeParams` modifier.
*)

Oak() {
    AnonymousModule() {
        // Class with a single type parameter
        TypeDefn("Box", Constructor(ParameterPat("value", "'T"))) { Member("this.Value", ConstantExpr("value")) }
        |> _.typeParams(PostfixList("'T"))

        // Class with multiple type parameters
        TypeDefn("KeyValuePair", Constructor(TuplePat([ ParameterPat("key", "'K"); ParameterPat("value", "'V") ]))) {
            Member("this.Key", ConstantExpr("key"))
            Member("this.Value", ConstantExpr("value"))
        }
        |> _.typeParams(PostfixList([ "'K"; "'V" ]))

        // Using a generic class
        Value("intBox", NewExpr("Box", ParenExpr(Int(42))))
        Value("nameBox", NewExpr("Box", ParenExpr(String("John"))))
    }
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)

(**
## Access Modifiers
Classes can have different access modifiers like `public` (default), `private`, or `internal`. In Fabulous.AST, these are added using the corresponding modifiers.
*)

Oak() {
    AnonymousModule() {
        // Public class (default)
        TypeDefn("PublicClass", Constructor()) { Member("this.Method()", String("Public method")) }
        |> _.toPublic()

        // Private class
        TypeDefn("PrivateClass", Constructor()) { Member("this.Method()", String("Private method")) }
        |> _.toPrivate()

        // Internal class
        TypeDefn("InternalClass", Constructor()) { Member("this.Method()", String("Internal method")) }
        |> _.toInternal()
    }
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)

(**
## Attributes
You can apply attributes to classes using the `attribute` or `attributes` modifiers.
*)

Oak() {
    AnonymousModule() {
        // Class with a single attribute
        TypeDefn("SerializableClass", Constructor()) { Member("this.Data", String("Some data")) }
        |> _.attribute(Attribute("Serializable"))

        // Class with multiple attributes
        TypeDefn("AnnotatedClass", Constructor()) { Member("this.Method()", String("Method with attributes")) }
        |> _.attributes([ Attribute("Obsolete"); Attribute("Serializable") ])

        // Class with an attribute that has parameters
        TypeDefn("DeprecatedClass", Constructor()) { Member("this.Method()", String("Deprecated method")) }
        |> _.attribute(Attribute("Obsolete", String("Use NewClass instead")))
    }
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)

(**
## Inheritance
Classes can inherit from a base class using the `inherit` clause, generated with `InheritParen`. The derived classes below add new members; to override a virtual member, see [Abstract and Default Members](#Abstract-and-Default-Members).
*)

Oak() {
    AnonymousModule() {
        // Base class
        TypeDefn("Animal", Constructor(ParameterPat("name", String()))) { Member("this.Name", ConstantExpr("name")) }

        // Derived class
        TypeDefn("Dog", Constructor(ParenPat(ParameterPat("name", String())))) {
            // Inherit from base class
            InheritParen("Animal", ConstantExpr("name"))

            // New members
            Member("this.MakeSound()", String("Woof!"))
            Member("this.Fetch()", String("Fetching..."))
        }

        // Another derived class
        TypeDefn("Cat", Constructor(ParenPat(ParameterPat("name", String())))) {
            // Inherit from base class
            InheritParen("Animal", ConstantExpr("name"))

            // New members
            Member("this.MakeSound()", String("Meow!"))
            Member("this.Purr()", String("Purring..."))
        }
    }
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)

(**
`InheritUnit` produces `inherit Base()` for a base class with a parameterless constructor, and
`InheritOther` passes the argument expression as is (it must be parenthesized). An interface
inherits another interface with `Inherit`. This generates all three forms:
*)

Oak() {
    AnonymousModule() {
        TypeDefn("Repository", Constructor()) {
            InheritUnit("Object")
            Member("this.ToString", UnitPat(), String("Repository")).toOverride()
        }

        TypeDefn(
            "Rectangle",
            Constructor(ParenPat(TuplePat([ ParameterPat("width", Float()); ParameterPat("height", Float()) ])))
        ) {
            InheritOther("Measurement", ParenExpr(InfixAppExpr("width", "*", "height")))
        }

        TypeDefn("IResource") {
            Inherit("IDisposable")
            AbstractMember("Name", String())
        }
    }
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)

(**
## Abstract and Default Members
`AbstractMember` declares a virtual slot, `Default` gives it a default implementation, and
`.toOverride()` turns a `Member` into an `override` in a derived class. This generates a
`Shape` base class with two virtual methods and two subclasses that override them:
*)

Oak() {
    AnonymousModule() {
        TypeDefn("Shape", Constructor(ParenPat(ParameterPat("name", String())))) {
            AbstractMember("Area", [ Unit() ], Float())
            Default("this.Area", UnitPat(), Float(0.0))

            AbstractMember("Describe", [ Unit() ], String())

            Default(
                "this.Describe",
                UnitPat(),
                AppExpr(
                    "sprintf",
                    [ ConstantExpr(String("%s with area %.2f"))
                      ConstantExpr("name")
                      ParenExpr(AppExpr("this.Area", UnitExpr())) ]
                )
            )
        }

        TypeDefn("Circle", Constructor(ParenPat(ParameterPat("radius", Float())))) {
            InheritParen("Shape", String("circle"))
            Member("this.Area()", InfixAppExpr("Math.PI", "*", InfixAppExpr("radius", "*", "radius"))).toOverride()
        }

        TypeDefn("Square", Constructor(ParenPat(ParameterPat("side", Float())))) {
            InheritParen("Shape", String("square"))
            Member("this.Area()", InfixAppExpr("side", "*", "side")).toOverride()
            Member("this.Describe()", String("A perfect square")).toOverride()
        }
    }
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)

(**
## Explicit Fields and Constructors
A class without a primary constructor declares its fields with `ValField` (`val` fields,
optionally `.toMutable()`) and initializes them in explicit `Constructor`s. Inside a
constructor, `InheritRecordExpr` produces the `{ inherit Base(...); Field = value }` object
initializer and `ExplicitConstructorThenExpr` adds a `then` block that runs after
construction. This generates an exception type with an extra field and a linked-list node:
*)

Oak() {
    AnonymousModule() {
        TypeDefn("ValidationError") {
            Inherit("Exception")
            ValField("Field", String())

            Constructor(
                ParenPat(TuplePat([ ParameterPat("field", String()); ParameterPat("message", String()) ])),
                InheritRecordExpr(InheritParen("Exception", "message"), [ RecordFieldExpr("Field", "field") ])
            )

            Constructor(
                ParenPat(ParameterPat("field", String())),
                CompExprBodyExpr(
                    [ AppSingleParenArgExpr(
                          "ValidationError",
                          ParenExpr(TupleExpr([ Constant("field"); String("Invalid value") ]))
                      )
                      ExplicitConstructorThenExpr(
                          AppExpr("printfn", [ String("Validation failed for %s"); Constant("field") ])
                      ) ]
                )
            )
        }

        TypeDefn("Node") {
            ValField("Value", Int())
            ValField("Next", "Node option").toMutable()

            Constructor(
                ParenPat(ParameterPat("value", Int())),
                RecordExpr([ RecordFieldExpr("Value", "value"); RecordFieldExpr("Next", "None") ])
            )
        }
    }
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)

(**
## Mutually Recursive Types
When you define types that reference each other in a circular way, you use the `and` keyword in F#. In Fabulous.AST, you can create mutually recursive types by using the `toRecursive` modifier.
*)

Oak() {
    AnonymousModule() {
        // First mutually recursive type
        TypeDefn("Folder", Constructor(ParenPat(ParameterPat("pathIn", String())))) {
            LetBindings(
                [ Value("path", "pathIn")
                  Value("filenameArray", AppExpr("Directory.GetFiles", ParenExpr("path")), Array(String())) ]
            )

            Member(
                "this.FileArray",
                AppExpr(
                    "Array.map",
                    [ ParenLambdaExpr("elem", NewExpr("File", ParenExpr(TupleExpr([ "elem"; "this" ]))))
                      ConstantExpr "filenameArray" ]
                )
            )
        }

        // Second mutually recursive type
        TypeDefn(
            "File",
            Constructor(
                ParenPat(
                    TuplePat(
                        [ ParameterPat("filename", String())
                          ParameterPat("containingFolder", "Folder") ]
                    )
                )
            )
        ) {
            LetBindings([ Value("name", "filename") ])
            Member("this.ContainingFolder", "containingFolder")
        }
        |> _.toRecursive()
    }
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)

(**
## Struct Classes
You can create struct classes by applying the `Struct` attribute to a class. Structs are value types rather than reference types.
*)

Oak() {
    AnonymousModule() {
        // Struct class
        TypeDefn("Point", Constructor(TuplePat([ ParameterPat("x", Float()); ParameterPat("y", Float()) ]))) {
            Member("this.X", ConstantExpr("x"))
            Member("this.Y", ConstantExpr("y"))

            Member(
                "this.Distance",
                AppExpr("sqrt", ParenExpr(InfixAppExpr(InfixAppExpr("x", "*", "x"), "+", InfixAppExpr("y", "*", "y"))))
            )
        }
        |> _.attribute(Attribute("Struct"))
    }
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)

(**
## Class, Struct and Interface Blocks
`ClassEnd`, `StructEnd` and `InterfaceEnd` wrap the members in explicit `class ... end`,
`struct ... end` and `interface ... end` blocks; with no members they produce an empty type.
This generates an empty marker class, a struct with a primary constructor and an empty
marker interface:
*)

Oak() {
    AnonymousModule() {
        ClassEnd("Marker") { () }

        StructEnd("Vector", Constructor(TuplePat([ ParameterPat("x", Float()); ParameterPat("y", Float()) ]))) {
            Member("this.X", ConstantExpr("x"))
            Member("this.Y", ConstantExpr("y"))
        }

        InterfaceEnd("ITag") { () }
    }
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)
