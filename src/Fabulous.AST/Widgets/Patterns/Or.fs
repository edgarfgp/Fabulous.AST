namespace Fabulous.AST

open Fabulous.AST
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
                    [| Or.LHSPattern.WithValue(lhs.Compile())
                       Or.RHSPattern.WithValue(rhs.Compile()) |],
                    Array.empty
                )
            )

        static member OrPat(lhs: string, rhs: WidgetBuilder<Pattern>) = Ast.OrPat(Ast.ConstantPat(lhs), rhs)

        static member OrPat(lhs: WidgetBuilder<Pattern>, rhs: string) = Ast.OrPat(lhs, Ast.ConstantPat(rhs))

        static member OrPat(lhs: WidgetBuilder<Constant>, rhs: WidgetBuilder<Constant>) =
            Ast.OrPat(Ast.ConstantPat(lhs), Ast.ConstantPat(rhs))

        static member OrPat(lhs: WidgetBuilder<Pattern>, rhs: WidgetBuilder<Constant>) =
            Ast.OrPat(lhs, Ast.ConstantPat(rhs))

        static member OrPat(lhs: WidgetBuilder<Constant>, rhs: WidgetBuilder<Pattern>) =
            Ast.OrPat(Ast.ConstantPat(lhs), rhs)

        static member OrPat(lhs: string, rhs: WidgetBuilder<Constant>) =
            Ast.OrPat(Ast.ConstantPat(lhs), Ast.ConstantPat(rhs))

        static member OrPat(lhs: WidgetBuilder<Constant>, rhs: string) =
            Ast.OrPat(Ast.ConstantPat(lhs), Ast.ConstantPat(rhs))

        static member OrPat(lhs: string, rhs: string) =
            Ast.OrPat(Ast.Constant(lhs), Ast.Constant(rhs))
