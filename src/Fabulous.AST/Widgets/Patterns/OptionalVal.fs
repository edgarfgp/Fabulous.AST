namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fabulous.AST.StackAllocatedCollections
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module OptionalVal =
    let Value = Attributes.defineScalar<string> "Value"

    let WidgetKey =
        Widgets.register "Pattern" (fun widget ->
            let value = Helpers.getScalarValue widget Value
            Pattern.OptionalVal(SingleTextNode.Create(value)))

[<AutoOpen>]
module OptionalValBuilders =
    type Ast with

        static member private OptionalValPat(value: string) =
            WidgetBuilder<Pattern>(
                OptionalVal.WidgetKey,
                AttributesBundle(StackList.one(OptionalVal.Value.WithValue(value)), ValueNone, ValueNone)
            )
