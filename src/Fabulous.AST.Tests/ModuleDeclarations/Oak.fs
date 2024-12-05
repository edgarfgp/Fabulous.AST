namespace Fabulous.AST.Tests.ModuleDeclarations

open Fabulous.AST.Tests
open Xunit

open Fabulous.AST

open type Ast

module Oak =
    [<Fact>]
    let ``Produces multiple ModuleOrNamespaceNode widgets``() =
        Oak() {
            AnonymousModule() { NoWarn(String "0044") }

            Namespace("MyNamespace") { Record("Person") { Field("name", "string") } }

            Namespace("MyModule") { Record("Person") { Field("name", "string") } }
            |> _.toImplicit()

            AnonymousModule() { Record("Person") { Field("name", "string") } }

            Namespace("MyNamespace") { Module("MyModule") { Record("Person") { Field("name", "string") } } }
        }
        |> produces
            """
#nowarn "0044"
namespace MyNamespace

type Person = { name: string }
module MyModule

type Person = { name: string }
type Person = { name: string }
namespace MyNamespace

module MyModule =
    type Person = { name: string }

"""
