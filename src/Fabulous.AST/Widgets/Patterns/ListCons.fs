namespace Fabulous.AST

open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
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
                    ValueSome
                        [| ListCons.LHSPattern.WithValue(lhs.Compile())
                           ListCons.RHSPattern.WithValue(rhs.Compile()) |],
                    ValueNone
                )
            )

        static member ListConsPat(lhs: string, rhs: string) =
            WidgetBuilder<Pattern>(
                ListCons.WidgetKey,
                AttributesBundle(
                    StackList.one(ListCons.MiddlePattern.WithValue(SingleTextNode.doubleColon)),
                    ValueSome
                        [| ListCons.LHSPattern.WithValue(Ast.NamedPat(lhs).Compile())
                           ListCons.RHSPattern.WithValue(Ast.NamedPat(rhs).Compile()) |],
                    ValueNone
                )
            )

        static member ListConsPat(lhs: WidgetBuilder<Pattern>, middle: string, rhs: WidgetBuilder<Pattern>) =
            WidgetBuilder<Pattern>(
                ListCons.WidgetKey,
                AttributesBundle(
                    StackList.one(ListCons.MiddlePattern.WithValue(SingleTextNode.Create(middle))),
                    ValueSome
                        [| ListCons.LHSPattern.WithValue(lhs.Compile())
                           ListCons.RHSPattern.WithValue(rhs.Compile()) |],
                    ValueNone
                )
            )

        static member ListConsPat(lhs: string, middle: string, rhs: string) =
            WidgetBuilder<Pattern>(
                ListCons.WidgetKey,
                AttributesBundle(
                    StackList.one(ListCons.MiddlePattern.WithValue(SingleTextNode.Create(middle))),
                    ValueSome
                        [| ListCons.LHSPattern.WithValue(Ast.NamedPat(lhs).Compile())
                           ListCons.RHSPattern.WithValue(Ast.NamedPat(rhs).Compile()) |],
                    ValueNone
                )
            )
