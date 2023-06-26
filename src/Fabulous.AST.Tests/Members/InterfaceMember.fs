namespace Fabulous.AST.Tests.MethodDefinitions

open Fantomas.FCS.Text
open Fabulous.AST
open Fabulous.AST.Tests
open Fantomas.Core.SyntaxOak
open type Ast
open NUnit.Framework

module InterfaceMembers =

    [<Test>]
    let ``Produces a class with a interface member`` () =
        let expr = Expr.Constant(Constant.FromText(SingleTextNode("\"23\"", Range.Zero)))

        AnonymousModule() {
            Interface("Meh") { AbstractPropertyMember("Name", CommonType.String) }

            (Class("Person") {
                InterfaceMember(CommonType.mkType("Meh")) { PropertyMember("this.Name") { EscapeHatch(expr) } }
            })
                .implicitConstructorParameters([])
        }
        |> produces
            """
type Meh =
    abstract member Name: string

type Person () =
    interface Meh with
        member this.Name = "23"
"""
