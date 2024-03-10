namespace Fabulous.AST.Tests.ModuleDeclarations.TopLevelBindings

open Fabulous.AST
open Fabulous.AST.Tests
open Xunit

open type Ast

module Function =

    [<Fact>]
    let ``Produces a function with parameter`` () =
        AnonymousModule() { Function("x", ParameterPat("i"), ConstantExpr(ConstantUnit())) }
        |> produces
            """

let x i = ()

"""

    [<Fact>]
    let ``Produces a function with single tupled parameter`` () =
        AnonymousModule() { Function("x", NamedPat("i"), ConstantExpr(ConstantUnit())) }
        |> produces
            """
let x i = ()

"""

    [<Fact>]
    let ``Produces a function with single parameter`` () =
        AnonymousModule() { Function("x", ParenPat(NamedPat("i")), ConstantExpr(ConstantUnit())) }
        |> produces
            """
let x (i) = ()

"""

    [<Fact>]
    let ``Produces a function with single tupled typed parameter`` () =
        AnonymousModule() {
            Function("x", ParenPat(ParameterPat(NamedPat("i"), Int32())), ConstantExpr(ConstantUnit()))
        }
        |> produces
            """
let x (i: int) = ()

"""

    [<Fact>]
    let ``Produces a function with tupled parameters`` () =
        AnonymousModule() {
            Function(
                "x",
                ParametersPat(true) {
                    NamedPat("i")
                    NamedPat("j")
                    NamedPat("k")
                },
                ConstantExpr(ConstantUnit())
            )
        }
        |> produces
            """
let x (i, j, k) = ()

"""

    [<Fact>]
    let ``Produces a function with curried parameters`` () =
        AnonymousModule() {
            Function(
                "x",
                ParametersPat(false) {
                    NamedPat("i")
                    NamedPat("j")
                    NamedPat("k")
                },
                ConstantExpr(ConstantUnit())
            )
        }
        |> produces
            """
let x i j k = ()

"""

    [<Fact>]
    let ``Produces a function with tupled typed parameters`` () =
        AnonymousModule() {
            Function(
                "x",
                ParametersPat(true) {
                    ParameterPat(NamedPat("i"), Int32())
                    ParameterPat(NamedPat("j"), String())
                    ParameterPat(NamedPat("k"), Boolean())
                },
                ConstantExpr(ConstantUnit())
            )
        }
        |> produces
            """
let x (i: int, j: string, k: bool) = ()

"""

    [<Fact>]
    let ``Produces a function with parameters and an attribute`` () =
        AnonymousModule() {
            (Function("x", NamedPat("i"), ConstantExpr(ConstantUnit())))
                .attribute(Attribute("Obsolete", ParenExpr(ConstantExpr("Use bar instead"))))
        }
        |> produces
            """
[<Obsolete("Use bar instead")>]
let x i = ()

"""

    [<Fact>]
    let ``Produces a function with parameters and Xml Doc`` () =
        AnonymousModule() {
            (Function("x", NamedPat("i"), ConstantExpr(ConstantUnit())))
                .xmlDocs([ "Im a function" ])
        }
        |> produces
            """
/// Im a function
let x i = ()

"""

    [<Fact>]
    let ``Produces a function with parameters and return type`` () =
        AnonymousModule() {
            (Function("x", NamedPat("i"), ConstantExpr(ConstantUnit())))
                .returnType(Unit())
        }
        |> produces
            """
let x i : unit = ()

"""

    [<Fact>]
    let ``Produces a function with parameters, return type and typeParams `` () =
        AnonymousModule() {
            (Function(
                "foo",
                ParametersPat(true) {
                    ParameterPat("x", "'T")
                    ParameterPat(NamedPat("i"), "'U")
                },
                ConstantExpr(ConstantUnit())
            ))
                .returnType(Unit())
        }
        |> produces
            """
let foo (x: 'T, i: 'U) : unit = ()

"""

    [<Fact>]
    let ``Produces an inlined function with parameters`` () =
        AnonymousModule() {
            Function("x", NamedPat("i"), ConstantExpr(ConstantUnit()))
                .toInlined()
        }
        |> produces
            """

let inline x i = ()

"""

    [<Fact>]
    let ``Produces a function with parameters and access controls`` () =
        AnonymousModule() {
            (Function("x", NamedPat("i"), ConstantExpr(ConstantUnit())))
                .toPublic()

            (Function("y", NamedPat("i"), ConstantExpr(ConstantUnit())))
                .toPrivate()

            (Function("z", NamedPat("i"), ConstantExpr(ConstantUnit())))
                .toInternal()
        }
        |> produces
            """

let public x i = ()
let private y i = ()
let internal z i = ()

"""
