namespace Fabulous.AST.Tests.ModuleDeclarations

open Fabulous.AST.Tests
open Xunit

open Fabulous.AST

open type Ast

module Oak =
    [<Fact>]
    let ``Produces multiple ModuleOrNamespaceNode widgets``() =
        Oak() {
            Namespace("MyNamespace") { Record("Person") { Field("name", "string") } }

            TopLevelModule("MyModule") { Record("Person") { Field("name", "string") } }

            AnonymousModule() { Record("Person") { Field("name", "string") } }

            Namespace("MyNamespace") { NestedModule("MyModule") { Record("Person") { Field("name", "string") } } }
        }
        |> produces
            """

namespace MyNamespace

type Person = { name: string }
module MyModule

type Person = { name: string }
type Person = { name: string }
namespace MyNamespace

module MyModule =
    type Person = { name: string }

"""
