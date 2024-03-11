namespace Fabulous.AST

open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module Or =
    let LHSPattern = Attributes.defineWidget "LHSPattern"

    let MHSPattern = Attributes.defineScalar<SingleTextNode> "MHSPattern"

    let RHSPattern = Attributes.defineWidget "RHSPattern"

    let WidgetKey =
        Widgets.register "Or" (fun widget ->
            let lhs = Widgets.getNodeFromWidget widget LHSPattern
            let middle = Widgets.getScalarValue widget MHSPattern
            let rhs = Widgets.getNodeFromWidget widget RHSPattern
            Pattern.Or(PatLeftMiddleRight(lhs, Choice1Of2(middle), rhs, Range.Zero)))

[<AutoOpen>]
module OrBuilders =
    type Ast with

        static member OrPat(lhs: WidgetBuilder<Pattern>, rhs: WidgetBuilder<Pattern>) =
            WidgetBuilder<Pattern>(
                Or.WidgetKey,
                AttributesBundle(
                    StackList.one(Or.MHSPattern.WithValue(SingleTextNode.bar)),
                    ValueSome
                        [| Or.LHSPattern.WithValue(lhs.Compile())
                           Or.RHSPattern.WithValue(rhs.Compile()) |],
                    ValueNone
                )
            )

        static member OrPat(lhs: string, rhs: string) =
            WidgetBuilder<Pattern>(
                Or.WidgetKey,
                AttributesBundle(
                    StackList.one(Or.MHSPattern.WithValue(SingleTextNode.bar)),
                    ValueSome
                        [| Or.LHSPattern.WithValue(Ast.NamedPat(lhs).Compile())
                           Or.RHSPattern.WithValue(Ast.NamedPat(rhs).Compile()) |],
                    ValueNone
                )
            )

        static member OrPat(lhs: WidgetBuilder<Pattern>, middle: string, rhs: WidgetBuilder<Pattern>) =
            WidgetBuilder<Pattern>(
                Or.WidgetKey,
                AttributesBundle(
                    StackList.one(Or.MHSPattern.WithValue(SingleTextNode.Create(middle))),
                    ValueSome
                        [| Or.LHSPattern.WithValue(lhs.Compile())
                           Or.RHSPattern.WithValue(rhs.Compile()) |],
                    ValueNone
                )
            )

        static member OrPat(lhs: string, middle: string, rhs: string) =
            WidgetBuilder<Pattern>(
                Or.WidgetKey,
                AttributesBundle(
                    StackList.one(Or.MHSPattern.WithValue(SingleTextNode.Create(middle))),
                    ValueSome
                        [| Or.LHSPattern.WithValue(Ast.NamedPat(lhs).Compile())
                           Or.RHSPattern.WithValue(Ast.NamedPat(rhs).Compile()) |],
                    ValueNone
                )
            )
