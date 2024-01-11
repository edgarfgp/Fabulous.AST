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

        AnonymousModule() { Class("Person") { PropertyMember("this.Name") { EscapeHatch(expr) } } }
        |> produces
            """
type Person =
    member this.Name = name

"""

    [<Test>]
    let ``Produces a class explicit constructor with no params`` () =
        let expr = Expr.Constant(Constant.FromText(SingleTextNode("\"\"", Range.Zero)))

        AnonymousModule() {
            (Class("Person") { PropertyMember("this.Name") { EscapeHatch(expr) } })
                .implicitConstructorParameters([])
        }
        |> produces
            """
type Person () =
    member this.Name = ""

"""

    [<Test>]
    let ``Produces a class explicit constructor with params`` () =
        let expr = Expr.Constant(Constant.FromText(SingleTextNode("name", Range.Zero)))

        let param =
            [ "name"; "lastName"; "age" ]
            |> List.map(fun n -> SimplePatNode(None, false, SingleTextNode(n, Range.Zero), None, Range.Zero))

        AnonymousModule() {
            (Class("Person") { PropertyMember("this.Name") { EscapeHatch(expr) } })
                .implicitConstructorParameters(param)
        }
        |> produces
            """
type Person (name, lastName, age) =
    member this.Name = name

"""

    [<Test>]
    let ``Produces a class explicit constructor with typed params`` () =
        let expr = Expr.Constant(Constant.FromText(SingleTextNode("name", Range.Zero)))

        // TODD: Simplify the parameter creation
        let param =
            [ ("name", CommonType.String)
              ("lastName", CommonType.String)
              ("age", CommonType.Int32) ]
            |> List.map(fun n ->
                let isOpt = fst n = "age"

                SimplePatNode(None, isOpt, SingleTextNode(fst n, Range.Zero), Some(snd n), Range.Zero))

        AnonymousModule() {
            (Class("Person") { PropertyMember("this.Name") { EscapeHatch(expr) } })
                .implicitConstructorParameters(param)
        }
        |> produces
            """
type Person (name: string, lastName: string, ?age: int) =
    member this.Name = name
"""

    [<Test>]
    let ``Produces a class explicit constructor with multiple typed params`` () =
        let expr = Expr.Constant(Constant.FromText(SingleTextNode("name", Range.Zero)))

        // TODD: Simplify the parameter creation
        let param =
            [ SimplePatNode(None, false, SingleTextNode("name", Range.Zero), Some(CommonType.String), Range.Zero)
              SimplePatNode(None, false, SingleTextNode("age", Range.Zero), Some(CommonType.Int32), Range.Zero) ]

        AnonymousModule() {
            (Class("Person") { PropertyMember("this.Name") { EscapeHatch(expr) } })
                .implicitConstructorParameters(param)
        }
        |> produces
            """
type Person (name: string, age: int) =
    member this.Name = name

"""

    [<Test>]
    let ``Produces a class marked as a Struct explicit constructor with typed params`` () =
        let expr = Expr.Constant(Constant.FromText(SingleTextNode("name", Range.Zero)))

        let param =
            SimplePatNode(None, false, SingleTextNode("name", Range.Zero), Some(CommonType.String), Range.Zero)

        AnonymousModule() {
            (Class("Person") { PropertyMember("this.Name") { EscapeHatch(expr) } })
                .isStruct()
                .implicitConstructorParameters([ param ])
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
            (Class("Person") { PropertyMember("this.Name") { EscapeHatch(expr) } })
                .attributes([ "Sealed"; "AbstractClass" ])
                .implicitConstructorParameters([])
        }
        |> produces
            """
[<Sealed; AbstractClass>]
type Person () =
    member this.Name = ""

"""
