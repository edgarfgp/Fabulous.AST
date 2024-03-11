namespace Fabulous.AST.Tests

open Fantomas.Core

open Fabulous.AST

open type Ast
open Xunit

module WidgetTests =
    [<Fact>]
    let ``string -> OAK``() =
        let source =
            """

let ``a`` = 0

"""

        let rootNode = CodeFormatter.ParseOakAsync(false, source) |> Async.RunSynchronously
        Assert.NotNull(rootNode)
