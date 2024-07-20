namespace Fabulous.AST

open Fabulous.Builders
open Fabulous.Builders.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module As =
    let LHSPattern = Attributes.defineWidget "LHSPattern"

    let MiddlePattern = Attributes.defineScalar<SingleTextNode> "MHSPattern"

    let RHSPattern = Attributes.defineWidget "RHSPattern"

    let WidgetKey =
        Widgets.register "As" (fun widget ->
            let lhs = Widgets.getNodeFromWidget widget LHSPattern
            let middle = Widgets.getScalarValue widget MiddlePattern
            let rhs = Widgets.getNodeFromWidget widget RHSPattern
            Pattern.As(PatLeftMiddleRight(lhs, Choice1Of2(middle), rhs, Range.Zero)))

[<AutoOpen>]
module AsBuilders =
    type Ast with

        static member AsPat(lhs: WidgetBuilder<Pattern>, rhs: WidgetBuilder<Pattern>) =
            WidgetBuilder<Pattern>(
                As.WidgetKey,
                AttributesBundle(
                    StackList.one(As.MiddlePattern.WithValue(SingleTextNode.``as``)),
                    [| As.LHSPattern.WithValue(lhs.Compile())
                       As.RHSPattern.WithValue(rhs.Compile()) |],
                    Array.empty
                )
            )

        static member AsPat(lhs: WidgetBuilder<Constant>, rhs: WidgetBuilder<Pattern>) =
            Ast.AsPat(Ast.ConstantPat(lhs), rhs)

        static member AsPat(lhs: WidgetBuilder<Pattern>, rhs: string) = Ast.AsPat(lhs, Ast.ConstantPat(rhs))

        static member AsPat(lhs: string, rhs: WidgetBuilder<Pattern>) = Ast.AsPat(Ast.Constant(lhs), rhs)

        static member AsPat(lhs: WidgetBuilder<Pattern>, rhs: WidgetBuilder<Constant>) =
            Ast.AsPat(lhs, Ast.ConstantPat(rhs))

        static member AsPat(lhs: WidgetBuilder<Constant>, rhs: WidgetBuilder<Constant>) =
            Ast.AsPat(Ast.ConstantPat(lhs), Ast.ConstantPat(rhs))

        static member AsPat(lhs: string, rhs: WidgetBuilder<Constant>) =
            Ast.AsPat(Ast.Constant(lhs), Ast.ConstantPat(rhs))

        static member AsPat(lhs: WidgetBuilder<Constant>, rhs: string) =
            Ast.AsPat(Ast.ConstantPat(lhs), Ast.Constant(rhs))

        static member AsPat(lhs: string, rhs: string) =
            Ast.AsPat(Ast.Constant(lhs), Ast.Constant(rhs))
