(**
---
title: Discriminated Unions
category: widgets
index: 12
---
*)

(**
# Discriminated Unions

## Contents
- [Overview](#overview)
- [Basic Usage](#basic-usage)
- [Union Cases](#union-cases)
- [Union Cases with Fields](#union-cases-with-fields)
- [Named and Anonymous Fields](#named-and-anonymous-fields)
- [Type Parameters](#type-parameters)
- [Union Attributes](#union-attributes)
- [Access Modifiers](#access-modifiers)
- [XML Documentation](#xml-documentation)
- [Union Members](#union-members)
- [Struct Unions](#struct-unions)
- [Using Unions Instead of Object Hierarchies](#using-unions-instead-of-object-hierarchies)
- [Tree Data Structures with Recursive Unions](#tree-data-structures-with-recursive-unions)
- [Mutually Recursive Unions](#mutually-recursive-unions)
- [Unwrapping Union Cases](#unwrapping-union-cases)
- [Pattern Matching with Unions](#pattern-matching-with-unions)

## Overview
Discriminated unions provide support for values that can be one of a number of named cases, possibly each with different values and types. They are useful for heterogeneous data, handling special cases (including valid and error cases), data that varies in type, and as an alternative to small object hierarchies. Recursive discriminated unions can represent tree data structures.

## Basic Usage
Create a discriminated union with the `Union` widget:
*)

#r "../../src/Fabulous.AST/bin/Release/netstandard2.1/publish/Fantomas.Core.dll"
#r "../../src/Fabulous.AST/bin/Release/netstandard2.1/publish/Fabulous.AST.dll"
#r "../../src/Fabulous.AST/bin/Release/netstandard2.1/publish/Fantomas.FCS.dll"

open Fabulous.AST
open type Fabulous.AST.Ast

Oak() {
    AnonymousModule() {
        Union("Color") {
            UnionCase("Red")
            UnionCase("Green")
            UnionCase("Blue")
        }
    }
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)

(**
## Union Cases
Add cases to unions with the `UnionCase` widget. The `option` type is a simple discriminated union in the F# core library:
*)

Oak() {
    AnonymousModule() {
        Union("Option") {
            UnionCase("Some", "'a")
            UnionCase("None")
        }
        |> _.typeParams(PostfixList("'a"))
    }
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)

(**
## Union Cases with Fields
Add fields to union cases, as in this Shape type example from the F# documentation:
*)

Oak() {
    AnonymousModule() {
        Union("Shape") {
            UnionCase("Rectangle", [ Field("width", Float()); Field("length", Float()) ])
            UnionCase("Circle", Field("radius", Float()))
            UnionCase("Prism", [ Field("width", Float()); Field(Float()); Field("height", Float()) ])
        }
    }
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)

(**
## Named and Anonymous Fields can be named or anonymous. In the previous example, most fields are named, but the second field of the Prism case is anonymous.

You construct objects by providing values for the named and anonymous fields:

```fsharp
// Using the Shape type from above
let rect = Rectangle(length = 1.3, width = 10.0)
let circ = Circle(1.0)
let prism = Prism(5.0, 2.0, height = 3.0)
```

In the Fabulous.AST construction, you can define fields with or without names:
*)

Oak() {
    AnonymousModule() {
        Union("Shape") {
            // Named fields
            UnionCase("Rectangle", [ Field("width", Float()); Field("length", Float()) ])

            // Anonymous field
            UnionCase("Circle", Float())

            // Mixed named and anonymous fields
            UnionCase("Prism", [ Field("width", Float()); Field(Float()); Field("height", Float()) ])
        }
    }
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)

(**
## Type Parameters
Add type parameters to unions with the `typeParams` method, like in the `option` type:
*)

Oak() {
    AnonymousModule() {
        Union("Option") {
            UnionCase("Some", "'a")
            UnionCase("None")
        }
        |> _.typeParams(PostfixList("'a"))
    }
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)

(**
## Union Attributes
Add attributes to unions with the `attribute` or `attributes` methods:
*)

Oak() {
    AnonymousModule() {
        // Using the RequireQualifiedAccess attribute
        Union("CommandType") {
            UnionCase("Create")
            UnionCase("Update")
            UnionCase("Delete")
        }
        |> _.attribute(Attribute("RequireQualifiedAccess"))

        // Using the Struct attribute for value type unions
        Union("StructShape") {
            UnionCase("Circle", Float())
            UnionCase("Square", Float())
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
## Access Modifiers
Set access modifiers for unions with the `toPublic`, `toPrivate`, and `toInternal` methods.
By default, accessibility for discriminated unions is public.
*)

Oak() {
    AnonymousModule() {
        Union("PublicUnion") {
            UnionCase("Case1")
            UnionCase("Case2")
        }
        |> _.toPublic()

        Union("PrivateUnion") {
            UnionCase("Case1")
            UnionCase("Case2")
        }
        |> _.toPrivate()
    }
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)

(**
## XML Documentation
Add XML documentation to unions with the `xmlDocs` method:
*)

Oak() {
    AnonymousModule() {
        Union("DocumentedUnion") {
            UnionCase("Case1")
            UnionCase("Case2")
        }
        |> _.xmlDocs([ "A discriminated union with documentation" ])
    }
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)

(**
## Union Members
Add members to unions with the `members` method. This example shows a Shape type with an Area property:
*)

Oak() {
    AnonymousModule() {
        (Union("Shape") {
            UnionCase("Circle", Field("radius", Float()))
            UnionCase("EquilateralTriangle", Field("side", Float()))
            UnionCase("Square", Field("side", Float()))
            UnionCase("Rectangle", [ Field("length", Float()); Field("width", Float()) ])
        })
            .members() {
            Member(
                "this.Area",
                MatchExpr(
                    "this",
                    [ MatchClauseExpr(
                          LongIdentPat("Circle", NamedPat("r")),
                          InfixAppExpr("Math.PI", "*", ParenExpr(InfixAppExpr("r", "*", "r")))
                      )
                      MatchClauseExpr(
                          LongIdentPat("EquilateralTriangle", [ NamedPat("s") ]),
                          InfixAppExpr(
                              InfixAppExpr("s", "*", "s"),
                              "*",
                              InfixAppExpr(AppExpr("sqrt", Float(3.0)), "/", Float(4.0))
                          )
                      )
                      MatchClauseExpr(LongIdentPat("Square", [ NamedPat("s") ]), InfixAppExpr("s", "*", "s"))

                      MatchClauseExpr(
                          LongIdentPat("Rectangle", ParenPat(TuplePat([ NamedPat("l"); NamedPat("w") ]))),
                          InfixAppExpr("l", "*", "w")
                      ) ]
                )
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
## Struct Unions
Create struct discriminated unions with the `Struct` attribute:
*)

Oak() {
    AnonymousModule() {
        Union("SingleCase") { UnionCase("Case", String()) }
        |> _.attribute(Attribute("Struct"))

        Union("Multicase") {
            UnionCase("Case1", String())
            UnionCase("Case2", Int())
            UnionCase("Case3", "double")
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
## Using Unions Instead of Object Hierarchies
Discriminated unions can be used as a simpler alternative to small object hierarchies.
Here's an example from the F# documentation:
*)

Oak() {
    AnonymousModule() {
        Union("Shape") {
            UnionCase("Circle", Field("radius", Float()))
            UnionCase("Square", Field("side", Float()))
            UnionCase("Rectangle", [ Field("height", Float()); Field("width", Float()) ])
        }

        // Calculate area using pattern matching instead of virtual methods
        Function(
            "area",
            ParameterPat("shape"),
            MatchExpr(
                "shape",
                [ MatchClauseExpr(
                      LongIdentPat("Circle", [ NamedPat("r") ]),
                      InfixAppExpr("Math.PI", "*", ParenExpr(InfixAppExpr("r", "*", "r")))
                  )
                  MatchClauseExpr(LongIdentPat("Square", NamedPat("s")), InfixAppExpr("s", "*", "s"))
                  MatchClauseExpr(
                      LongIdentPat("Rectangle", ParenPat(TuplePat([ NamedPat("h"); NamedPat("w") ]))),
                      InfixAppExpr("h", "*", "w")
                  ) ]
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
## Tree Data Structures with Recursive Unions
Use recursive discriminated unions to create tree data structures. Here's the Expression example from the F# documentation:
*)

Oak() {
    AnonymousModule() {
        Union("Expression") {
            UnionCase("Number", Int())
            UnionCase("Variable", String())

            UnionCase(
                "Add",
                [ Field("left", LongIdent("Expression"))
                  Field("right", LongIdent("Expression")) ]
            )

            UnionCase(
                "Multiply",
                [ Field("left", LongIdent("Expression"))
                  Field("right", LongIdent("Expression")) ]
            )
        }
        |> _.toRecursive()

        // Evaluates an expression with a given variable map
        Function(
            "evaluate",
            [ ParameterPat("expr"); ParameterPat("variables") ],
            MatchExpr(
                "expr",
                [ MatchClauseExpr(LongIdentPat("Number", NamedPat("n")), ConstantExpr "n")

                  MatchClauseExpr(
                      LongIdentPat("Variable", [ NamedPat("name") ]),
                      AppExpr("Map.find", [ "name"; "variables" ])
                  )

                  MatchClauseExpr(
                      LongIdentPat("Add", ParenPat(TuplePat([ NamedPat("left"); NamedPat("right") ]))),
                      InfixAppExpr(
                          AppExpr("evaluate", [ "left"; "variables" ]),
                          "+",
                          AppExpr("evaluate", [ "right"; "variables" ])
                      )
                  )
                  MatchClauseExpr(
                      LongIdentPat("Multiply", ParenPat(TuplePat([ NamedPat("left"); NamedPat("right") ]))),
                      InfixAppExpr(
                          AppExpr("evaluate", [ "left"; "variables" ]),
                          "*",
                          AppExpr("evaluate", [ "right"; "variables" ])
                      )
                  ) ]
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
## Mutually Recursive Unions
Discriminated unions in F# can be mutually recursive using the `and` keyword. Here's an example from the F# documentation:
*)

Oak() {
    AnonymousModule() {
        Union("Expression") {
            UnionCase("Literal", Int())
            UnionCase("Variable", String())

            UnionCase(
                "Operation",
                [ Field("op", String())
                  Field("left", LongIdent("Expression"))
                  Field("right", LongIdent("Expression")) ]
            )
        }

        Union("Statement") {
            UnionCase("Assign", [ Field("var", String()); Field("value", LongIdent("Expression")) ])
            UnionCase("Sequence", [ Field("Statement", List()) ])

            UnionCase(
                "IfElse",
                [ Field("condition", LongIdent("Expression"))
                  Field("thenBranch", LongIdent("Statement"))
                  Field("elseBranch", LongIdent("Statement")) ]
            )
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
## Unwrapping Union Cases
In F#, you can unwrap single-case unions easily:
*)
Oak() {
    AnonymousModule() {
        // Define a single-case union
        Union("ShaderProgram") { UnionCase("ShaderProgram", Int()) }

        // Unwrap using pattern matching
        Function(
            "someFunctionUsingShaderProgram",
            [ ParameterPat("shaderProgram") ],
            MatchExpr(
                "shaderProgram",
                [ MatchClauseExpr(
                      LongIdentPat("ShaderProgram", NamedPat("id")),
                      // Use the unwrapped value
                      AppExpr("useShader", [ "id" ])
                  ) ]
            )
        )

        Function(
            "someFunctionUsingShaderProgram",
            [ ParameterPat("shaderProgram") ],
            CompExprBodyExpr(
                [ LetOrUseExpr(Value(ParenPat(LongIdentPat("ShaderProgram", "id")), "shaderProgram"))
                  OtherExpr(UnitExpr()) ]
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

## Pattern Matching with Unions
Discriminated unions are powerful when combined with pattern matching. Here's the `getShapeWidth` example from the F# documentation:
*)

Oak() {
    AnonymousModule() {
        Function(
            "getShapeWidth",
            [ ParameterPat("shape") ],
            MatchExpr(
                "shape",
                [ MatchClauseExpr(
                      NamePatPairsPat("Rectangle", [ NamePatPairPat("width", "w") ]),
                      LongIdentSetExpr("w", "w")
                  )
                  MatchClauseExpr(
                      NamePatPairsPat("Circle", [ NamePatPairPat("radius", "r") ]),
                      InfixAppExpr(Float(2.0), "*", ParenExpr(InfixAppExpr("r", "*", "r")))
                  )
                  MatchClauseExpr(
                      NamePatPairsPat("Prism", [ NamePatPairPat("width", "w") ]),
                      LongIdentSetExpr("w", "w")
                  ) ]
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
Since F# 9, discriminated unions automatically expose `.Is*` properties:
*)
Oak() {
    AnonymousModule() {
        Union("Contact") {
            UnionCase("Email", Field("address", String()))
            UnionCase("Phone", [ Field("countryCode", Int()); Field("number", String()) ])
        }

        Function("canSendEmailTo", ParameterPat("person"), OptVarExpr("person.contact.IsEmail"))
    }
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"
// produces the following code:
(*** include-output ***)
