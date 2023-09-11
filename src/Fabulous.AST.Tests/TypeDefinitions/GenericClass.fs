namespace Fabulous.AST.Tests.TypeDefinitions

open System.Data
open Fantomas.FCS.Text
open Fabulous.AST.Tests
open Fantomas.Core.SyntaxOak
open NUnit.Framework

open Fabulous.AST
open type Ast

module GenericClass =
    [<Test>]
    let ``Produces a generic class`` () =
        let expr = Expr.Constant(Constant.FromText(SingleTextNode("\"\"", Range.Zero)))

        AnonymousModule() {
            GenericClass("Person", [ "'a"; "'b" ]) { PropertyMember("this.Name") { EscapeHatch(expr) } }

        }
        |> produces
            """
type Person <'a, 'b> =
    member this.Name = ""

"""

    [<Test>]
    let ``Produces a generic class with a constructor`` () =
        let expr = Expr.Constant(Constant.FromText(SingleTextNode("\"\"", Range.Zero)))

        AnonymousModule() {
            (GenericClass("Person", [ "'a"; "'b" ]) { PropertyMember("this.Name") { EscapeHatch(expr) } })
                .implicitConstructorParameters([])

        }
        |> produces
            """
type Person <'a, 'b>() =
    member this.Name = ""

"""

    [<Test>]
    let ``Produces a struct generic class with a constructor`` () =
        let expr = Expr.Constant(Constant.FromText(SingleTextNode("\"\"", Range.Zero)))

        AnonymousModule() {
            (GenericClass("Person", [ "'a"; "'b" ]) { PropertyMember("this.Name") { EscapeHatch(expr) } })
                .isStruct()
                .implicitConstructorParameters([])

        }
        |> produces
            """
[<Struct>]
type Person <'a, 'b>() =
    member this.Name = ""

"""
