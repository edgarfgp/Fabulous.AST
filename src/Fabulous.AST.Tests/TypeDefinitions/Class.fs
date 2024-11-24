namespace Fabulous.AST.Tests.TypeDefinitions

open Fantomas.FCS.Text
open Fabulous.AST.Tests
open Fantomas.Core.SyntaxOak
open Xunit

open Fabulous.AST
open type Ast

module Class =
    [<Fact>]
    let ``Produces a class implicit constructor``() =
        let expr = Expr.Constant(Constant.FromText(SingleTextNode("name", Range.Zero)))

        Oak() {
            AnonymousModule() { Class("Person") { Member(ConstantPat(Constant("this.Name")), EscapeHatch(expr)) } }
        }
        |> produces
            """
type Person() =
    member this.Name = name

"""

    [<Fact>]
    let ``Produces a class explicit constructor with no params``() =

        Oak() {
            AnonymousModule() {
                Class("Person") { Member(ConstantPat(Constant("this.Name")), ConstantExpr(String "")) }
            }
        }
        |> produces
            """
type Person() =
    member this.Name = ""

"""

    [<Fact>]
    let ``Produces a private class with``() =

        Oak() {
            AnonymousModule() {
                Class("Person") { Member(ConstantPat(Constant("this.Name")), ConstantExpr(String "")) }
                |> _.toPrivate()
            }
        }
        |> produces
            """
type private Person() =
    member this.Name = ""

    """

    [<Fact>]
    let ``Produces a class explicit constructor with params``() =
        Oak() {
            AnonymousModule() {
                Class(
                    "Person",
                    ImplicitConstructor(
                        ParenPat(
                            TuplePat(
                                [ ParameterPat(ConstantPat(Constant("name")))
                                  ParameterPat(ConstantPat(Constant("lastName")))
                                  ParameterPat(ConstantPat(Constant("age"))) ]
                            )
                        )
                    )
                ) {
                    Member(ConstantPat(Constant("this.Name")), ConstantExpr(Constant "name"))
                }

            }
        }
        |> produces
            """
type Person(name, lastName, age) =
    member this.Name = name

"""

    [<Fact>]
    let ``Produces a class explicit constructor with typed params``() =
        Oak() {
            AnonymousModule() {
                Class(
                    "Person",
                    ImplicitConstructor(
                        ParenPat(
                            TuplePat(
                                [ ParameterPat(ConstantPat(Constant("name")), String())
                                  ParameterPat(ConstantPat(Constant("lastName")), String())
                                  ParameterPat(ConstantPat(Constant("?age")), Int()) ]
                            )
                        )
                    )
                ) {
                    Member(ConstantPat(Constant("this.Name")), ConstantExpr(Constant "name"))
                }
            }
        }
        |> produces
            """
type Person(name: string, lastName: string, ?age: int) =
    member this.Name = name
"""

    [<Fact>]
    let ``Produces a class explicit constructor with multiple typed params``() =
        Oak() {
            AnonymousModule() {
                Class(
                    "Person",
                    ImplicitConstructor(
                        ParenPat(
                            TuplePat(
                                [ ParameterPat(ConstantPat(Constant("name")), String())
                                  ParameterPat(ConstantPat(Constant("age")), Int()) ]
                            )
                        )
                    )
                ) {
                    Member(ConstantPat(Constant("this.Name")), ConstantExpr(Constant "name"))
                }
            }
        }
        |> produces
            """
type Person(name: string, age: int) =
    member this.Name = name

"""

    [<Fact>]
    let ``Produces a class marked as a Struct explicit constructor with typed params``() =
        Oak() {
            AnonymousModule() {
                (Class("Person", ImplicitConstructor(ParenPat(ParameterPat(ConstantPat(Constant("name")), String())))) {
                    Member(ConstantPat(Constant("this.Name")), ConstantExpr(Constant "name"))
                })
                    .attribute(Attribute("Struct"))
            }
        }
        |> produces
            """
[<Struct>]
type Person(name: string) =
    member this.Name = name

"""

    [<Fact>]
    let ``Produces a class marked with multiple attributes``() =
        let expr = Expr.Constant(Constant.FromText(SingleTextNode("\"\"", Range.Zero)))

        Oak() {
            AnonymousModule() {
                (Class("Person") { Member(ConstantPat(Constant("this.Name")), EscapeHatch(expr)) })
                    .attributes([ Attribute("Sealed"); Attribute("AbstractClass") ])
            }
        }
        |> produces
            """
[<Sealed; AbstractClass>]
type Person() =
    member this.Name = ""

"""

module GenericClass =
    [<Fact>]
    let ``Produces a generic class``() =
        Oak() {
            AnonymousModule() {
                Class("Person") { Member(ConstantPat(Constant("this.Name")), ConstantExpr(String "")) }
                |> _.typeParams(PostfixList([ "'a"; "'b" ]))

            }
        }
        |> produces
            """
type Person<'a, 'b>() =
    member this.Name = ""

"""

    [<Fact>]
    let ``Produces a generic class with a constructor``() =

        Oak() {
            AnonymousModule() {
                Class("Person") { Member(ConstantPat(Constant("this.Name")), ConstantExpr(String "")) }
                |> _.typeParams(PostfixList([ "'a"; "'b" ]))

            }
        }
        |> produces
            """
type Person<'a, 'b>() =
    member this.Name = ""

"""

    [<Fact>]
    let ``Produces a struct generic class with a constructor``() =
        Oak() {
            AnonymousModule() {
                Class("Person") { Member(ConstantPat(Constant("this.Name")), ConstantExpr(String "")) }
                |> _.typeParams(PostfixList([ "'a"; "'b" ]))
                |> _.attribute(Attribute("Struct"))

            }
        }
        |> produces
            """
[<Struct>]
type Person<'a, 'b>() =
    member this.Name = ""

"""
