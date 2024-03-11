namespace Fabulous.AST

open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module NamePatPair =

    let Ident = Attributes.defineScalar<string> "Ident"
    let Pat = Attributes.defineWidget "Pat"

    let WidgetKey =
        Widgets.register "NamePatPair" (fun widget ->
            let ident = Widgets.getScalarValue widget Ident
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
                    ValueSome [| NamePatPair.Pat.WithValue(pat.Compile()) |],
                    ValueNone
                )
            )

        static member NamePatPairPat(ident: string, pat: string) =
            WidgetBuilder<NamePatPair>(
                NamePatPair.WidgetKey,
                AttributesBundle(
                    StackList.one(NamePatPair.Ident.WithValue(ident)),
                    ValueSome [| NamePatPair.Pat.WithValue(Ast.NamedPat(pat).Compile()) |],
                    ValueNone
                )
            )
