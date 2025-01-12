namespace Fabulous.AST

open Fabulous.AST
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
            WidgetBuilder<Pattern>(OptionalVal.WidgetKey, OptionalVal.Value.WithValue(value))
