namespace Fabulous.AST.Tests.Core

open Xunit
open Fabulous.AST.Tests

open Fabulous.AST

open type Ast

module TriviaTests =

    [<Fact>]
    let ``Single line comment before``() =
        Oak() {
            AnonymousModule() { Value("x", ConstantExpr(Int(42)), "int").triviaBefore(SingleLine("This is a comment")) }
        }
        |> produces
            """
// This is a comment
let x: int = 42
"""

    [<Fact>]
    let ``Line comment after source code``() =
        Oak() {
            AnonymousModule() {
                Value("x", ConstantExpr(Int(42)), "int").triviaAfter(LineCommentAfterSourceCode("This is a comment"))
            }
        }
        |> produces
            """
let x: int = 42 // This is a comment
"""

    [<Fact>]
    let ``Block comment without newlines``() =
        Oak() {
            AnonymousModule() {
                Value("x", ConstantExpr(Int(42)), "int").triviaBefore(BlockComment("This is a block comment"))
            }
        }
        |> produces
            """
(*This is a block comment*) let x: int = 42
"""

    [<Fact>]
    let ``Block comment with newlines``() =
        Oak() {
            AnonymousModule() {
                Value("x", ConstantExpr(Int(42)), "int")
                    .triviaBefore(BlockComment("This is a block comment", true, true))
            }
        }
        |> produces
            """
(*
This is a block comment
*)
let x: int = 42
"""

    [<Fact>]
    let ``Directive trivia``() =
        Oak() { AnonymousModule() { Value("x", ConstantExpr(Int(42)), "int").triviaBefore(Directive("#if DEBUG")) } }
        |> produces
            """
#if DEBUG
let x: int = 42
"""

    [<Fact>]
    let ``Newline trivia``() =
        Oak() {
            AnonymousModule() {
                Value("x", ConstantExpr(Int(42)), "int").triviaAfter(Newline())
                Value("y", ConstantExpr(Int(43)), "int")
            }
        }
        |> produces
            """
let x: int = 42


let y: int = 43
"""

    [<Fact>]
    let ``Multiple trivia before``() =
        Oak() {
            AnonymousModule() {
                Value("x", ConstantExpr(Int(42)), "int")
                    .triviaBefore([ SingleLine("First comment"); SingleLine("Second comment") ])
            }
        }
        |> produces
            """
// First comment
// Second comment
let x: int = 42
"""

    [<Fact>]
    let ``Multiple trivia after``() =
        Oak() {
            AnonymousModule() {
                Value("x", ConstantExpr(Int(42)), "int")
                    .triviaAfter([ LineCommentAfterSourceCode("First comment"); Newline() ])
            }
        }
        |> produces
            """
let x: int = 42 // First comment

"""

module PatternTriviaTests =

    [<Fact>]
    let ``Pattern trivia before with single line comment``() =
        Oak() {
            AnonymousModule() {
                Value(ConstantPat(Int(42)).triviaBefore(SingleLine("Pattern comment")), ConstantExpr(String("value")))
            }
        }
        |> produces
            """
// Pattern comment
let 42 = "value"
"""

    [<Fact>]
    let ``Pattern trivia before with block comment``() =
        Oak() {
            AnonymousModule() {
                Value(
                    ConstantPat(Int(42)).triviaBefore(BlockComment("Block pattern comment")),
                    ConstantExpr(String("value"))
                )
            }
        }
        |> produces
            """
(*Block pattern comment*) let 42 = "value"
"""

    [<Fact>]
    let ``Pattern trivia after with line comment``() =
        Oak() {
            AnonymousModule() {
                Value(
                    ConstantPat(Int(42)).triviaAfter(LineCommentAfterSourceCode("After pattern")),
                    ConstantExpr(String("value"))
                )
            }
        }
        |> produces
            """
let 42 // After pattern
    = "value"
"""

    [<Fact>]
    let ``Pattern trivia before with multiple comments``() =
        Oak() {
            AnonymousModule() {
                Value(
                    ConstantPat(Int(42)).triviaBefore([ SingleLine("First"); SingleLine("Second") ]),
                    ConstantExpr(String("value"))
                )
            }
        }
        |> produces
            """
// First
// Second
let 42 = "value"
"""

    [<Fact>]
    let ``Pattern trivia with TriviaNode``() =
        Oak() {
            AnonymousModule() {
                Value(
                    ConstantPat(Int(42)).triviaBefore(TriviaNode(SingleLine("TriviaNode comment"))),
                    ConstantExpr(String("value"))
                )
            }
        }
        |> produces
            """
// TriviaNode comment
let 42 = "value"
"""

    [<Fact>]
    let ``Named pattern with trivia``() =
        Oak() {
            AnonymousModule() { Value(NamedPat("x").triviaBefore(SingleLine("Named pattern")), ConstantExpr(Int(42))) }
        }
        |> produces
            """
// Named pattern
let x = 42
"""

module ExprTriviaTests =

    [<Fact>]
    let ``Expr trivia before with single line comment``() =
        Oak() { AnonymousModule() { Value("x", ConstantExpr(Int(42)).triviaBefore(SingleLine("Expr comment"))) } }
        |> produces
            """
let x =
    // Expr comment
    42
"""

    [<Fact>]
    let ``Expr trivia before with block comment``() =
        Oak() {
            AnonymousModule() { Value("x", ConstantExpr(Int(42)).triviaBefore(BlockComment("Block expr comment"))) }
        }
        |> produces
            """
let x = (*Block expr comment*) 42
"""

    [<Fact>]
    let ``Expr trivia after with line comment``() =
        Oak() {
            AnonymousModule() {
                Value("x", ConstantExpr(Int(42)).triviaAfter(LineCommentAfterSourceCode("After expr")))
            }
        }
        |> produces
            """
let x = 42 // After expr
"""

    [<Fact>]
    let ``Expr trivia before with multiple comments``() =
        Oak() {
            AnonymousModule() {
                Value("x", ConstantExpr(Int(42)).triviaBefore([ SingleLine("First"); SingleLine("Second") ]))
            }
        }
        |> produces
            """
let x =
    // First
    // Second
    42
"""

    [<Fact>]
    let ``Expr trivia with TriviaNode``() =
        Oak() {
            AnonymousModule() {
                Value("x", ConstantExpr(Int(42)).triviaBefore(TriviaNode(SingleLine("TriviaNode comment"))))
            }
        }
        |> produces
            """
let x =
    // TriviaNode comment
    42
"""

    [<Fact>]
    let ``Expr trivia after with newline``() =
        Oak() {
            AnonymousModule() {
                Value("x", ConstantExpr(Int(42)).triviaAfter(Newline()))
                Value("y", ConstantExpr(Int(43)))
            }
        }
        |> produces
            """
let x = 42

let y = 43
"""

    [<Fact>]
    let ``IfThenElse expr with trivia``() =
        Oak() {
            AnonymousModule() {
                Value(
                    "result",
                    IfThenElseExpr(ConstantExpr(Bool(true)), ConstantExpr(Int(1)), ConstantExpr(Int(0)))
                        .triviaBefore(SingleLine("Conditional expression"))
                )
            }
        }
        |> produces
            """
let result =
    // Conditional expression
    if true then 1 else 0
"""

module TypeTriviaTests =

    [<Fact>]
    let ``Type annotation trivia before with single line comment``() =
        Oak() {
            AnonymousModule() {
                Value("x", ConstantExpr(Int(42)), LongIdent("int").triviaBefore(SingleLine("Type comment")))
            }
        }
        |> produces
            """
let x:
    // Type comment
    int = 42
"""

    [<Fact>]
    let ``Type annotation trivia before with block comment``() =
        Oak() {
            AnonymousModule() {
                Value("x", ConstantExpr(Int(42)), LongIdent("int").triviaBefore(BlockComment("Block type comment")))
            }
        }
        |> produces
            """
let x: (*Block type comment*) int = 42
"""

    [<Fact>]
    let ``Type trivia after with line comment``() =
        Oak() {
            AnonymousModule() {
                Value(
                    "x",
                    ConstantExpr(Int(42)),
                    LongIdent("int").triviaAfter(LineCommentAfterSourceCode("After type"))
                )
            }
        }
        |> produces
            """
let x: int // After type
    = 42
"""

    [<Fact>]
    let ``Type trivia before with multiple comments``() =
        Oak() {
            AnonymousModule() {
                Value(
                    "x",
                    ConstantExpr(Int(42)),
                    LongIdent("int").triviaBefore([ SingleLine("Type info"); SingleLine("More info") ])
                )
            }
        }
        |> produces
            """
let x:
    // Type info
    // More info
    int = 42
"""

    [<Fact>]
    let ``Type trivia with TriviaNode``() =
        Oak() {
            AnonymousModule() {
                Value(
                    "x",
                    ConstantExpr(Int(42)),
                    LongIdent("int").triviaBefore(TriviaNode(SingleLine("TriviaNode type comment")))
                )
            }
        }
        |> produces
            """
let x:
    // TriviaNode type comment
    int = 42
"""

module TypeDefnTriviaTests =

    [<Fact>]
    let ``Record type definition trivia before with single line comment``() =
        Oak() {
            AnonymousModule() {
                (Record("Person") { Field("Name", LongIdent("string")) }).triviaBefore(SingleLine("Person record"))
            }
        }
        |> produces
            """
// Person record
type Person = { Name: string }
"""

    [<Fact>]
    let ``Record type definition trivia before with block comment``() =
        Oak() {
            AnonymousModule() {
                (Record("Person") { Field("Name", LongIdent("string")) })
                    .triviaBefore(BlockComment("Block type comment", true, true))
            }
        }
        |> produces
            """
(*
Block type comment
*)
type Person = { Name: string }
"""

    [<Fact>]
    let ``Record type definition trivia after with line comment``() =
        Oak() {
            AnonymousModule() {
                (Record("Person") { Field("Name", LongIdent("string")) })
                    .triviaAfter(LineCommentAfterSourceCode("End of Person"))
            }
        }
        |> produces
            """
type Person = { Name: string } // End of Person
"""

    [<Fact>]
    let ``Class type definition trivia before``() =
        Oak() {
            AnonymousModule() {
                (TypeDefn("MyClass", UnitPat()) { Member(ConstantPat(Constant("this.Value")), ConstantExpr(Int(42))) })
                    .triviaBefore(SingleLine("MyClass definition"))
            }
        }
        |> produces
            """
// MyClass definition
type MyClass() =
    member this.Value = 42
"""

    [<Fact>]
    let ``Union type definition trivia before``() =
        Oak() {
            AnonymousModule() {
                (Union("Shape") {
                    UnionCase("Circle")
                    UnionCase("Rectangle")
                })
                    .triviaBefore(SingleLine("Shape union"))
            }
        }
        |> produces
            """
// Shape union
type Shape =
    | Circle
    | Rectangle
"""

    [<Fact>]
    let ``TypeDefn trivia before with multiple comments``() =
        Oak() {
            AnonymousModule() {
                (Record("Data") { Field("Value", LongIdent("int")) })
                    .triviaBefore([ SingleLine("Data record"); SingleLine("More info") ])
            }
        }
        |> produces
            """
// Data record
// More info
type Data = { Value: int }
"""

    [<Fact>]
    let ``TypeDefn trivia with TriviaNode``() =
        Oak() {
            AnonymousModule() {
                (Record("Item") { Field("Id", LongIdent("int")) })
                    .triviaBefore(TriviaNode(SingleLine("TriviaNode type def comment")))
            }
        }
        |> produces
            """
// TriviaNode type def comment
type Item = { Id: int }
"""

    [<Fact>]
    let ``Enum type definition trivia before``() =
        Oak() {
            AnonymousModule() {
                (Enum("Color") {
                    EnumCase("Red", Int(0))
                    EnumCase("Green", Int(1))
                })
                    .triviaBefore(SingleLine("Color enum"))
            }
        }
        |> produces
            """
// Color enum
type Color =
    | Red = 0
    | Green = 1
"""

module MemberDefnTriviaTests =

    [<Fact>]
    let ``Member trivia before with single line comment``() =
        Oak() {
            AnonymousModule() {
                TypeDefn("MyClass", UnitPat()) {
                    Member(ConstantPat(Constant("this.Value")), ConstantExpr(Int(42)))
                        .triviaBefore(SingleLine("Value member"))
                }
            }
        }
        |> produces
            """
type MyClass() =
    // Value member
    member this.Value = 42
"""

    [<Fact>]
    let ``Member trivia before with block comment``() =
        Oak() {
            AnonymousModule() {
                TypeDefn("MyClass", UnitPat()) {
                    Member(ConstantPat(Constant("this.Value")), ConstantExpr(Int(42)))
                        .triviaBefore(BlockComment("Block member comment"))
                }
            }
        }
        |> produces
            """
type MyClass() =
    (*Block member comment*) member this.Value = 42
"""

    [<Fact>]
    let ``Member trivia after with line comment``() =
        Oak() {
            AnonymousModule() {
                TypeDefn("MyClass", UnitPat()) {
                    Member(ConstantPat(Constant("this.Value")), ConstantExpr(Int(42)))
                        .triviaAfter(LineCommentAfterSourceCode("End of Value"))
                }
            }
        }
        |> produces
            """
type MyClass() =
    member this.Value = 42 // End of Value
"""

    [<Fact>]
    let ``Member trivia before with multiple comments``() =
        Oak() {
            AnonymousModule() {
                TypeDefn("MyClass", UnitPat()) {
                    Member(ConstantPat(Constant("this.Value")), ConstantExpr(Int(42)))
                        .triviaBefore([ SingleLine("Primary value"); SingleLine("Used for calculations") ])
                }
            }
        }
        |> produces
            """
type MyClass() =
    // Primary value
    // Used for calculations
    member this.Value = 42
"""

    [<Fact>]
    let ``Member trivia with TriviaNode``() =
        Oak() {
            AnonymousModule() {
                TypeDefn("MyClass", UnitPat()) {
                    Member(ConstantPat(Constant("this.Value")), ConstantExpr(Int(42)))
                        .triviaBefore(TriviaNode(SingleLine("TriviaNode member comment")))
                }
            }
        }
        |> produces
            """
type MyClass() =
    // TriviaNode member comment
    member this.Value = 42
"""

    [<Fact>]
    let ``Multiple members with trivia``() =
        Oak() {
            AnonymousModule() {
                TypeDefn("MyClass", UnitPat()) {
                    Member(ConstantPat(Constant("this.X")), ConstantExpr(Int(1)))
                        .triviaBefore(SingleLine("X coordinate"))

                    Member(ConstantPat(Constant("this.Y")), ConstantExpr(Int(2)))
                        .triviaBefore(SingleLine("Y coordinate"))
                }
            }
        }
        |> produces
            """
type MyClass() =
    // X coordinate
    member this.X = 1
    // Y coordinate
    member this.Y = 2
"""

module ModuleDeclTriviaTests =

    [<Fact>]
    let ``Module declaration trivia before with single line comment``() =
        Oak() {
            AnonymousModule() {
                (Module("Inner") { Value("x", ConstantExpr(Int(42))) }).triviaBefore(SingleLine("Inner module"))
            }
        }
        |> produces
            """
// Inner module
module Inner =
    let x = 42
"""

    [<Fact>]
    let ``Module declaration trivia before with block comment``() =
        Oak() {
            AnonymousModule() {
                (Module("Inner") { Value("x", ConstantExpr(Int(42))) })
                    .triviaBefore(BlockComment("Block module comment", true, true))
            }
        }
        |> produces
            """
(*
Block module comment
*)
module Inner =
    let x = 42
"""

    [<Fact>]
    let ``Module declaration trivia after with line comment``() =
        Oak() {
            AnonymousModule() {
                (Module("Inner") { Value("x", ConstantExpr(Int(42))) })
                    .triviaAfter(LineCommentAfterSourceCode("End of Inner"))
            }
        }
        |> produces
            """
module Inner =
    let x = 42 // End of Inner
"""

    [<Fact>]
    let ``Module declaration trivia before with multiple comments``() =
        Oak() {
            AnonymousModule() {
                (Module("Utils") { Value("helper", ConstantExpr(Int(1))) })
                    .triviaBefore([ SingleLine("Utility functions"); SingleLine("For internal use") ])
            }
        }
        |> produces
            """
// Utility functions
// For internal use
module Utils =
    let helper = 1
"""

    [<Fact>]
    let ``Module declaration trivia with TriviaNode``() =
        Oak() {
            AnonymousModule() {
                (Module("Core") { Value("myVal", ConstantExpr(Int(0))) })
                    .triviaBefore(TriviaNode(SingleLine("TriviaNode module comment")))
            }
        }
        |> produces
            """
// TriviaNode module comment
module Core =
    let myVal = 0
"""

    [<Fact>]
    let ``Open declaration with trivia``() =
        Oak() { AnonymousModule() { Open("System").triviaBefore(SingleLine("System namespace")) } }
        |> produces
            """
// System namespace
open System
"""

    [<Fact>]
    let ``Value declaration in module with trivia``() =
        Oak() {
            AnonymousModule() {
                Value("config", ConstantExpr(String("default"))).triviaBefore(SingleLine("Configuration value"))
            }
        }
        |> produces
            """
// Configuration value
let config = "default"
"""

    [<Fact>]
    let ``Function declaration in module with trivia``() =
        Oak() {
            AnonymousModule() {
                Function("add", [ ParameterPat("a"); ParameterPat("b") ], InfixAppExpr("a", "+", "b"))
                    .triviaBefore(SingleLine("Adds two numbers"))
            }
        }
        |> produces
            """
// Adds two numbers
let add a b = a + b
"""

    [<Fact>]
    let ``Method with conditional compilation directives in parameters``() =
        let condition =
            "!(ELECTRON_OS_LIN || ELECTRON_OS_WIN || ELECTRON_OS_MAC || ELECTRON_OS_MAS) || ELECTRON_OS_LIN || ELECTRON_OS_WIN"

        Oak() {
            AnonymousModule() {
                TypeDefn("MyType", UnitPat()) {
                    Member(
                        "this.MyMethod",
                        ParenPat(
                            TuplePat(
                                [ ParameterPat(ConstantPat(Constant("?color")), String())
                                      .triviaBefore(Directive($"#if {condition}"))
                                      .triviaAfter(Directive("#endif"))
                                  ParameterPat(ConstantPat(Constant("?symbolColor")), String())
                                      .triviaBefore(Directive($"#if {condition}"))
                                      .triviaAfter(Directive("#endif"))
                                  ParameterPat(ConstantPat(Constant("?height")), Int()) ]
                            )
                        ),
                        ConstantExpr(ConstantUnit())
                    )
                }
            }
        }
        |> produces
            """
type MyType() =
    member this.MyMethod
        (
            #if !(ELECTRON_OS_LIN || ELECTRON_OS_WIN || ELECTRON_OS_MAC || ELECTRON_OS_MAS) || ELECTRON_OS_LIN || ELECTRON_OS_WIN
            ?color: string,
            #endif
            #if !(ELECTRON_OS_LIN || ELECTRON_OS_WIN || ELECTRON_OS_MAC || ELECTRON_OS_MAS) || ELECTRON_OS_LIN || ELECTRON_OS_WIN
            ?symbolColor: string,
            #endif
            ?height: int
        ) =
        ()
"""
