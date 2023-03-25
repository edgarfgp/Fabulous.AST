namespace Fabulous.AST.Tests

open Fantomas.Core
open NUnit.Framework

open Fabulous.AST

open type Ast

module WidgetTests =
    [<Test>]
    let ``Widget -> OAK`` () =
        AnonymousModule() { Let("x", "12") }
        |> produces
            """
        
let x = 12

"""

    [<Test>]
    let ``string -> OAK`` () =
        let source =
            """

let mutable x = 10

"""

        let rootNode = CodeFormatter.ParseOakAsync(false, source) |> Async.RunSynchronously
        Assert.NotNull(rootNode)
