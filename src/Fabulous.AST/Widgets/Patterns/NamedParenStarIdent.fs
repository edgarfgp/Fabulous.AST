namespace Fabulous.AST

open Fabulous.AST
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Syntax
open Fantomas.FCS.Text

module NamedParenStarIdent =
    let Value = Attributes.defineScalar<string> "Value"

    let WidgetKey =
        Widgets.register "NamedParenStarIdent" (fun widget ->
            // Operator names such as `*` or `*+` are written as is; identifiers are normalized.
            let value = Widgets.getScalarValue widget Value

            let value =
                if Naming.isSymbolicOperator value then
                    value
                else
                    PrettyNaming.NormalizeIdentifierBackticks value

            let accessControl =
                Widgets.tryGetScalarValue widget Pattern.Accessibility
                |> ValueOption.defaultValue AccessControl.Unknown

            let accessControl =
                match accessControl with
                | Public -> Some(SingleTextNode.``public``)
                | Private -> Some(SingleTextNode.``private``)
                | Internal -> Some(SingleTextNode.``internal``)
                | Unknown -> None

            Pattern.NamedParenStarIdent(
                PatNamedParenStarIdentNode(
                    accessControl,
                    SingleTextNode.leftParenthesis,
                    SingleTextNode.Create(value),
                    SingleTextNode.rightParenthesis,
                    Range.range0
                )
            ))

[<AutoOpen>]
module NamedParenStarIdentBuilders =
    type Ast with

        static member NamedParenStarIdentPat(name: string) =
            WidgetBuilder<Pattern>(NamedParenStarIdent.WidgetKey, NamedParenStarIdent.Value.WithValue(name))
