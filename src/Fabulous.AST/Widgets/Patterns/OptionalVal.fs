namespace Fabulous.AST

open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak

module OptionalVal =
    let Value = Attributes.defineScalar<string> "Value"

    let WidgetKey =
        Widgets.register "Pattern" (fun widget ->
            let value = Widgets.getScalarValue widget Value
            Pattern.OptionalVal(SingleTextNode.Create(value)))

[<AutoOpen>]
module OptionalValBuilders =
    type Ast with

        static member private OptionalValPat(value: string) =
            WidgetBuilder<Pattern>(
                OptionalVal.WidgetKey,
                AttributesBundle(StackList.one(OptionalVal.Value.WithValue(value)), Array.empty, Array.empty)
            )
