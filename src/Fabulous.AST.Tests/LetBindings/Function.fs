namespace Fabulous.AST.Tests.LetBindings

open Fabulous.AST
open Fabulous.AST.Tests

open type Ast
open NUnit.Framework

module Function =

    [<Test>]
    let ``Produces a function with parameter`` () =
        AnonymousModule() { Function("x", ParameterPat("i")) { ConstantExpr(ConstantUnit()) } }
        |> produces
            """

let x i = ()

"""

    [<Test>]
    let ``Produces a function with single tupled parameter`` () =
        AnonymousModule() { Function("x", NamedPat("i")) { ConstantExpr(ConstantUnit()) } }
        |> produces
            """
let x i = ()

"""

    [<Test>]
    let ``Produces a function with single parameter`` () =
        AnonymousModule() { Function("x", ParenPat(NamedPat("i"))) { ConstantExpr(ConstantUnit()) } }
        |> produces
            """
let x (i) = ()

"""

    [<Test>]
    let ``Produces a function with single tupled typed parameter`` () =
        AnonymousModule() {
            Function("x", ParenPat(ParameterPat(NamedPat("i"), CommonType.Int32))) { ConstantExpr(ConstantUnit()) }
        }
        |> produces
            """
let x (i: int) = ()

"""

    [<Test>]
    let ``Produces a function with tupled parameters`` () =
        AnonymousModule() {
            Function(
                "x",
                ParametersPat(true) {
                    NamedPat("i")
                    NamedPat("j")
                    NamedPat("k")
                }
            ) {
                ConstantExpr(ConstantUnit())
            }
        }
        |> produces
            """
let x (i, j, k) = ()

"""

    [<Test>]
    let ``Produces a function with curried parameters`` () =
        AnonymousModule() {
            Function(
                "x",
                ParametersPat(false) {
                    NamedPat("i")
                    NamedPat("j")
                    NamedPat("k")
                }
            ) {
                ConstantExpr(ConstantUnit())
            }
        }
        |> produces
            """
let x i j k = ()

"""

    [<Test>]
    let ``Produces a function with tupled typed parameters`` () =
        AnonymousModule() {
            Function(
                "x",
                ParametersPat(true) {
                    ParameterPat(NamedPat("i"), CommonType.Int32)
                    ParameterPat(NamedPat("j"), CommonType.String)
                    ParameterPat(NamedPat("k"), CommonType.Boolean)
                }
            ) {
                ConstantExpr(ConstantUnit())
            }
        }
        |> produces
            """
let x (i: int, j: string, k: bool) = ()

"""

    [<Test>]
    let ``Produces a function with parameters and an attribute`` () =
        AnonymousModule() {
            (Function("x", NamedPat("i")) { ConstantExpr(ConstantUnit()) })
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
            (Function("x", NamedPat("i")) { ConstantExpr(ConstantUnit()) })
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
            (Function("x", NamedPat("i")) { ConstantExpr(ConstantUnit()) })
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
                ParametersPat(true) {
                    ParameterPat("x", "'T")
                    ParameterPat(NamedPat("i"), "'U")
                }
            ) {
                ConstantExpr(ConstantUnit())
            })
                .returnType(CommonType.Unit)
        }
        |> produces
            """
let foo (x: 'T, i: 'U) : unit = ()

"""

    [<Test>]
    let ``Produces an inlined function with parameters`` () =
        AnonymousModule() { InlinedFunction("x", NamedPat("i")) { ConstantExpr(ConstantUnit()) } }
        |> produces
            """

let inline x i = ()

"""

    [<Test>]
    let ``Produces a function with parameters and access controls`` () =
        AnonymousModule() {
            (Function("x", NamedPat("i")) { ConstantExpr(ConstantUnit()) })
                .toPublic()

            (Function("y", NamedPat("i")) { ConstantExpr(ConstantUnit()) })
                .toPrivate()

            (Function("z", NamedPat("i")) { ConstantExpr(ConstantUnit()) })
                .toInternal()
        }
        |> produces
            """

let public x i = ()
let private y i = ()
let internal z i = ()

"""
