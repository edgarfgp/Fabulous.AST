namespace Fabulous.AST.Tests.LetBindings

open Fabulous.AST
open Fabulous.AST.Tests

open type Ast
open Fantomas.Core.SyntaxOak
open NUnit.Framework

module Function =

    [<Test>]
    let ``Produces a function with parameter`` () =
        AnonymousModule() { Function("x", Parameters() { Named("i") }) { UnitExpr() } }
        |> produces
            """

let x i = ()

"""

    [<Test>]
    let ``Produces a function with parameters`` () =
        AnonymousModule() {
            Function(
                "x",
                Parameters() {
                    Named("i")
                    Named("j")
                    Named("k")
                }
            ) {
                UnitExpr()
            }
        }
        |> produces
            """
let x i j k = ()

"""

    [<Test>]
    let ``Produces a function with single tupled parameter`` () =
        AnonymousModule() { Function("x", Parameters(true) { Named("i") }) { UnitExpr() } }
        |> produces
            """
let x i = ()

"""

    [<Test>]
    let ``Produces a function with single tupled typed parameter`` () =
        AnonymousModule() { Function("x", Parameters(true) { Parameter(Named("i"), CommonType.Int32) }) { UnitExpr() } }
        |> produces
            """
let x (i: int) = ()

"""

    [<Test>]
    let ``Produces a function with tupled parameters`` () =
        AnonymousModule() {
            Function(
                "x",
                Parameters(true) {
                    Named("i")
                    Named("j")
                    Named("k")
                }
            ) {
                UnitExpr()
            }
        }
        |> produces
            """
let x (i, j, k) = ()

"""

    [<Test>]
    let ``Produces a function with tupled typed parameters`` () =
        AnonymousModule() {
            Function(
                "x",
                Parameters(true) {
                    Parameter(Named("i"), CommonType.Int32)
                    Parameter(Named("j"), CommonType.String)
                    Parameter(Named("k"), CommonType.Boolean)
                }
            ) {
                UnitExpr()
            }
        }
        |> produces
            """
let x (i: int, j: string, k: bool) = ()

"""

    [<Test>]
    let ``Produces a function with parameters and an attribute`` () =
        AnonymousModule() {
            (Function("x", Parameters() { Named("i") }) { UnitExpr() })
                .attributes([ """Obsolete("Use bar instead")""" ])
        }
        |> produces
            """
[<Obsolete("Use bar instead")>]
let x i = ()

"""

    [<Test>]
    let ``Produces a function with parameters and Xml Doc`` () =
        AnonymousModule() {
            (Function("x", Parameters() { Named("i") }) { UnitExpr() })
                .xmlDocs([ "Im a function" ])
        }
        |> produces
            """
/// Im a function
let x i = ()

"""

    [<Test>]
    let ``Produces a function with parameters and return type`` () =
        AnonymousModule() {
            (Function("x", Parameters() { Named("i") }) { UnitExpr() })
                .returnType(CommonType.Unit)
        }
        |> produces
            """
let x i : unit = ()

"""

    [<Test>]
    let ``Produces a function with parameters, return type and typeParams `` () =
        AnonymousModule() {
            (Function(
                "foo",
                Parameters(true) {
                    Parameter(Named("x"), "'T")
                    Parameter(Named("i"), "'U")
                }
            ) {
                Constant(UnitExpr())
            })
                .returnType(CommonType.Unit)
        }
        |> produces
            """
let foo (x: 'T, i: 'U) : unit = ()

"""

    [<Test>]
    let ``Produces an inlined function with parameters`` () =
        AnonymousModule() { InlinedFunction("x", Parameters() { Named("i") }) { UnitExpr() } }
        |> produces
            """

let inline x i = ()

"""

    [<Test>]
    let ``Produces a function with parameters and access controls`` () =
        AnonymousModule() {
            (Function("x", Parameters() { Named("i") }) { UnitExpr() })
                .toPublic()

            (Function("y", Parameters() { Named("i") }) { UnitExpr() })
                .toPrivate()

            (Function("z", Parameters() { Named("i") }) { UnitExpr() })
                .toInternal()
        }
        |> produces
            """

let public x i = ()
let private y i = ()
let internal z i = ()

"""
