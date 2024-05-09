namespace Fabulous.AST

open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Syntax
open Fantomas.FCS.Text

module ListCons =
    let LHSPattern = Attributes.defineWidget "LHSPattern"

    let MiddlePattern = Attributes.defineScalar<SingleTextNode> "MHSPattern"

    let RHSPattern = Attributes.defineWidget "RHSPattern"

    let WidgetKey =
        Widgets.register "ListCons" (fun widget ->
            let lhs = Widgets.getNodeFromWidget widget LHSPattern
            let middle = Widgets.getScalarValue widget MiddlePattern
            let rhs = Widgets.getNodeFromWidget widget RHSPattern

            Pattern.ListCons(PatLeftMiddleRight(lhs, Choice1Of2(middle), rhs, Range.Zero)))

[<AutoOpen>]
module ListConsBuilders =
    type Ast with

        static member ListConsPat(lhs: WidgetBuilder<Pattern>, rhs: WidgetBuilder<Pattern>) =
            WidgetBuilder<Pattern>(
                ListCons.WidgetKey,
                AttributesBundle(
                    StackList.one(ListCons.MiddlePattern.WithValue(SingleTextNode.doubleColon)),
                    [| ListCons.LHSPattern.WithValue(lhs.Compile())
                       ListCons.RHSPattern.WithValue(rhs.Compile()) |],
                    Array.empty
                )
            )

        static member ListConsPat(lhs: WidgetBuilder<Constant>, rhs: WidgetBuilder<Constant>) =
            Ast.ListConsPat(Ast.ConstantPat(lhs), Ast.ConstantPat(rhs))

        static member ListConsPat(lhs: string, rhs: string) =
            Ast.ListConsPat(Ast.Constant(lhs), Ast.Constant(rhs))
