namespace Fabulous.AST

open Fabulous.AST
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Syntax
open Fantomas.FCS.Text

module NamedParenStarIdent =
    let Value = Attributes.defineScalar<string> "Value"

    let WidgetKey =
        Widgets.register "NamedParenStarIdent" (fun widget ->
            let value =
                Widgets.getScalarValue widget Value |> PrettyNaming.NormalizeIdentifierBackticks

            Pattern.NamedParenStarIdent(
                PatNamedParenStarIdentNode(
                    None,
                    SingleTextNode.leftParenthesis,
                    SingleTextNode.Create(value),
                    SingleTextNode.rightParenthesis,
                    Range.Zero
                )
            ))

[<AutoOpen>]
module NamedParenStarIdentBuilders =
    type Ast with

        static member NamedParenStarIdentPat(name: string) =
            WidgetBuilder<Pattern>(NamedParenStarIdent.WidgetKey, NamedParenStarIdent.Value.WithValue(name))
