namespace Fabulous.AST.Tests

open Fantomas.Core
open NUnit.Framework

open Fabulous.AST

open type Ast

module WidgetTests =
    [<Test>]
    let ``string -> OAK`` () =
        let source =
            """

let ``a`` = 0

"""

        let rootNode = CodeFormatter.ParseOakAsync(false, source) |> Async.RunSynchronously
        Assert.NotNull(rootNode)
