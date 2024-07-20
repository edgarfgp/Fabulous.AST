namespace Fabulous.AST

open Fabulous.Builders
open Fabulous.Builders.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Syntax

module OptionalVal =
    let Value = Attributes.defineScalar<string> "Value"

    let WidgetKey =
        Widgets.register "Pattern" (fun widget ->
            let value =
                Widgets.getScalarValue widget Value |> PrettyNaming.NormalizeIdentifierBackticks

            Pattern.OptionalVal(SingleTextNode.Create(value)))

[<AutoOpen>]
module OptionalValBuilders =
    type Ast with

        static member OptionalValPat(value: string) =
            WidgetBuilder<Pattern>(
                OptionalVal.WidgetKey,
                AttributesBundle(StackList.one(OptionalVal.Value.WithValue(value)), Array.empty, Array.empty)
            )
