namespace Fabulous.AST.Tests.LetBindings

open Fantomas.FCS.Text
open Fabulous.AST
open Fabulous.AST.Tests

open type Ast
open Fantomas.Core.SyntaxOak
open NUnit.Framework

module Function =

    [<Test>]
    let ``Produces a function with parameters`` () =
        let thenExpr =
            Expr.Constant(
                Constant.Unit(UnitNode(SingleTextNode("(", Range.Zero), SingleTextNode(")", Range.Zero), Range.Zero))
            )

        AnonymousModule() { Function("x", [ Pattern.CreateNamed("i") ]) { EscapeHatch(thenExpr) } }
        |> produces
            """

let x i = ()

"""

    [<Test>]
    let ``Produces a function with parameters and an attribute`` () =
        let thenExpr =
            Expr.Constant(
                Constant.Unit(UnitNode(SingleTextNode("(", Range.Zero), SingleTextNode(")", Range.Zero), Range.Zero))
            )

        AnonymousModule() {
            (Function("x", [ Pattern.CreateNamed("i") ]) { EscapeHatch(thenExpr) })
                .attributes([ """Obsolete("Use bar instead")""" ])
        }
        |> produces
            """
[<Obsolete("Use bar instead")>]
let x i = ()

"""

    [<Test>]
    let ``Produces a function with parameters and Xml Doc`` () =
        let thenExpr =
            Expr.Constant(
                Constant.Unit(UnitNode(SingleTextNode("(", Range.Zero), SingleTextNode(")", Range.Zero), Range.Zero))
            )

        AnonymousModule() {
            (Function("x", [ Pattern.CreateNamed("i") ]) { EscapeHatch(thenExpr) })
                .xmlDocs([ "Im a function" ])
        }
        |> produces
            """
/// Im a function
let x i = ()

"""

    [<Test>]
    let ``Produces a function with parameters and return type`` () =
        let thenExpr =
            Expr.Constant(
                Constant.Unit(UnitNode(SingleTextNode("(", Range.Zero), SingleTextNode(")", Range.Zero), Range.Zero))
            )

        AnonymousModule() {
            (Function("x", [ Pattern.CreateNamed("i") ]) { EscapeHatch(thenExpr) })
                .returnType(CommonType.Unit)
        }
        |> produces
            """
let x i : unit = ()

"""

    [<Test>]
    let ``Produces a function with parameters, return type and typeParams `` () =
        let thenExpr =
            Expr.Constant(
                Constant.Unit(UnitNode(SingleTextNode("(", Range.Zero), SingleTextNode(")", Range.Zero), Range.Zero))
            )

        AnonymousModule() {
            (Function("foo", []) { EscapeHatch(thenExpr) })
                .returnType(CommonType.Unit)
                .typeParams(TyparDecls.Prefix([ "x:'T"; " i:'U" ]))
        }
        |> produces
            """
let foo(x:'T,  i:'U) : unit = ()

"""

    [<Test>]
    let ``Produces an inlined function with parameters`` () =
        let thenExpr =
            Expr.Constant(
                Constant.Unit(UnitNode(SingleTextNode("(", Range.Zero), SingleTextNode(")", Range.Zero), Range.Zero))
            )

        AnonymousModule() { InlinedFunction("x", [ Pattern.CreateNamed("i") ]) { EscapeHatch(thenExpr) } }
        |> produces
            """

let inline x i = ()

"""

    [<Test>]
    let ``Produces a function with parameters and access controls`` () =
        let thenExpr =
            Expr.Constant(
                Constant.Unit(UnitNode(SingleTextNode("(", Range.Zero), SingleTextNode(")", Range.Zero), Range.Zero))
            )

        AnonymousModule() {
            (Function("x", [ Pattern.CreateNamed("i") ]) { EscapeHatch(thenExpr) })
                .toPublic()

            (Function("y", [ Pattern.CreateNamed("i") ]) { EscapeHatch(thenExpr) })
                .toPrivate()

            (Function("z", [ Pattern.CreateNamed("i") ]) { EscapeHatch(thenExpr) })
                .toInternal()
        }
        |> produces
            """

let public x i = ()
let private y i = ()
let internal z i = ()

"""
