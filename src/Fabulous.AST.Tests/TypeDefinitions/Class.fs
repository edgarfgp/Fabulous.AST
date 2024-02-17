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

        AnonymousModule() { Class("Person") { Member("this.Name") { EscapeHatch(expr) } } }
        |> produces
            """
type Person () =
    member this.Name = name

"""

    [<Test>]
    let ``Produces a class explicit constructor with no params`` () =

        AnonymousModule() { Class("Person") { Member("this.Name") { ConstantExpr(ConstantString "\"\"") } } }
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
                Member("this.Name") { ConstantExpr(ConstantString "name") }
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
                    SimplePat("name", CommonType.String, false)
                    SimplePat("lastName", CommonType.String, false)
                    SimplePat("age", CommonType.Int32, true)
                }
            ) {
                Member("this.Name") { ConstantExpr(ConstantString "name") }
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
                    SimplePat("name", CommonType.String, false)
                    SimplePat("age", CommonType.Int32, false)
                }
            ) {
                Member("this.Name") { ConstantExpr(ConstantString "name") }
            }
        }
        |> produces
            """
type Person (name: string, age: int) =
    member this.Name = name

"""

    [<Test>]
    let ``Produces a class marked as a Struct explicit constructor with typed params`` () =
        let param =
            SimplePatNode(None, false, SingleTextNode("name", Range.Zero), Some(CommonType.String), Range.Zero)

        AnonymousModule() {
            (Class("Person", ImplicitConstructor() { SimplePat("name", CommonType.String, false) }) {
                Member("this.Name") { ConstantExpr(ConstantString "name") }
            })
                .attributes(AttributeNode("Struct"))
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
            (Class("Person") { Member("this.Name") { EscapeHatch(expr) } })
                .attributes(
                    AttributeNodes() {
                        AttributeNode("Sealed")
                        AttributeNode("AbstractClass")
                    }
                )
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
            Class("Person", [ "'a"; "'b" ]) { Member("this.Name") { ConstantExpr(ConstantString "\"\"") } }

        }
        |> produces
            """
type Person <'a, 'b>() =
    member this.Name = ""

"""

    [<Test>]
    let ``Produces a generic class with a constructor`` () =

        AnonymousModule() {
            Class("Person", [ "'a"; "'b" ]) { Member("this.Name") { ConstantExpr(ConstantString "\"\"") } }

        }
        |> produces
            """
type Person <'a, 'b>() =
    member this.Name = ""

"""

    [<Test>]
    let ``Produces a struct generic class with a constructor`` () =
        AnonymousModule() {
            (Class("Person", [ "'a"; "'b" ]) { Member("this.Name") { ConstantExpr(ConstantString "\"\"") } })
                .attributes(AttributeNode("Struct"))

        }
        |> produces
            """
[<Struct>]
type Person <'a, 'b>() =
    member this.Name = ""

"""
