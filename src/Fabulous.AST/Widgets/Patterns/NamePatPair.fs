namespace Fabulous.AST

open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Syntax
open Fantomas.FCS.Text

module NamePatPair =

    let Ident = Attributes.defineScalar<string> "Ident"
    let Pat = Attributes.defineWidget "Pat"

    let WidgetKey =
        Widgets.register "NamePatPair" (fun widget ->
            let ident =
                Widgets.getScalarValue widget Ident |> PrettyNaming.NormalizeIdentifierBackticks

            let pat = Widgets.getNodeFromWidget widget Pat
            NamePatPair(SingleTextNode.Create(ident), SingleTextNode.equals, pat, Range.Zero))

[<AutoOpen>]
module NamePatPairBuilders =
    type Ast with

        static member NamePatPairPat(ident: string, pat: WidgetBuilder<Pattern>) =
            WidgetBuilder<NamePatPair>(
                NamePatPair.WidgetKey,
                AttributesBundle(
                    StackList.one(NamePatPair.Ident.WithValue(ident)),
                    [| NamePatPair.Pat.WithValue(pat.Compile()) |],
                    Array.empty
                )
            )
