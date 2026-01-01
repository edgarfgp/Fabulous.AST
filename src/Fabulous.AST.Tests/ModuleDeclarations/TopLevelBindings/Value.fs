namespace Fabulous.AST.Tests.ModuleDeclarations.TopLevelBindings

open Fabulous.AST
open Fabulous.AST.Tests
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text
open Xunit

open type Ast

module Value =

    [<Theory>]
    [<InlineData("Red Blue", "``Red Blue``")>]
    [<InlineData("Red_Blue", "Red_Blue")>]
    [<InlineData(" Red Blue ", "`` Red Blue ``")>]
    [<InlineData("net6.0", "``net6.0``")>]
    [<InlineData(" net6.0 ", "`` net6.0 ``")>]
    [<InlineData("class", "``class``")>]
    [<InlineData("2013", "``2013``")>]
    [<InlineData("some value", "``some value``")>]
    let ``Produces a let binding with auto-escaped identifier`` (value: string) (expected: string) =
        Oak() { AnonymousModule() { Value(value, ConstantExpr(Int(12))) } }
        |> produces
            $$"""

let {{expected}} = 12
"""

    [<Fact>]
    let ``Value with string name and constant auto-escapes identifiers``() =
        Oak() { AnonymousModule() { Value("some value", Int(42)) } }
        |> produces
            """

let ``some value`` = 42

"""

    [<Fact>]
    let ``Simple Let binding``() =
        Oak() {
            AnonymousModule() {
                Value(ConstantPat(Constant("x")), ConstantExpr(Int(12)))
                Value(Constant("x"), Int(12))
                Use(ConstantPat(Constant("x")), ConstantExpr(Int(12)))
                Use(ConstantPat(Constant("x")), Int(12))
            }
        }
        |> produces
            """

let x = 12
let x = 12
use x = 12
use x = 12

"""

    [<Fact>]
    let ``Simple Let binding with an array expression``() =

        let subcommands = [| "ControlFlow"; "Core"; "Expressions"; "LetBindings" |]

        Oak() {
            Namespace("Gdmt.Launcher") {
                Module("Subcommands") {
                    Value(
                        ConstantPat(Constant("GdmtSubcommands")),
                        ArrayExpr(
                            [ for subcommand in subcommands do
                                  ConstantExpr(String subcommand) ]
                        )
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
                    TuplePat([ NamedPat("x"); NamedPat("y"); NamedPat("z") ]),
                    TupleExpr([ ConstantExpr(Int 1); ConstantExpr(Int 2); ConstantExpr(Int 3) ])
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
                Value(ConstantPat(Constant("x")), ConstantExpr(Int(12)), Int())

                Value(ConstantPat(Constant("y")), ConstantExpr(VerbatimString(Int(12))))
                Value(ConstantPat(Constant("z")), ConstantExpr(TripleQuotedString(Int(12))))
            }
        }
        |> produces
            "
let x: int = 12
let y = @\"12\"
let z = \"\"\"12\"\"\"
"

    [<Fact>]
    let ``Simple Let binding with return widget type``() =
        Oak() {
            AnonymousModule() {
                Value(ConstantPat(Constant("x")), ConstantExpr(Int(12)), Int())

                Value(ConstantPat(Constant("y")), ConstantExpr(Int(12)), LongIdent("int"))

                Value(
                    ConstantPat(Constant("z")),
                    ConstantExpr(Int(12)),
                    returnType = Funs([ LongIdent("int") ], LongIdent("string"))
                )

                Value(ConstantPat(Constant("a")), ConstantExpr(Int(12)), Funs([ LongIdent("int") ], LongIdent "string"))

                Value(ConstantPat(Constant("b")), ConstantExpr(Int(12)), LongIdent "string -> int")

                Value(ConstantPat(Constant("c")), ConstantExpr(Int(12)), HashConstraint(LongIdent("int")))
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
        Oak() { AnonymousModule() { Value(ConstantPat(Constant("x")), ConstantExpr(Int(12))) } }
        |> produces
            """

let x = 12

"""

    [<Fact>]
    let ``Simple Let binding with type params Postfix``() =
        Oak() {
            AnonymousModule() {
                Value(ConstantPat(Constant("x")), ConstantExpr(Int(12))).typeParams(PostfixList(TyparDecl("'T")))

                Value(ConstantPat(Constant("x")), ConstantExpr(Int(12)))
                    .typeParams(PostfixList([ TyparDecl("'a"); TyparDecl("'b"); TyparDecl("'c") ]))
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
                Value(ConstantPat(Constant("x")), ConstantExpr(Int(12))).typeParams(PostfixList(TyparDecl("'T")))

                Value(ConstantPat(Constant("x")), ConstantExpr(Int(12)))
                    .typeParams(PostfixList([ TyparDecl("'a"); TyparDecl("'b"); TyparDecl("'c") ]))

                Value(ConstantPat(Constant("x")), ConstantExpr(Int(12))).typeParams(PostfixList([ "'a"; "'b"; "'c" ]))
            }
        }
        |> produces
            """
let x<'T> = 12
let x<'a, 'b, 'c> = 12
let x<'a, 'b, 'c> = 12
"""

    [<Fact>]
    let ``Simple Let private binding``() =
        Oak() { AnonymousModule() { Value(ConstantPat(Constant("x")), ConstantExpr(Int(12))).toPrivate() } }
        |> produces
            """

let private x = 12

"""

    [<Fact>]
    let ``Simple Let internal binding``() =
        Oak() { AnonymousModule() { Value(ConstantPat(Constant("x")), ConstantExpr(Int(12))).toInternal() } }
        |> produces
            """

let internal x = 12

"""

    [<Fact>]
    let ``Simple Let binding with a single xml doc``() =
        Oak() {
            AnonymousModule() {
                Value(ConstantPat(Constant("x")), ConstantExpr(Int(12))).xmlDocs([ "This is a comment" ])
            }
        }
        |> produces
            """

/// This is a comment
let x = 12

"""

    [<Fact>]
    let ``Simple Let binding with multiline xml doc``() =
        Oak() {
            AnonymousModule() {
                Value(ConstantPat(Constant("x")), ConstantExpr(Int(12)))
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
                Value(ConstantPat(Constant("x")), ConstantExpr(Int(12))).attribute(Attribute "Obsolete")

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
                Value(ConstantPat(Constant("x")), ConstantExpr(Int(12)))
                    .attributes([ Attribute("EditorBrowsable"); Attribute("Obsolete") ])

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
        Oak() { AnonymousModule() { Value(ConstantPat(Constant("x")), ConstantExpr(Int(12))).toMutable() } }
        |> produces
            """

let mutable x = 12

"""

    [<Fact>]
    let ``Produces a top level mutable let binding with return type``() =
        Oak() { AnonymousModule() { Value(ConstantPat(Constant("x")), ConstantExpr(Int(12)), Int()).toMutable() } }
        |> produces
            """

let mutable x: int = 12

"""

    [<Fact>]
    let ``Produces a top level mutable let binding with an expression``() =
        Oak() { AnonymousModule() { Value(ConstantPat(Constant("x")), ConstantExpr(Int(12))).toMutable() } }
        |> produces
            """

let mutable x = 12

"""

    [<Fact>]
    let ``Produces a top level inlined let binding with type params and an expression``() =
        Oak() {
            AnonymousModule() {
                Value(ConstantPat(Constant("x")), ConstantExpr(Int(12)))
                    .toInlined()
                    .typeParams(PostfixList(TyparDecl("'a")))
            }
        }
        |> produces
            """

let inline x<'a> = 12

"""

    [<Fact>]
    let ``Produces let binding with AppLongIdentAndSingleParenArg expression``() =
        Oak() {
            AnonymousModule() {
                Value(
                    ConstantPat(Constant("res")),
                    AppLongIdentAndSingleParenArgExpr([ "conn"; "Open" ], ConstantExpr(ConstantUnit()))
                )

                Value(
                    ConstantPat(Constant("res2")),
                    AppLongIdentAndSingleParenArgExpr([ "conn"; "Open" ], ConstantExpr(Constant("()")))
                )

                Value(
                    ConstantPat(Constant("res3")),
                    AppLongIdentAndSingleParenArgExpr("conn.Open", ConstantExpr(Constant("()")))
                )

                Value(
                    ConstantPat(Constant("res4")),
                    AppLongIdentAndSingleParenArgExpr([ "conn"; "Open" ], ConstantUnit())
                )

                Value(ConstantPat(Constant("res5")), AppLongIdentAndSingleParenArgExpr([ "conn"; "Open" ], "()"))
            }
        }
        |> produces
            """

let res = conn.Open()
let res2 = conn.Open ()
let res3 = conn.Open ()
let res4 = conn.Open()
let res5 = conn.Open ()
"""

    [<Fact>]
    let ``yield! multiple values``() =
        Oak() {
            AnonymousModule() {
                yield!
                    [ Value(ConstantPat(Constant("x")), ConstantExpr(Int(1)))
                      Value(ConstantPat(Constant("y")), ConstantExpr(Int(2)))
                      Value(ConstantPat(Constant("z")), ConstantExpr(Int(3))) ]
            }
        }
        |> produces
            """
let x = 1
let y = 2
let z = 3
"""
