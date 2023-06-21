namespace Fabulous.AST.Tests.MethodDefinitions

open Fantomas.FCS.Text
open Fabulous.AST
open Fabulous.AST.Tests
open Fantomas.Core.SyntaxOak
open type Ast
open NUnit.Framework

module MethodMembers =

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
            [ Pattern.CreateSingleParameter(Pattern.CreateNamed("params"), Some(CommonType.string)) ]

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
                [ Pattern.CreateParameter(Pattern.CreateNamed("name"), Some(CommonType.string))

                  Pattern.CreateParameter(Pattern.CreateNamed("age"), Some(CommonType.int)) ]
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
                [ Pattern.CreateParameter(Pattern.CreateNamed("name"), Some(CommonType.string))

                  Pattern.CreateParameter(Pattern.CreateNamed("age"), Some(CommonType.int)) ]
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
