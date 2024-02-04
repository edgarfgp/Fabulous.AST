namespace Fabulous.AST.Tests.LetBindings

open Fabulous.AST
open Fabulous.AST.Tests

open type Ast
open Fantomas.Core.SyntaxOak
open NUnit.Framework

module Function =

    [<Test>]
    let ``Produces a function with parameters`` () =
        AnonymousModule() { Function("x", Parameters([ ("i", None) ], false)) { UnitExpr() } }
        |> produces
            """

let x i = ()

"""

    [<Test>]
    let ``Produces a function with parameters and an attribute`` () =
        AnonymousModule() {
            (Function("x", Parameters([ ("i", None) ], false)) { UnitExpr() })
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
            (Function("x", Parameters([ ("i", None) ], false)) { UnitExpr() })
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
            (Function("x", Parameters([ ("i", None) ], false)) { UnitExpr() })
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
                Parameters(
                    [ ("x", Some(CommonType.mkLongIdent("'T")))
                      ("i", Some(CommonType.mkLongIdent("'U"))) ],
                    true
                )
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
        AnonymousModule() { InlinedFunction("x", Parameters([ ("i", None) ], false)) { UnitExpr() } }
        |> produces
            """

let inline x i = ()

"""

    [<Test>]
    let ``Produces a function with parameters and access controls`` () =
        AnonymousModule() {
            (Function("x", Parameters([ ("i", None) ], false)) { UnitExpr() })
                .toPublic()

            (Function("y", Parameters([ ("i", None) ], false)) { UnitExpr() })
                .toPrivate()

            (Function("z", Parameters([ ("i", None) ], false)) { UnitExpr() })
                .toInternal()
        }
        |> produces
            """

let public x i = ()
let private y i = ()
let internal z i = ()

"""
