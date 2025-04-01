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
- [Overview](#overview)
- [Basic Usage](#basic-usage)
- [Constructors](#constructors)
- [Let and Do Bindings](#let-and-do-bindings)
- [Self Identifiers](#self-identifiers)
- [Members](#members)
- [Generic Type Parameters](#generic-type-parameters)
- [Access Modifiers](#access-modifiers)
- [Attributes](#attributes)
- [Inheritance](#inheritance)
- [Mutually Recursive Types](#mutually-recursive-types)
- [Struct Classes](#struct-classes)

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
            DoExpr(AppExpr(" printfn", [ String("%d %d"); Constant("x"); Constant("y") ]))

            // Additional constructor
            Constructor(
                ParenPat(TuplePat([ Constant("0"); Constant("0") ])),
                AppExpr("MyClass", ParenExpr(TupleExpr([ Constant("0"); Constant("0") ])))
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
## Let and Do Bindings
The `let` and `do` bindings in a class definition form the body of the primary class constructor, and therefore they run whenever a class instance is created. Let bindings can be used to create private fields or local variables, and do expressions execute initialization code.
*)

Oak() {
    AnonymousModule() {
        TypeDefn("Person", Constructor(ParenPat(ParameterPat("dataIn", Int())))) {
            // Let binding creates a private field
            LetBindings([ Value("data", "dataIn") ])

            // Do binding runs initialization code
            DoExpr(AppExpr(" self.PrintMessage", ConstantUnit()))

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
            Member("Counter.Create()", NewExpr("Counter", UnitExpr())).toStatic()
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
            // Instance methods
            Member("this.Add(a, b)", InfixAppExpr("a", "+", "b"))

            Member("this.Multiply(a, b)", InfixAppExpr("a", "*", "b"))

            // Property with explicit getter
            Member("this.Pi", Getter(Float(3.14159)))

            // Property with getter and setter
            Value("_result", Float(0.0)).toMutable()

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
        Value("intBox", NewExpr("Box", Int(42)))
        Value("nameBox", NewExpr("Box", String("John")))
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
Classes can inherit from a base class using the `inherit` clause. In Fabulous.AST, you can specify inheritance using appropriate widgets.
*)

Oak() {
    AnonymousModule() {
        // Base class
        TypeDefn("Animal", Constructor(ParameterPat("name", String()))) {
            Member("this.Name", ConstantExpr("name"))
            Member("this.MakeSound()", String("..."))
        }

        // Derived class
        TypeDefn("Dog", Constructor(ParenPat(ParameterPat("name", String())))) {
            // Inherit from base class
            InheritParen("Animal", ConstantExpr("name"))

            // Override method
            Member("this.MakeSound()", String("Woof!"))

            // Add new method
            Member("this.Fetch()", String("Fetching..."))
        }

        // Another derived class
        TypeDefn("Cat", Constructor(ParenPat(ParameterPat("name", String())))) {
            // Inherit from base class
            InheritParen("Animal", ConstantExpr("name"))

            // Override method
            Member("this.MakeSound()", String("Meow!"))

            // Add new method
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
                AppExpr("sqrt", [ InfixAppExpr(InfixAppExpr("x", "*", "x"), "+", InfixAppExpr("y", "*", "y")) ])
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
