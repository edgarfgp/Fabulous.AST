namespace Fabulous.AST

open Fabulous.AST
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Syntax

module OptionalVal =
    let Value = Attributes.defineScalar<string> "Value"

    let WidgetKey =
        Widgets.register "OptionalVal" (fun widget ->
            // Accept both `name` and `?name`; the pattern is always written `?name`.
            let value = (Widgets.getScalarValue widget Value).TrimStart('?')
            let value = PrettyNaming.NormalizeIdentifierBackticks value
            Pattern.OptionalVal(SingleTextNode.Create("?" + value)))

[<AutoOpen>]
module OptionalValBuilders =
    type Ast with

        static member OptionalValPat(value: string) =
            WidgetBuilder<Pattern>(OptionalVal.WidgetKey, OptionalVal.Value.WithValue(value))
