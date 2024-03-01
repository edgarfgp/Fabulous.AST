namespace Fabulous.AST.Tests.ModuleDeclarations.TopLevelBindings

open Fantomas.FCS.Text
open Fantomas.Core.SyntaxOak
open NUnit.Framework
open Fabulous.AST.Tests

open Fabulous.AST

open type Ast

module Value =
    [<Test>]
    let ``Simple Let binding`` () =
        AnonymousModule() { Value("x", "12") }
        |> produces
            """

let x = 12

"""

    [<Test>]
    let ``Simple Let binding with an array expression`` () =

        let subcommands = [| "ControlFlow"; "Core"; "Expressions"; "LetBindings" |]

        Namespace("Gdmt.Launcher") {
            NestedModule("Subcommands") {
                Value(
                    "GdmtSubcommands",
                    ArrayExpr() {
                        for subcommand in subcommands do
                            ConstantExpr(ConstantString($"\"{subcommand}\""))
                    }
                )
            }
        }

        |> produces
            """

namespace Gdmt.Launcher

module Subcommands =
    let GdmtSubcommands = [| "ControlFlow"; "Core"; "Expressions"; "LetBindings" |]

"""

    [<Test>]
    let ``Simple value with tuple pattern`` () =
        AnonymousModule() {
            Value(
                TuplePat() {
                    NamedPat("x")
                    NamedPat("y")
                    NamedPat("z")
                },
                TupleExpr() {
                    ConstantExpr(ConstantString "1")
                    ConstantExpr(ConstantString "2")
                    ConstantExpr(ConstantString "3")
                }
            )
        }
        |> produces
            """

let x, y, z = 1, 2, 3

"""

    [<Test>]
    let ``Simple Let binding with return type`` () =
        AnonymousModule() { Value("x", "12").returnType(CommonType.Int32) }
        |> produces
            """

let x: int = 12

"""

    [<Test>]
    let ``Simple Let binding with return widget type`` () =
        AnonymousModule() {
            Value("x", "12").returnType("int")
            Value("y", "12").returnType(TypeLongIdent("int"))

            Value("z", "12")
                .returnType(TypeFuns(TypeLongIdent("string")) { TypeLongIdent("int") })

            Value("a", "12")
                .returnType(TypeFuns("string") { TypeLongIdent("int") })

            Value("b", "12").returnType("string -> int")

            Value("c", "12")
                .returnType(TypeHashConstraint(TypeLongIdent("int")))
        }
        |> produces
            """

let x: int = 12
let y: int = 12
let z: int -> string = 12
let a: int -> string = 12
let b: string -> int = 12
let c: #int = 12

"""

    [<Test>]
    let ``Simple Let binding with an expression`` () =
        AnonymousModule() { Value("x", ConstantExpr(ConstantString "12")) }
        |> produces
            """

let x = 12

"""

    [<Test>]
    let ``Simple Let binding with type params Postfix`` () =
        AnonymousModule() {
            Value("x", [ "'T" ], "12")
            Value("x", [ "'a"; "'b"; "'c" ], "12")
        }
        |> produces
            """

let x<'T> = 12
let x<'a, 'b, 'c> = 12

"""

    [<Test>]
    let ``Simple Let binding with type params Prefix`` () =
        AnonymousModule() {
            Value("x", [ "'T" ], "12")
            Value("x", [ "'a"; "'b"; "'c" ], "12")
        }
        |> produces
            """
let x<'T> = 12
let x<'a, 'b, 'c> = 12
"""

    [<Test>]
    let ``Simple Let binding with type params SinglePrefix`` () =
        AnonymousModule() {
            Value("x", [ "'T" ], ConstantExpr(ConstantString "12"))

            Value("x", [ "'T" ], "12")
        }
        |> produces
            """
let x<'T> = 12
let x<'T> = 12
"""

    [<Test>]
    let ``Simple Let private binding`` () =
        AnonymousModule() { Value("x", "12").toPrivate() }
        |> produces
            """

let private x = 12

"""

    [<Test>]
    let ``Simple Let internal binding`` () =
        AnonymousModule() { Value("x", "12").toInternal() }
        |> produces
            """

let internal x = 12

"""

    [<Test>]
    let ``Simple Let binding with a single xml doc`` () =
        AnonymousModule() { Value("x", "12").xmlDocs([ "This is a comment" ]) }
        |> produces
            """

/// This is a comment
let x = 12

"""

    [<Test>]
    let ``Simple Let binding with multiline xml doc`` () =
        AnonymousModule() {
            Value("x", "12")
                .xmlDocs(
                    [ "This is a fist comment"
                      "This is a second comment"
                      "This is a third comment" ]
                )

        }
        |> produces
            """

/// This is a fist comment
/// This is a second comment
/// This is a third comment
let x = 12

"""

    [<Test>]
    let ``Simple Let binding with multiline with a single attribute`` () =
        AnonymousModule() {
            Value("x", "12").attribute("Obsolete")

        }
        |> produces
            """
[<Obsolete>]
let x = 12

"""

    [<Test>]
    let ``Simple Let binding with multiline with a multiple attributes`` () =
        AnonymousModule() {
            Value("x", "12").attributes() {
                Attribute("EditorBrowsable")
                Attribute("Obsolete")
            }

        }
        |> produces
            """
            
[<EditorBrowsable; Obsolete>]
let x = 12

"""

    [<Test>]
    let ``Simple Let binding with escape hatch`` () =
        AnonymousModule() {
            BindingNode(
                None,
                None,
                MultipleTextsNode([ SingleTextNode("let", Range.Zero) ], Range.Zero),
                false,
                None,
                None,
                Choice1Of2(IdentListNode([ IdentifierOrDot.Ident(SingleTextNode("x", Range.Zero)) ], Range.Zero)),
                None,
                List.Empty,
                None,
                SingleTextNode("=", Range.Zero),
                Expr.Constant(Constant.FromText(SingleTextNode("12", Range.Zero))),
                Range.Zero
            )
        }
        |> produces
            """
        
let x = 12

"""

    [<Test>]
    let ``Produces a top level let binding from BindingNode(using Widgets)`` () =
        AnonymousModule() {
            BindingNode(
                None,
                None,
                MultipleTextsNode([ SingleTextNode("let", Range.Zero) ], Range.Zero),
                false,
                None,
                None,
                Choice1Of2(IdentListNode([ IdentifierOrDot.Ident(SingleTextNode("x", Range.Zero)) ], Range.Zero)),
                None,
                List.Empty,
                None,
                SingleTextNode("=", Range.Zero),
                Expr.Constant(Constant.FromText(SingleTextNode("12", Range.Zero))),
                Range.Zero
            )
        }
        |> produces
            """
        
let x = 12

"""

    [<Test>]
    let ``Produces a top level mutable let binding`` () =
        AnonymousModule() { Value("x", "12").toMutable() }
        |> produces
            """
        
let mutable x = 12

"""

    [<Test>]
    let ``Produces a top level mutable let binding with return type`` () =
        AnonymousModule() { Value("x", "12").returnType(CommonType.Int32).toMutable() }
        |> produces
            """
        
let mutable x: int = 12

"""

    [<Test>]
    let ``Produces a top level mutable let binding with an expression`` () =
        AnonymousModule() { Value("x", ConstantExpr(ConstantString "12")).toMutable() }
        |> produces
            """
        
let mutable x = 12

"""

    [<Test>]
    let ``Produces a top level inlined let binding with type params and an expression`` () =
        AnonymousModule() {
            Value("x", [ "'a" ], ConstantExpr(ConstantString "12"))
                .toInlined()
        }
        |> produces
            """
        
let inline x<'a> = 12

"""
