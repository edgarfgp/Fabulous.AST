namespace Fabulous.AST.Tests.ModuleDeclarations.TopLevelBindings

open Fabulous.AST
open Fabulous.AST.Tests
open Xunit

open type Ast

module Function =

    [<Fact>]
    let ``Produces a function with widget parameter``() =
        Oak() {
            AnonymousModule() { Function("x", ParameterPat(ConstantPat(Constant "i")), ConstantExpr(ConstantUnit())) }
        }
        |> produces
            """

let x i = ()

"""

    [<Fact>]
    let ``Produces a function with widget parameters``() =
        Oak() {
            AnonymousModule() {
                Function("x", [ ParameterPat(ConstantPat(Constant "i")) ], ConstantExpr(ConstantUnit()))
                Function("x2", ParameterPat(ConstantPat("i")), ConstantExpr("()"))

                Function(
                    "y",
                    [ ParameterPat(ConstantPat("i")); ParameterPat(ConstantPat("j")) ],
                    ConstantExpr(ConstantUnit())
                )

                Function("y2", [ ParameterPat("i"); ParameterPat("j") ], ConstantExpr(ConstantUnit()))
                Function("z", [ "i"; "j" ], ConstantExpr(ConstantUnit()))
            }
        }
        |> produces
            """

let x i = ()
let x2 i = ()
let y i j = ()
let y2 i j = ()
let z i j = ()

"""

    [<Fact>]
    let ``Produces a function with parameter``() =
        Oak() {
            AnonymousModule() {
                Function("x", "i", ConstantExpr(ConstantUnit()))
                Function("y", "i", ConstantUnit())
                Function("z", "i", "()")
            }
        }
        |> produces
            """

let x i = ()
let y i = ()
let z i = ()

"""

    [<Fact>]
    let ``Produces a function with single tupled parameter``() =
        Oak() { AnonymousModule() { Function("x", NamedPat("i"), ConstantExpr(ConstantUnit())) } }
        |> produces
            """
let x i = ()

"""

    [<Fact>]
    let ``Produces a function with single parameter``() =
        Oak() { AnonymousModule() { Function("x", ParenPat(NamedPat("i")), ConstantExpr(ConstantUnit())) } }
        |> produces
            """
let x (i) = ()

"""

    [<Fact>]
    let ``Produces a function with single tupled typed parameter``() =
        Oak() {
            AnonymousModule() {
                Function("x", ParenPat(ParameterPat(NamedPat("i"), Int())), ConstantExpr(ConstantUnit()))
                Function("x", ParenPat(ParameterPat("i", Int())), ConstantExpr(ConstantUnit()))
                Function("x", ParenPat(ParameterPat("i", "int")), ConstantExpr(ConstantUnit()))
            }
        }
        |> produces
            """
let x (i: int) = ()
let x (i: int) = ()
let x (i: int) = ()

"""

    [<Fact>]
    let ``Produces a function with tupled parameters``() =
        Oak() {
            AnonymousModule() {
                Function(
                    "x",
                    ParenPat(
                        TuplePat(
                            [ ParameterPat(ConstantPat(Constant "i"))
                              ParameterPat(ConstantPat(Constant "j"))
                              ParameterPat(ConstantPat(Constant "k")) ]
                        )
                    ),
                    ConstantExpr(ConstantUnit())
                )
            }
        }
        |> produces
            """
let x (i, j, k) = ()

"""

    [<Fact>]
    let ``Produces a function with tupled parameters using named pat``() =
        Oak() {
            AnonymousModule() {
                Function(
                    "x",
                    ParenPat(TuplePat([ NamedPat("i"); NamedPat("j"); NamedPat("k") ])),
                    ConstantExpr(ConstantUnit())
                )
            }
        }
        |> produces
            """
let x (i, j, k) = ()
"""

    [<Fact>]
    let ``Produces a function with tupled parameters using parameter pat``() =
        Oak() {
            AnonymousModule() {
                Function(
                    "x",
                    ParenPat(
                        TuplePat(
                            [ ParameterPat(ConstantPat(Constant("i")), Int())
                              ParameterPat(ConstantPat(Constant "j"), String())
                              ParameterPat(ConstantPat(Constant "k")) ]
                        )
                    ),
                    ConstantExpr(ConstantUnit())
                )
            }
        }
        |> produces
            """
let x (i: int, j: string, k) = ()
"""

    [<Fact>]
    let ``Produces a function with curried parameters``() =
        Oak() {
            AnonymousModule() {
                Function(
                    "x",
                    LongIdentPat(
                        [ ParameterPat(ConstantPat(Constant "i"))
                          ParameterPat(ConstantPat(Constant "j"))
                          ParameterPat(ConstantPat(Constant "k")) ]
                    ),
                    ConstantExpr(ConstantUnit())
                )
            }
        }
        |> produces
            """
let x i j k = ()

"""

    [<Fact>]
    let ``Produces a function with tupled typed parameters``() =
        Oak() {
            AnonymousModule() {
                Function(
                    "x",
                    ParenPat(
                        TuplePat(
                            [ ParameterPat(NamedPat("i"), Int())
                              ParameterPat(NamedPat("j"), String())
                              ParameterPat(NamedPat("k"), Boolean()) ]
                        )
                    ),
                    ConstantExpr(ConstantUnit())
                )
            }
        }
        |> produces
            """
let x (i: int, j: string, k: bool) = ()

"""

    [<Fact>]
    let ``Produces a function with parameters and an attribute``() =
        Oak() {
            AnonymousModule() {
                (Function("x", [ NamedPat("i") ], ConstantExpr(ConstantUnit())))
                    .attribute(Attribute("Obsolete", ParenExpr(ConstantExpr(String "Use bar instead"))))
            }
        }
        |> produces
            """
[<Obsolete("Use bar instead")>]
let x i = ()

"""

    [<Fact>]
    let ``Produces a function with parameters and Xml Doc``() =
        Oak() {
            AnonymousModule() {
                (Function("x", NamedPat("i"), ConstantExpr(ConstantUnit())))
                    .xmlDocs([ "Im a function" ])
            }
        }
        |> produces
            """
/// Im a function
let x i = ()

"""

    [<Fact>]
    let ``Produces a function with parameters and return type``() =
        Oak() { AnonymousModule() { (Function("x", NamedPat("i"), ConstantExpr(ConstantUnit()))).returnType(Unit()) } }
        |> produces
            """
let x i : unit = ()

"""

    [<Fact>]
    let ``Produces a function with parameters, return type and typeParams ``() =
        Oak() {
            AnonymousModule() {
                (Function(
                    "foo",
                    ParenPat(
                        TuplePat(
                            [ ParameterPat(ConstantPat(Constant "x"), LongIdent "'T")
                              ParameterPat(NamedPat("i"), LongIdent "'U") ]
                        )
                    ),
                    ConstantExpr(ConstantUnit())
                ))
                    .returnType(Unit())
            }
        }
        |> produces
            """
let foo (x: 'T, i: 'U) : unit = ()

"""

    [<Fact>]
    let ``Produces an inlined function with parameters``() =
        Oak() { AnonymousModule() { Function("x", NamedPat("i"), ConstantExpr(ConstantUnit())).toInlined() } }
        |> produces
            """

let inline x i = ()

"""

    [<Fact>]
    let ``Produces a function with parameters and access controls``() =
        Oak() {
            AnonymousModule() {
                Function("x", NamedPat("i"), ConstantExpr(ConstantUnit())).toPublic()

                Function("y", NamedPat("i"), ConstantExpr(ConstantUnit())).toPrivate()

                Function("z", NamedPat("i"), ConstantExpr(ConstantUnit())).toInternal()
            }
        }
        |> produces
            """

let public x i = ()
let private y i = ()
let internal z i = ()

"""

    [<Fact>]
    let ``Produces a default member``() =
        Oak() {
            AnonymousModule() {
                TypeDefn("Person", ParenPat()) {
                    AbstractMember("GetValue", [ Unit() ], String())
                    Default("this.GetValue", UnitPat(), ConstantExpr(String("")))
                }
            }
        }
        |> produces
            """
type Person() =
    abstract GetValue: unit -> string
    default this.GetValue() = ""
"""
