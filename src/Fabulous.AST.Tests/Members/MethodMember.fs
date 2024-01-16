namespace Fabulous.AST.Tests.MethodDefinitions

open Fantomas.FCS.Text
open Fabulous.AST
open Fabulous.AST.Tests
open Fantomas.Core.SyntaxOak
open type Ast
open NUnit.Framework

module MethodMembers =

    [<Test>]
    let ``Produces a record with TypeParams and method member`` () =
        AnonymousModule() {
            (GenericRecord("Colors", [ "'other" ]) {
                Field("Green", CommonType.String)
                Field("Blue", CommonType.mkLongIdent("'other"))
                Field("Yellow", CommonType.Int32)
            })
                .members() {
                let expr = Expr.Constant(Constant.FromText(SingleTextNode("\"\"", Range.Zero)))

                let parameters =
                    [ Pattern.CreateSingleParameter(Pattern.CreateNamed("p"), Some(CommonType.String)) ]

                MethodMember("this.A", parameters) { EscapeHatch(expr) }
            }
        }

        |> produces
            """

type Colors<'other> =
    { Green: string
      Blue: 'other
      Yellow: int }

    member this.A(p: string) = ""

"""

    [<Test>]
    let ``Produces a record with TypeParams and static method member`` () =
        AnonymousModule() {
            (GenericRecord("Colors", [ "'other" ]) {
                Field("Green", CommonType.String)
                Field("Blue", CommonType.mkLongIdent("'other"))
                Field("Yellow", CommonType.Int32)
            })
                .members() {
                let expr = Expr.Constant(Constant.FromText(SingleTextNode("\"\"", Range.Zero)))

                let parameters =
                    [ Pattern.CreateSingleParameter(Pattern.CreateNamed("p"), Some(CommonType.String)) ]

                StaticMethodMember("A", parameters) { EscapeHatch(expr) }
            }
        }

        |> produces
            """

type Colors<'other> =
    { Green: string
      Blue: 'other
      Yellow: int }

    static member A(p: string) = ""

"""

    [<Test>]
    let ``Produces a record with method member`` () =

        let expr = Expr.Constant(Constant.FromText(SingleTextNode("\"\"", Range.Zero)))

        let parameters =
            [ Pattern.CreateSingleParameter(Pattern.CreateNamed("p"), Some(CommonType.String)) ]

        AnonymousModule() {
            (Record("Colors") { Field("X", CommonType.String) })
                .members() {
                MethodMember("this.A", parameters) { EscapeHatch(expr) }
            }
        }
        |> produces
            """

type Colors =
    { X: string }

    member this.A(p: string) = ""

"""

    [<Test>]
    let ``Produces a record with static method member`` () =

        let expr = Expr.Constant(Constant.FromText(SingleTextNode("\"\"", Range.Zero)))

        let parameters =
            [ Pattern.CreateSingleParameter(Pattern.CreateNamed("p"), Some(CommonType.String)) ]

        AnonymousModule() {
            (Record("Colors") { Field("X", CommonType.String) })
                .members() {
                StaticMethodMember("A", parameters) { EscapeHatch(expr) }
            }
        }
        |> produces
            """

type Colors =
    { X: string }

    static member A(p: string) = ""

"""


    [<Test>]
    let ``Produces a classes with a method member`` () =
        let expr = Expr.Constant(Constant.FromText(SingleTextNode("23", Range.Zero)))

        AnonymousModule() {
            (Class("Person") { MethodMember("this.Name") { EscapeHatch(expr) } })
                .implicitConstructorParameters([])
        }
        |> produces
            """
type Person () =
    member this.Name() = 23
"""

    [<Test>]
    let ``Produces a classes with a method member and parameter`` () =
        let expr = Expr.Constant(Constant.FromText(SingleTextNode("23", Range.Zero)))

        let parameters =
            [ Pattern.CreateSingleParameter(Pattern.CreateNamed("params"), Some(CommonType.String)) ]

        AnonymousModule() {
            (Class("Person") { MethodMember("this.Name", parameters) { EscapeHatch(expr) } })
                .implicitConstructorParameters([])
        }
        |> produces
            """
type Person () =
    member this.Name(params: string) = 23
"""

    [<Test>]
    let ``Produces a method member with tupled parameter`` () =
        let expr = Expr.Constant(Constant.FromText(SingleTextNode("23", Range.Zero)))

        let parameters =
            Pattern.CreateTupleParameters(
                [ Pattern.CreateParameter(Pattern.CreateNamed("name"), Some(CommonType.String))

                  Pattern.CreateParameter(Pattern.CreateNamed("age"), Some(CommonType.Int32)) ]
            )

        AnonymousModule() {
            (Class("Person") {
                MethodMember("this.Name", parameters) { EscapeHatch(expr) }

            })
                .implicitConstructorParameters([])
        }
        |> produces
            """
type Person () =
    member this.Name(name: string, age: int) = 23
"""

    [<Test>]
    let ``Produces a method member with multiple parameter`` () =
        let expr = Expr.Constant(Constant.FromText(SingleTextNode("23", Range.Zero)))

        let parameters =
            Pattern.CreateCurriedParameters(
                [ Pattern.CreateParameter(Pattern.CreateNamed("name"), Some(CommonType.String))

                  Pattern.CreateParameter(Pattern.CreateNamed("age"), Some(CommonType.Int32)) ]
            )

        AnonymousModule() {
            (Class("Person") { MethodMember("this.Name", parameters) { EscapeHatch(expr) } })
                .implicitConstructorParameters([])
        }
        |> produces
            """
type Person () =
    member this.Name (name: string) (age: int) = 23
"""

    [<Test>]
    let ``Produces a method member with attributes`` () =
        let expr = Expr.Constant(Constant.FromText(SingleTextNode.Create("23")))

        AnonymousModule() {
            (Class("Person") {
                (MethodMember("this.Name") { EscapeHatch(expr) })
                    .attributes([ "Obsolete" ])
            })
                .implicitConstructorParameters([])
        }
        |> produces
            """
type Person () =
    [<Obsolete>]
    member this.Name() = 23
    
"""

    [<Test>]
    let ``Produces an inline method member`` () =
        let expr = Expr.Constant(Constant.FromText(SingleTextNode("23", Range.Zero)))

        AnonymousModule() {
            (Class("Person") {
                (MethodMember("this.Name") { EscapeHatch(expr) })
                    .isInlined()
            })
                .implicitConstructorParameters([])
        }
        |> produces
            """
type Person () =
    member inline this.Name() = 23
"""

    [<Test>]
    let ``Produces an method member with type parameters`` () =
        let expr = Expr.Constant(Constant.FromText(SingleTextNode.Create("23")))

        AnonymousModule() {
            (Class("Person") {
                (MethodMember("this.Name") { EscapeHatch(expr) })
                    .genericTypeParameters([ "'other" ])
            })
                .implicitConstructorParameters([])
        }
        |> produces
            """
type Person () =
    member this.Name<'other>() = 23
            """

    [<Test>]
    let ``Produces a union with a method member `` () =
        let constExpr = Expr.Constant(Constant.FromText(SingleTextNode.Create("\"name\"")))

        AnonymousModule() {
            (Union("Person") { UnionCase("Name") }).members() { MethodMember("this.Name") { EscapeHatch(constExpr) } }

        }
        |> produces
            """
type Person =
    | Name

    member this.Name() = "name"

"""

    [<Test>]
    let ``Produces a generic union with a method member`` () =
        let constExpr = Expr.Constant(Constant.FromText(SingleTextNode.Create("\"name\"")))

        AnonymousModule() {
            (GenericUnion("Colors", [ "'other" ]) {
                UnionParameterizedCase("Red") {
                    Field("a", CommonType.String)
                    Field("b", CommonType.mkLongIdent "'other")
                }

                UnionCase("Green")
                UnionCase("Blue")
                UnionCase("Yellow")
            })
                .members() {
                MethodMember("this.Name") { EscapeHatch(constExpr) }
            }

        }
        |> produces
            """
type Colors<'other> =
    | Red of a: string * b: 'other
    | Green
    | Blue
    | Yellow

    member this.Name() = "name"

"""
