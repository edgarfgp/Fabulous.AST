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

        Oak() { AnonymousModule() { Class("Person") { Property("this.Name", EscapeHatch(expr)) } } }
        |> produces
            """
type Person () =
    member this.Name = name

"""

    [<Fact>]
    let ``Produces a class explicit constructor with no params``() =

        Oak() { AnonymousModule() { Class("Person") { Property("this.Name", ConstantExpr(Quoted "")) } } }
        |> produces
            """
type Person () =
    member this.Name = ""

"""

    [<Fact>]
    let ``Produces a class explicit constructor with params``() =
        Oak() {
            AnonymousModule() {
                Class(
                    "Person",
                    Constructor(
                        ParametersPat(true) {
                            ParameterPat("name")
                            ParameterPat("lastName")
                            ParameterPat("age")
                        }
                    )
                ) {
                    Property("this.Name", ConstantExpr(Unquoted "name"))
                }

            }
        }
        |> produces
            """
type Person (name, lastName, age) =
    member this.Name = name

"""

    [<Fact>]
    let ``Produces a class explicit constructor with typed params``() =
        Oak() {
            AnonymousModule() {
                Class(
                    "Person",
                    Constructor(
                        ParametersPat(true) {
                            ParameterPat("name", String())
                            ParameterPat("lastName", String())
                            ParameterPat("?age", Int32())
                        }
                    )
                ) {
                    Property("this.Name", ConstantExpr(Unquoted "name"))
                }
            }
        }
        |> produces
            """
type Person (name: string, lastName: string, ?age: int) =
    member this.Name = name
"""

    [<Fact>]
    let ``Produces a class explicit constructor with multiple typed params``() =
        Oak() {
            AnonymousModule() {
                Class(
                    "Person",
                    Constructor(
                        ParametersPat(true) {
                            ParameterPat("name", String())
                            ParameterPat("age", Int32())
                        }
                    )
                ) {
                    Property("this.Name", ConstantExpr(Unquoted "name"))
                }
            }
        }
        |> produces
            """
type Person (name: string, age: int) =
    member this.Name = name

"""

    [<Fact>]
    let ``Produces a class marked as a Struct explicit constructor with typed params``() =
        Oak() {
            AnonymousModule() {
                (Class("Person", Constructor(ParametersPat(true) { ParameterPat("name", String()) })) {
                    Property("this.Name", ConstantExpr(Unquoted "name"))
                })
                    .attribute(Attribute("Struct"))
            }
        }
        |> produces
            """
[<Struct>]
type Person (name: string) =
    member this.Name = name

"""

    [<Fact>]
    let ``Produces a class marked with multiple attributes``() =
        let expr = Expr.Constant(Constant.FromText(SingleTextNode("\"\"", Range.Zero)))

        Oak() {
            AnonymousModule() {
                (Class("Person") { Property("this.Name", EscapeHatch(expr)) }).attributes() {
                    Attribute("Sealed")
                    Attribute("AbstractClass")
                }
            }
        }
        |> produces
            """
[<Sealed; AbstractClass>]
type Person () =
    member this.Name = ""

"""

module GenericClass =
    [<Fact>]
    let ``Produces a generic class``() =
        Oak() {
            AnonymousModule() {
                Class("Person") { Property("this.Name", ConstantExpr(Quoted "")) }
                |> _.typeParams([ "'a"; "'b" ])

            }
        }
        |> produces
            """
type Person <'a, 'b>() =
    member this.Name = ""

"""

    [<Fact>]
    let ``Produces a generic class with a constructor``() =

        Oak() {
            AnonymousModule() {
                Class("Person") { Property("this.Name", ConstantExpr(Quoted "")) }
                |> _.typeParams([ "'a"; "'b" ])

            }
        }
        |> produces
            """
type Person <'a, 'b>() =
    member this.Name = ""

"""

    [<Fact>]
    let ``Produces a struct generic class with a constructor``() =
        Oak() {
            AnonymousModule() {
                Class("Person") { Property("this.Name", ConstantExpr(Quoted "")) }
                |> _.typeParams([ "'a"; "'b" ])
                |> _.attribute(Attribute("Struct"))

            }
        }
        |> produces
            """
[<Struct>]
type Person <'a, 'b>() =
    member this.Name = ""

"""
