namespace Fabulous.AST.Tests.TypeDefinitions

open Fantomas.FCS.Text
open Fabulous.AST.Tests
open Fantomas.Core.SyntaxOak
open NUnit.Framework

open Fabulous.AST
open type Ast

module Class =
    [<Test>]
    let ``Produces a class implicit constructor`` () =
        let expr = Expr.Constant(Constant.FromText(SingleTextNode("name", Range.Zero)))

        AnonymousModule() { Class("Person") { Property("this.Name", EscapeHatch(expr)) } }
        |> produces
            """
type Person () =
    member this.Name = name

"""

    [<Test>]
    let ``Produces a class explicit constructor with no params`` () =

        AnonymousModule() { Class("Person") { Property("this.Name", ConstantStringExpr("")) } }
        |> produces
            """
type Person () =
    member this.Name = ""

"""

    [<Test>]
    let ``Produces a class explicit constructor with params`` () =
        AnonymousModule() {
            Class(
                "Person",
                ImplicitConstructor() {
                    SimplePat("name", false)
                    SimplePat("lastName", false)
                    SimplePat("age", false)
                }
            ) {
                Property("this.Name", ConstantExpr("name"))
            }

        }
        |> produces
            """
type Person (name, lastName, age) =
    member this.Name = name

"""

    [<Test>]
    let ``Produces a class explicit constructor with typed params`` () =
        AnonymousModule() {
            Class(
                "Person",
                ImplicitConstructor() {
                    SimplePat("name", String(), false)
                    SimplePat("lastName", String(), false)
                    SimplePat("age", Int32(), true)
                }
            ) {
                Property("this.Name", ConstantExpr("name"))
            }
        }
        |> produces
            """
type Person (name: string, lastName: string, ?age: int) =
    member this.Name = name
"""

    [<Test>]
    let ``Produces a class explicit constructor with multiple typed params`` () =
        AnonymousModule() {
            Class(
                "Person",
                ImplicitConstructor() {
                    SimplePat("name", String(), false)
                    SimplePat("age", Int32(), false)
                }
            ) {
                Property("this.Name", ConstantExpr("name"))
            }
        }
        |> produces
            """
type Person (name: string, age: int) =
    member this.Name = name

"""

    [<Test>]
    let ``Produces a class marked as a Struct explicit constructor with typed params`` () =
        AnonymousModule() {
            (Class("Person", ImplicitConstructor() { SimplePat("name", String(), false) }) {
                Property("this.Name", ConstantExpr("name"))
            })
                .attribute(Attribute("Struct"))
        }
        |> produces
            """
[<Struct>]
type Person (name: string) =
    member this.Name = name

"""

    [<Test>]
    let ``Produces a class marked with multiple attributes`` () =
        let expr = Expr.Constant(Constant.FromText(SingleTextNode("\"\"", Range.Zero)))

        AnonymousModule() {
            (Class("Person") { Property("this.Name", EscapeHatch(expr)) })
                .attributes() {
                Attribute("Sealed")
                Attribute("AbstractClass")
            }
        }
        |> produces
            """
[<Sealed; AbstractClass>]
type Person () =
    member this.Name = ""

"""

module GenericClass =
    [<Test>]
    let ``Produces a generic class`` () =
        AnonymousModule() {
            Class("Person", [ "'a"; "'b" ]) { Property("this.Name", ConstantStringExpr("")) }

        }
        |> produces
            """
type Person <'a, 'b>() =
    member this.Name = ""

"""

    [<Test>]
    let ``Produces a generic class with a constructor`` () =

        AnonymousModule() {
            Class("Person", [ "'a"; "'b" ]) { Property("this.Name", ConstantStringExpr("")) }

        }
        |> produces
            """
type Person <'a, 'b>() =
    member this.Name = ""

"""

    [<Test>]
    let ``Produces a struct generic class with a constructor`` () =
        AnonymousModule() {
            (Class("Person", [ "'a"; "'b" ]) { Property("this.Name", ConstantStringExpr("")) })
                .attribute(Attribute("Struct"))

        }
        |> produces
            """
[<Struct>]
type Person <'a, 'b>() =
    member this.Name = ""

"""
