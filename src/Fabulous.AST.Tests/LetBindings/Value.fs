namespace Fabulous.AST.Tests.LetBindings

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
    let ``Simple Let binding with an expression`` () =
        AnonymousModule() { Value("x", Expr.Constant(Constant.Text("12"))) }
        |> produces
            """

let x = 12

"""

    [<Test>]
    let ``Simple Let binding inlined`` () =
        AnonymousModule() { InlinedValue("x", "12") }
        |> produces
            """

let inline x = 12

"""

    [<Test>]
    let ``Simple Let binding inlined with an expression`` () =
        AnonymousModule() { InlinedValue("x", Expr.Constant(Constant.Text("12"))) }
        |> produces
            """

let inline x = 12

"""


    [<Test>]
    let ``Simple Let private binding`` () =
        AnonymousModule() { Value("x", "12").accessibility(AccessControl.Private) }
        |> produces
            """

let private x = 12

"""

    [<Test>]
    let ``Simple Let internal binding`` () =
        AnonymousModule() { Value("x", "12").accessibility(AccessControl.Internal) }
        |> produces
            """

let internal x = 12

"""

    [<Test>]
    let ``Simple Let binding with a single xml doc`` () =
        AnonymousModule() { Value("x", "12").xmlDocs([ "/// This is a comment" ]) }
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
                    [ "/// This is a fist comment"
                      "/// This is a second comment"
                      "/// This is a third comment" ]
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
            Value("x", "12").attributes([ "Obsolete" ])

        }
        |> produces
            """
[<Obsolete>]
let x = 12

"""

    [<Test>]
    let ``Simple Let binding with multiline with a multiple attributes`` () =
        AnonymousModule() {
            Value("x", "12")
                .attributes([ "EditorBrowsable"; "Obsolete" ])
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
        AnonymousModule() { MutableValue("x", "12") }
        |> produces
            """
        
let mutable x = 12

"""

    [<Test>]
    let ``Produces a top level mutable let binding with an expression`` () =
        AnonymousModule() { MutableValue("x", Expr.Constant(Constant.Text("12"))) }
        |> produces
            """
        
let mutable x = 12

"""
