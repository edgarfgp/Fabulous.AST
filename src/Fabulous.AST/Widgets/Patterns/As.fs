namespace Fabulous.AST

open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module As =
    let LHSPattern = Attributes.defineWidget "LHSPattern"

    let MiddlePattern = Attributes.defineScalar<SingleTextNode> "MHSPattern"

    let RHSPattern = Attributes.defineWidget "RHSPattern"

    let WidgetKey =
        Widgets.register "As" (fun widget ->
            let lhs = Helpers.getNodeFromWidget widget LHSPattern
            let middle = Helpers.getScalarValue widget MiddlePattern
            let rhs = Helpers.getNodeFromWidget widget RHSPattern
            Pattern.As(PatLeftMiddleRight(lhs, Choice1Of2(middle), rhs, Range.Zero)))

[<AutoOpen>]
module AsBuilders =
    type Ast with

        static member AsPat(lhs: WidgetBuilder<Pattern>, rhs: WidgetBuilder<Pattern>) =
            WidgetBuilder<Pattern>(
                As.WidgetKey,
                AttributesBundle(
                    StackList.one(As.MiddlePattern.WithValue(SingleTextNode.``as``)),
                    ValueSome
                        [| As.LHSPattern.WithValue(lhs.Compile())
                           As.RHSPattern.WithValue(rhs.Compile()) |],
                    ValueNone
                )
            )

        static member AsPat(lhs: string, rhs: string) =
            WidgetBuilder<Pattern>(
                As.WidgetKey,
                AttributesBundle(
                    StackList.one(As.MiddlePattern.WithValue(SingleTextNode.``as``)),
                    ValueSome
                        [| As.LHSPattern.WithValue(Ast.NamedPat(lhs).Compile())
                           As.RHSPattern.WithValue(Ast.NamedPat(rhs).Compile()) |],
                    ValueNone
                )
            )

        static member AsPat(lhs: WidgetBuilder<Pattern>, middle: string, rhs: WidgetBuilder<Pattern>) =
            WidgetBuilder<Pattern>(
                As.WidgetKey,
                AttributesBundle(
                    StackList.one(As.MiddlePattern.WithValue(SingleTextNode.Create(middle))),
                    ValueSome
                        [| As.LHSPattern.WithValue(lhs.Compile())
                           As.RHSPattern.WithValue(rhs.Compile()) |],
                    ValueNone
                )
            )

        static member AsPat(lhs: string, middle: string, rhs: string) =
            WidgetBuilder<Pattern>(
                As.WidgetKey,
                AttributesBundle(
                    StackList.one(As.MiddlePattern.WithValue(SingleTextNode.Create(middle))),
                    ValueSome
                        [| As.LHSPattern.WithValue(Ast.NamedPat(lhs).Compile())
                           As.RHSPattern.WithValue(Ast.NamedPat(rhs).Compile()) |],
                    ValueNone
                )
            )
