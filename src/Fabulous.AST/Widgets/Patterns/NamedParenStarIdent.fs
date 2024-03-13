namespace Fabulous.AST

open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module NamedParenStarIdent =
    let Value = Attributes.defineScalar<string> "Value"

    let WidgetKey =
        Widgets.register "NamedParenStarIdent" (fun widget ->
            let value = Widgets.getScalarValue widget Value

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
            WidgetBuilder<Pattern>(
                NamedParenStarIdent.WidgetKey,
                AttributesBundle(StackList.one(NamedParenStarIdent.Value.WithValue(name)), Array.empty, Array.empty)
            )
