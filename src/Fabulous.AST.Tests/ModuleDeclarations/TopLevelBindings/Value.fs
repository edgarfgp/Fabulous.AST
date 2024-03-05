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
        AnonymousModule() { Value("x", "12", false) }
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
                            ConstantExpr($"{subcommand}")
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
                    ConstantExpr("1")
                    ConstantExpr("2")
                    ConstantExpr("3")
                }
            )
        }
        |> produces
            """

let x, y, z = 1, 2, 3

"""

    [<Test>]
    let ``Simple Let binding with return type`` () =
        AnonymousModule() { Value("x", "12", false).returnType(Int32()) }
        |> produces
            """

let x: int = 12

"""

    [<Test>]
    let ``Simple Let binding with return widget type`` () =
        AnonymousModule() {
            Value("x", "12", false).returnType("int")
            Value("y", "12", false).returnType(TypeLongIdent("int"))

            Value("z", "12", false)
                .returnType(TypeFuns(TypeLongIdent("string")) { TypeLongIdent("int") })

            Value("a", "12", false)
                .returnType(TypeFuns("string") { TypeLongIdent("int") })

            Value("b", "12", false).returnType("string -> int")

            Value("c", "12", false)
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
        AnonymousModule() { Value("x", ConstantExpr("12")) }
        |> produces
            """

let x = 12

"""

    [<Test>]
    let ``Simple Let binding with type params Postfix`` () =
        AnonymousModule() {
            Value("x", [ "'T" ], "12", false)
            Value("x", [ "'a"; "'b"; "'c" ], "12", false)
        }
        |> produces
            """

let x<'T> = 12
let x<'a, 'b, 'c> = 12

"""

    [<Test>]
    let ``Simple Let binding with type params Prefix`` () =
        AnonymousModule() {
            Value("x", [ "'T" ], "12", false)
            Value("x", [ "'a"; "'b"; "'c" ], "12", false)
        }
        |> produces
            """
let x<'T> = 12
let x<'a, 'b, 'c> = 12
"""

    [<Test>]
    let ``Simple Let binding with type params SinglePrefix`` () =
        AnonymousModule() {
            Value("x", [ "'T" ], ConstantExpr("12"))

            Value("x", [ "'T" ], "12", false)
        }
        |> produces
            """
let x<'T> = 12
let x<'T> = 12
"""

    [<Test>]
    let ``Simple Let private binding`` () =
        AnonymousModule() { Value("x", "12", false).toPrivate() }
        |> produces
            """

let private x = 12

"""

    [<Test>]
    let ``Simple Let internal binding`` () =
        AnonymousModule() { Value("x", "12", false).toInternal() }
        |> produces
            """

let internal x = 12

"""

    [<Test>]
    let ``Simple Let binding with a single xml doc`` () =
        AnonymousModule() { Value("x", "12", false).xmlDocs([ "This is a comment" ]) }
        |> produces
            """

/// This is a comment
let x = 12

"""

    [<Test>]
    let ``Simple Let binding with multiline xml doc`` () =
        AnonymousModule() {
            Value("x", "12", false)
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
            Value("x", "12", false).attribute("Obsolete")

        }
        |> produces
            """
[<Obsolete>]
let x = 12

"""

    [<Test>]
    let ``Simple Let binding with multiline with a multiple attributes`` () =
        AnonymousModule() {
            Value("x", "12", false).attributes() {
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
        AnonymousModule() { Value("x", "12", false).toMutable() }
        |> produces
            """
        
let mutable x = 12

"""

    [<Test>]
    let ``Produces a top level mutable let binding with return type`` () =
        AnonymousModule() { Value("x", "12", false).returnType(Int32()).toMutable() }
        |> produces
            """
        
let mutable x: int = 12

"""

    [<Test>]
    let ``Produces a top level mutable let binding with an expression`` () =
        AnonymousModule() { Value("x", ConstantExpr("12")).toMutable() }
        |> produces
            """
        
let mutable x = 12

"""

    [<Test>]
    let ``Produces a top level inlined let binding with type params and an expression`` () =
        AnonymousModule() { Value("x", [ "'a" ], ConstantExpr("12")).toInlined() }
        |> produces
            """
        
let inline x<'a> = 12

"""
