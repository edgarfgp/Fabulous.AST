namespace Fabulous.AST.Tests.ModuleDeclarations.TopLevelBindings

open Fantomas.FCS.Text
open Fantomas.Core.SyntaxOak
open Xunit
open Fabulous.AST.Tests

open Fabulous.AST

open type Ast

module Value =

    [<Theory>]
    [<InlineData("Red Blue", "``Red Blue``")>]
    [<InlineData("Red_Blue", "Red_Blue")>]
    [<InlineData(" Red Blue ", "``Red Blue``")>]
    [<InlineData("net6.0", "``net6.0``")>]
    [<InlineData(" net6.0 ", "``net6.0``")>]
    [<InlineData("class", "``class``")>]
    [<InlineData("2013", "``2013``")>]
    let ``Produces an union with fields with backticks`` (value: string) (expected: string) =
        Oak() { AnonymousModule() { Value(value, Unquoted "12") } }
        |> produces
            $$"""

let {{expected}} = 12
"""

    [<Fact>]
    let ``Simple Let binding``() =
        Oak() { AnonymousModule() { Value("x", Unquoted "12") } }
        |> produces
            """

let x = 12

"""

    [<Fact>]
    let ``Simple Let binding with an array expression``() =

        let subcommands = [| "ControlFlow"; "Core"; "Expressions"; "LetBindings" |]

        Oak() {
            Namespace("Gdmt.Launcher") {
                NestedModule("Subcommands") {
                    Value(
                        "GdmtSubcommands",
                        ArrayExpr() {
                            for subcommand in subcommands do
                                ConstantExpr(Quoted subcommand)
                        }
                    )
                }
            }
        }

        |> produces
            """

namespace Gdmt.Launcher

module Subcommands =
    let GdmtSubcommands = [| "ControlFlow"; "Core"; "Expressions"; "LetBindings" |]

"""

    [<Fact>]
    let ``Simple value with tuple pattern``() =
        Oak() {
            AnonymousModule() {
                Value(
                    TuplePat() {
                        NamedPat("x")
                        NamedPat("y")
                        NamedPat("z")
                    },
                    TupleExpr() {
                        ConstantExpr(Unquoted "1")
                        ConstantExpr(Unquoted "2")
                        ConstantExpr(Unquoted "3")
                    }
                )
            }
        }
        |> produces
            """

let x, y, z = 1, 2, 3

"""

    [<Fact>]
    let ``Simple Let binding with return type``() =
        Oak() {
            AnonymousModule() {
                Value("x", Unquoted "12").returnType(Int32())
                Value("z", TripleQuoted("12"))
            }
        }
        |> produces
            "
let x: int = 12
let z = \"\"\"12\"\"\"
"

    [<Fact>]
    let ``Simple Let binding with return widget type``() =
        Oak() {
            AnonymousModule() {
                Value("x", Unquoted "12").returnType("int")
                Value("y", Unquoted "12").returnType(LongIdent("int"))

                Value("z", Unquoted "12")
                    .returnType(Funs(LongIdent("string")) { LongIdent("int") })

                Value("a", Unquoted "12").returnType(Funs("string") { LongIdent("int") })

                Value("b", Unquoted "12").returnType("string -> int")

                Value("c", Unquoted "12").returnType(HashConstraint(LongIdent("int")))
            }
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

    [<Fact>]
    let ``Simple Let binding with an expression``() =
        Oak() { AnonymousModule() { Value("x", ConstantExpr(Unquoted "12")) } }
        |> produces
            """

let x = 12

"""

    [<Fact>]
    let ``Simple Let binding with type params Postfix``() =
        Oak() {
            AnonymousModule() {
                Value("x", Unquoted "12").typeParams([ "'T" ])
                Value("x", Unquoted "12").typeParams([ "'a"; "'b"; "'c" ])
            }
        }
        |> produces
            """

let x<'T> = 12
let x<'a, 'b, 'c> = 12

"""

    [<Fact>]
    let ``Simple Let binding with type params Prefix``() =
        Oak() {
            AnonymousModule() {
                Value("x", Unquoted "12").typeParams([ "'T" ])
                Value("x", Unquoted "12").typeParams([ "'a"; "'b"; "'c" ])
            }
        }
        |> produces
            """
let x<'T> = 12
let x<'a, 'b, 'c> = 12
"""

    [<Fact>]
    let ``Simple Let binding with type params SinglePrefix``() =
        Oak() {
            AnonymousModule() {
                Value("x", ConstantExpr(Unquoted "12")).typeParams([ "'T" ])

                Value("x", Unquoted "12").typeParams([ "'T" ])
            }
        }
        |> produces
            """
let x<'T> = 12
let x<'T> = 12
"""

    [<Fact>]
    let ``Simple Let private binding``() =
        Oak() { AnonymousModule() { Value("x", Unquoted "12").toPrivate() } }
        |> produces
            """

let private x = 12

"""

    [<Fact>]
    let ``Simple Let internal binding``() =
        Oak() { AnonymousModule() { Value("x", Unquoted "12").toInternal() } }
        |> produces
            """

let internal x = 12

"""

    [<Fact>]
    let ``Simple Let binding with a single xml doc``() =
        Oak() { AnonymousModule() { Value("x", Unquoted "12").xmlDocs([ "This is a comment" ]) } }
        |> produces
            """

/// This is a comment
let x = 12

"""

    [<Fact>]
    let ``Simple Let binding with multiline xml doc``() =
        Oak() {
            AnonymousModule() {
                Value("x", Unquoted "12")

                    .xmlDocs(
                        [ "This is a fist comment"
                          "This is a second comment"
                          "This is a third comment" ]
                    )

            }
        }
        |> produces
            """

/// This is a fist comment
/// This is a second comment
/// This is a third comment
let x = 12

"""

    [<Fact>]
    let ``Simple Let binding with multiline with a single attribute``() =
        Oak() {
            AnonymousModule() {
                Value("x", Unquoted "12").attribute("Obsolete")

            }
        }
        |> produces
            """
[<Obsolete>]
let x = 12

"""

    [<Fact>]
    let ``Simple Let binding with multiline with a multiple attributes``() =
        Oak() {
            AnonymousModule() {
                Value("x", Unquoted "12").attributes() {
                    Attribute("EditorBrowsable")
                    Attribute("Obsolete")
                }

            }
        }
        |> produces
            """

[<EditorBrowsable; Obsolete>]
let x = 12

"""

    [<Fact>]
    let ``Simple Let binding with escape hatch``() =
        Oak() {
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
        }
        |> produces
            """

let x = 12

"""

    [<Fact>]
    let ``Produces a top level let binding from BindingNode(using Widgets)``() =
        Oak() {
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
        }
        |> produces
            """

let x = 12

"""

    [<Fact>]
    let ``Produces a top level mutable let binding``() =
        Oak() { AnonymousModule() { Value("x", Unquoted "12").toMutable() } }
        |> produces
            """

let mutable x = 12

"""

    [<Fact>]
    let ``Produces a top level mutable let binding with return type``() =
        Oak() { AnonymousModule() { Value("x", Unquoted "12").returnType(Int32()).toMutable() } }
        |> produces
            """

let mutable x: int = 12

"""

    [<Fact>]
    let ``Produces a top level mutable let binding with an expression``() =
        Oak() { AnonymousModule() { Value("x", ConstantExpr(Unquoted "12")).toMutable() } }
        |> produces
            """

let mutable x = 12

"""

    [<Fact>]
    let ``Produces a top level inlined let binding with type params and an expression``() =
        Oak() { AnonymousModule() { Value("x", ConstantExpr(Unquoted "12")).toInlined().typeParams([ "'a" ]) } }
        |> produces
            """

let inline x<'a> = 12

"""

    [<Fact>]
    let ``Produces let binding with AppLongIdentAndSingleParenArg expression``() =
        Oak() {
            AnonymousModule() {
                Value("res", AppLongIdentAndSingleParenArgExpr([ "conn"; "Open" ], ConstantExpr(ConstantUnit())))
                Value("res2", AppLongIdentAndSingleParenArgExpr([ "conn"; "Open" ], "()"))
                Value("res3", AppLongIdentAndSingleParenArgExpr("conn.Open", "()"))
            }
        }
        |> produces
            """

let res = conn.Open()
let res2 = conn.Open ()
let res3 = conn.Open ()

"""
