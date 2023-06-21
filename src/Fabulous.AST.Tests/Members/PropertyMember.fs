namespace Fabulous.AST.Tests.MethodDefinitions

open Fabulous.AST
open Fabulous.AST.Tests
open Fantomas.Core.SyntaxOak
open type Ast
open NUnit.Framework

module PropertyMember =

    [<Test>]
    let ``Produces a class with a static and not static member property `` () =
        let constExpr = Expr.Constant(Constant.FromText(SingleTextNode.Create("\"name\"")))

        AnonymousModule() {
            Class("Person") {
                PropertyMember("this.Name1") { EscapeHatch(constExpr) }

                StaticPropertyMember("Name2") { EscapeHatch(constExpr) }
            }
        }
        |> produces
            """
type Person =
    member this.Name1 = "name"
    static member Name2 = "name"

"""

    [<Test>]
    let ``Produces a generic class with a static and not static member property `` () =
        let constExpr = Expr.Constant(Constant.FromText(SingleTextNode.Create("\"name\"")))

        AnonymousModule() {
            GenericClass("Person", [ "'other" ]) {
                PropertyMember("this.Name1") { EscapeHatch(constExpr) }

                StaticPropertyMember("Name2") { EscapeHatch(constExpr) }
            }
        }
        |> produces
            """
type Person <'other> =
    member this.Name1 = "name"
    static member Name2 = "name"

"""

    [<Test>]
    let ``Produces a class with a member property with xml comments`` () =
        let constExpr = Expr.Constant(Constant.FromText(SingleTextNode.Create("\"name\"")))

        AnonymousModule() {
            Class("Person") {
                (PropertyMember("this.Name") { EscapeHatch(constExpr) })
                    .xmlDocs([ "This is a comment" ])
            }
        }
        |> produces
            """
type Person =
    /// This is a comment
    member this.Name = "name"

"""

    [<Test>]
    let ``Produces a class with a member property and accessibility controls`` () =
        let constExpr = Expr.Constant(Constant.FromText(SingleTextNode.Create("\"name\"")))

        AnonymousModule() {
            Class("Person") {
                for name, acc in
                    [ "Name", AccessControl.Public
                      "Age", AccessControl.Private
                      "Address", AccessControl.Internal
                      "PostalCode", AccessControl.Unknown ] do
                    (PropertyMember($"this.{name}") { EscapeHatch(constExpr) })
                        .accessibility(acc)
            }
        }
        |> produces
            """
type Person =
    member public this.Name = "name"
    member private this.Age = "name"
    member internal this.Address = "name"
    member this.PostalCode = "name"
    
"""

    [<Test>]
    let ``Produces a class with a member property and return type`` () =
        let expr = Expr.Constant(Constant.FromText(SingleTextNode.Create("23")))

        AnonymousModule() {
            (Class("Person") { PropertyMember("this.Name", CommonType.int) { EscapeHatch(expr) } })
                .implicitConstructorParameters([])
        }
        |> produces
            """
type Person () =
    member this.Name: int = 23
"""

    [<Test>]
    let ``Produces a class with a member property inlined`` () =
        let constExpr = Expr.Constant(Constant.FromText(SingleTextNode.Create("\"name\"")))

        AnonymousModule() {
            Class("Person") {
                (PropertyMember("this.Name") { EscapeHatch(constExpr) })
                    .isInlined()
            }
        }
        |> produces
            """
type Person =
    member inline this.Name = "name"

"""

    [<Test>]
    let ``Produces a class with property member with attributes`` () =
        let expr = Expr.Constant(Constant.FromText(SingleTextNode.Create("23")))

        AnonymousModule() {
            (Class("Person") {
                (PropertyMember("this.Name") { EscapeHatch(expr) })
                    .attributes([ "Obsolete" ])
            })
                .implicitConstructorParameters([])
        }
        |> produces
            """
type Person () =
    [<Obsolete>]
    member this.Name = 23
"""

    [<Test>]
    let ``Produces a record with a member property `` () =
        let constExpr = Expr.Constant(Constant.FromText(SingleTextNode.Create("\"name\"")))

        AnonymousModule() {
            (Record("Person") { Field("Name", CommonType.string) })
                .members() {
                PropertyMember("this.Name") { EscapeHatch(constExpr) }
            }

        }
        |> produces
            """
type Person =
    { Name: string }

    member this.Name = "name"

"""

    [<Test>]
    let ``Produces a generic record with a member property `` () =
        let constExpr = Expr.Constant(Constant.FromText(SingleTextNode.Create("\"name\"")))

        AnonymousModule() {
            (GenericRecord("Person", [ "'other" ]) { Field("Name", CommonType.mkType("'other")) })
                .members() {
                PropertyMember("this.Name") { EscapeHatch(constExpr) }
            }

        }
        |> produces
            """
type Person<'other> =
    { Name: 'other }

    member this.Name = "name"

"""

    [<Test>]
    let ``Produces a union with a member property `` () =
        let constExpr = Expr.Constant(Constant.FromText(SingleTextNode.Create("\"name\"")))

        AnonymousModule() {
            (Union("Person") { UnionCase("Name") }).members() { PropertyMember("this.Name") { EscapeHatch(constExpr) } }

        }
        |> produces
            """
type Person =
    | Name

    member this.Name = "name"

"""

    [<Test>]
    let ``Produces a generic union with a member property `` () =
        let constExpr = Expr.Constant(Constant.FromText(SingleTextNode.Create("\"name\"")))

        AnonymousModule() {
            (GenericUnion("Colors", [ "'other" ]) {
                UnionParameterizedCase("Red") {
                    Field("a", CommonType.string)
                    Field("b", CommonType.mkType "'other")
                }

                UnionCase("Green")
                UnionCase("Blue")
                UnionCase("Yellow")
            })
                .members() {
                PropertyMember("this.Name") { EscapeHatch(constExpr) }
            }

        }
        |> produces
            """
type Colors<'other> =
    | Red of a: string * b: 'other
    | Green
    | Blue
    | Yellow

    member this.Name = "name"

"""
