namespace Fabulous.AST.Tests.MethodDefinitions

open Fabulous.AST
open Fabulous.AST.Tests
open Xunit

open type Ast

module ExplicitConstructor =

    [<Fact>]
    let ``Produces a classes ExplicitCtor``() =
        Oak() {
            AnonymousModule() {
                TypeDefn("Person", UnitPat()) {
                    Constructor(
                        ParenPat(
                            TuplePat(
                                [ ConstantPat(Constant "x")
                                  ConstantPat(Constant "y")
                                  ConstantPat(Constant "z") ]
                            )
                        ),
                        RecordExpr(
                            [ RecordFieldExpr("X", ConstantExpr(Constant "x"))
                              RecordFieldExpr("Y", ConstantExpr(Constant "y"))
                              RecordFieldExpr("Z", ConstantExpr(Constant "z")) ]
                        )
                    )
                        .alias("this")

                    Constructor(
                        ParenPat(TuplePat([ Constant "x"; Constant "y"; Constant "z" ])),
                        RecordExpr(
                            [ RecordFieldExpr("X", ConstantExpr(Constant "x"))
                              RecordFieldExpr("Y", ConstantExpr(Constant "y"))
                              RecordFieldExpr("Z", ConstantExpr(Constant "z")) ]
                        )
                    )
                }
            }
        }
        |> produces
            """
type Person() =
    new(x, y, z) as this = { X = x; Y = y; Z = z }
    new(x, y, z) = { X = x; Y = y; Z = z }
"""

    [<Fact>]
    let ``Produces a class with ExplicitCtor Expr``() =
        Oak() {
            AnonymousModule() {
                TypeDefn(
                    "MyClass",
                    Constructor(
                        TuplePat(
                            [ ParameterPat(ConstantPat(Constant("x0")))
                              ParameterPat(ConstantPat(Constant("y0")))
                              ParameterPat(ConstantPat(Constant("z0"))) ]
                        )

                    )
                ) {
                    Constructor(
                        UnitPat(),
                        AppSingleParenArgExpr("MyClass", ParenExpr(TupleExpr([ Int(0); Int(0); Int(0) ])))
                    )
                }
            }
        }
        |> produces
            """
type MyClass(x0, y0, z0) =
    new() = MyClass (0, 0, 0)
"""

    [<Fact>]
    let ``Produces a class with ExplicitCtor Constant``() =
        Oak() {
            AnonymousModule() {
                TypeDefn(
                    "MyClass",
                    Constructor(
                        TuplePat(
                            [ ParameterPat(ConstantPat(Constant("x0")))
                              ParameterPat(ConstantPat(Constant("y0")))
                              ParameterPat(ConstantPat(Constant("z0"))) ]
                        )

                    )
                ) {
                    Constructor(
                        Constant("x"),
                        AppSingleParenArgExpr("MyClass", ParenExpr(TupleExpr([ Constant("x"); Int(0); Int(0) ])))
                    )
                }

                TypeDefn(
                    "MyClass",
                    Constructor(
                        TuplePat(
                            [ ParameterPat(ConstantPat(Constant("x0")))
                              ParameterPat(ConstantPat(Constant("y0")))
                              ParameterPat(ConstantPat(Constant("z0"))) ]
                        )

                    )
                ) {
                    Constructor(Constant("x"), Constant("MyClass (x, 0, 0)"))
                }

                TypeDefn(
                    "MyClass",
                    Constructor(
                        TuplePat(
                            [ ParameterPat(ConstantPat(Constant("x0")))
                              ParameterPat(ConstantPat(Constant("y0")))
                              ParameterPat(ConstantPat(Constant("z0"))) ]
                        )

                    )
                ) {
                    Constructor(UnitPat(), Constant("MyClass (0, 0, 0)"))
                }
            }
        }
        |> produces
            """
type MyClass(x0, y0, z0) =
    new(x) = MyClass (x, 0, 0)

type MyClass(x0, y0, z0) =
    new(x) = MyClass (x, 0, 0)

type MyClass(x0, y0, z0) =
    new() = MyClass (0, 0, 0)
"""

    [<Fact>]
    let ``Produces a class with ExplicitCtor Pattern``() =
        Oak() {
            AnonymousModule() {
                TypeDefn(
                    "MyClass",
                    Constructor(
                        TuplePat(
                            [ ParameterPat(ConstantPat(Constant("x0")))
                              ParameterPat(ConstantPat(Constant("y0")))
                              ParameterPat(ConstantPat(Constant("z0"))) ]
                        )

                    )
                ) {
                    Constructor(
                        ParenPat(TuplePat([ Constant "x"; Constant "y"; Constant "z" ])),
                        AppSingleParenArgExpr("MyClass", ParenExpr(TupleExpr([ Constant("x"); Int(0); Int(0) ])))
                    )
                }
            }
        }
        |> produces
            """
type MyClass(x0, y0, z0) =
    new(x, y, z) = MyClass (x, 0, 0)

"""

    [<Fact>]
    let ``Produces a class with ExplicitCtor with extra modifiers``() =
        Oak() {
            AnonymousModule() {
                TypeDefn(
                    "MyClass",
                    Constructor(
                        TuplePat(
                            [ ParameterPat(ConstantPat(Constant("x0")))
                              ParameterPat(ConstantPat(Constant("y0")))
                              ParameterPat(ConstantPat(Constant("z0"))) ]
                        )

                    )
                ) {
                    Constructor(
                        ParenPat(TuplePat([ Constant "x"; Constant "y"; Constant "z" ])),
                        AppSingleParenArgExpr("MyClass", ParenExpr(TupleExpr([ Constant("x"); Int(0); Int(0) ])))
                    )
                        .attributes([ Attribute("Obsolete") ])
                        .xmlDocs([ "This is an explicit constructor." ])

                    Constructor(
                        ParenPat(TuplePat([ Constant "x"; Constant "y"; Constant "z" ])),
                        AppSingleParenArgExpr("MyClass", ParenExpr(TupleExpr([ Constant("x"); Int(0); Int(0) ])))
                    )
                        .toPrivate()
                }
            }
        }
        |> produces
            """
type MyClass(x0, y0, z0) =
    /// This is an explicit constructor.
    [<Obsolete>]
    new(x, y, z) = MyClass (x, 0, 0)

    private new(x, y, z) = MyClass (x, 0, 0)

"""

    [<Fact>]
    let ``Produces a class with ExplicitCtor string``() =
        Oak() {
            AnonymousModule() {
                TypeDefn(
                    "MyClass",
                    Constructor(
                        TuplePat(
                            [ ParameterPat(ConstantPat(Constant("x0")))
                              ParameterPat(ConstantPat(Constant("y0")))
                              ParameterPat(ConstantPat(Constant("z0"))) ]
                        )

                    )
                ) {
                    Constructor(
                        "x",
                        AppSingleParenArgExpr("MyClass", ParenExpr(TupleExpr([ Constant("x"); Int(0); Int(0) ])))
                    )
                }
            }
        }
        |> produces
            """
type MyClass(x0, y0, z0) =
    new(x) = MyClass (x, 0, 0)
"""
