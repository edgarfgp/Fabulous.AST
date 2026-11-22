(**
---
title: Modules
category: widgets
index: 3
---
*)

(**
# Modules

## Contents
- [Overview](#Overview)
- [Basic Usage](#Basic-Usage)
- [Anonymous Modules](#Anonymous-Modules)
- [Named Modules](#Named-Modules)
- [Hash Directives](#Hash-Directives)
- [Nested Modules](#Nested-Modules)
- [Referencing Code in Modules](#Referencing-Code-in-Modules)
- [Recursive Modules](#Recursive-Modules)
- [Value Declarations in Modules](#Value-Declarations-in-Modules)
- [Function Declarations in Modules](#Function-Declarations-in-Modules)
- [Type Declarations in Modules](#Type-Declarations-in-Modules)

## Overview
Modules in F# are a way to group related functionality together. All F# code exists within modules. When you create a new file,
if it doesn't begin with a namespace declaration or a top-level module declaration, the entire contents of the file become part of an
implicit anonymous module.

## Basic Usage
Create a module with the `Module` widget:
*)

#r "../../src/Fabulous.AST/bin/Release/netstandard2.1/publish/Fantomas.Core.dll"
#r "../../src/Fabulous.AST/bin/Release/netstandard2.1/publish/Fabulous.AST.dll"
#r "../../src/Fabulous.AST/bin/Release/netstandard2.1/publish/Fantomas.FCS.dll"

open Fabulous.AST
open type Fabulous.AST.Ast

Oak() {
    AnonymousModule() {
        Module("MyModule") {
            Value("x", Int(42))
            Function("add", [ ParameterPat("a"); ParameterPat("b") ], InfixAppExpr("a", "+", "b"))
        }
    }
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)

(**
## Anonymous Modules
Anonymous modules represent the implicit module created for the content of a file:
*)

Oak() {
    AnonymousModule() {
        Value("x", Int(1))
        Function("add", [ ParameterPat("a"); ParameterPat("b") ], InfixAppExpr("a", "+", "b"))
    }
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)

(**
## Named Modules
Named modules explicitly define a module with a name:
*)

Oak() {
    AnonymousModule() {
        Module("MathModule") {
            Value("pi", Float(3.14159))
            Function("square", ParameterPat("x"), InfixAppExpr("x", "*", "x"))
        }
    }
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)

(**
## Hash Directives
Hash directives are declarations: `NoWarn` creates `#nowarn` and `HashDirective(name, arguments)` creates any other directive:
*)

Oak() {
    AnonymousModule() {
        NoWarn("0044")
        Module("MyModule") { Value("x", Int(42)) }
    }
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)

(**
To place directives at the top of the file, before any namespace or module, pass them to the
`hashDirective` or `hashDirectives` modifier of `Oak()`:
*)

Oak() { Namespace("MyApp") { Module("MyModule") { Value("x", Int(42)) } } }
|> _.hashDirectives([ NoWarn("0044"); HashDirective("r", String("nuget: FSharp.Data")) ])
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)

(**
## Nested Modules
Modules can be nested inside other modules:
*)

Oak() {
    AnonymousModule() {
        Module("Outer") {
            Value("outerValue", Int(1))

            Module("Inner") { Value("innerValue", Int(2)) }
        }
    }
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)

(**
## Referencing Code in Modules
When referencing code from other modules, you can either use a qualified name or open the module:
*)

Oak() {
    AnonymousModule() {
        Module("Math") {
            Function("add", [ ParameterPat("x"); ParameterPat("y") ], InfixAppExpr("x", "+", "y"))
            Function("multiply", [ ParameterPat("x"); ParameterPat("y") ], InfixAppExpr("x", "*", "y"))
        }

        // Using qualified name
        Value("sum", AppExpr("Math.add", [ Int(5); Int(10) ]))

        // Opening the module
        Open("Math")
        Value("product", AppExpr("multiply", [ Int(5); Int(10) ]))
    }
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)

(**
## Recursive Modules
Create recursive modules with the `toRecursive` method. This is useful when modules, types, and functions within need to reference each other:
*)

Oak() {
    AnonymousModule() {
        Module("RecursiveModule") {
            Union("Tree") {
                UnionCase("Leaf", "int")
                UnionCase("Node", [ Field("value", "int"); Field("left", "Tree"); Field("right", "Tree") ])
            }

            Function(
                "sum",
                ParameterPat("tree"),
                MatchExpr(
                    "tree",
                    [ MatchClauseExpr(("Leaf(n)"), "n")
                      MatchClauseExpr(
                          ConstantPat("Node(v, l, r)"),
                          InfixAppExpr("v", "+", ParenExpr(InfixAppExpr(AppExpr("sum", "l"), "+", AppExpr("sum", "r"))))
                      ) ]
                )
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
## Value Declarations in Modules
Add values to modules with the `Value` widget:
*)

Oak() {
    AnonymousModule() {
        Module("Constants") {
            Value("pi", Float(3.14159))
            Value("e", Float(2.71828))
            Value("phi", Float(1.61803))
        }
    }
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)

(**
## Function Declarations in Modules
Add functions to modules with the `Function` widget:
*)

Oak() {
    AnonymousModule() {
        Module("Functions") {
            Function("increment", ParameterPat("x"), InfixAppExpr("x", "+", Int(1)))
            Function("decrement", ParameterPat("x"), InfixAppExpr("x", "-", Int(1)))
            Function("apply", [ ParameterPat("f"); ParameterPat("x") ], AppExpr("f", "x"))
        }
    }
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)

(**
## Type Declarations in Modules
Add type declarations to modules:
*)

Oak() {
    AnonymousModule() {
        Module("Types") {
            Record("Person") {
                Field("Name", "string")
                Field("Age", "int")
            }

            Union("Shape") {
                UnionCase("Circle", Float())
                UnionCase("Square", Float())
            }

            TypeDefn(
                "Point",
                Constructor(ParenPat(TuplePat([ ParameterPat("x", Float()); ParameterPat("y", Float()) ])))
            ) {
                Member("this.X", ConstantExpr("x"))
                Member("this.Y", ConstantExpr("y"))
            }
        }
    }
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)
