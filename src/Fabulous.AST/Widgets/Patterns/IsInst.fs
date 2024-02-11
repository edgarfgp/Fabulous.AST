namespace Fabulous.AST

open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module IsInst =

    let Token = Attributes.defineScalar<SingleTextNode> "Token"

    let InstType = Attributes.defineScalar<Type> "InstType"

    let WidgetKey =
        Widgets.register "IsInst" (fun widget ->
            let token = Helpers.getScalarValue widget Token
            let instType = Helpers.getScalarValue widget InstType

            Pattern.IsInst(PatIsInstNode(token, instType, Range.Zero)))

[<AutoOpen>]
module IsInstPatBuilders =
    type Ast with

        static member IsInstPat(tp: Type) =
            WidgetBuilder<Pattern>(
                IsInst.WidgetKey,
                AttributesBundle(
                    StackList.two(IsInst.Token.WithValue(SingleTextNode.isInstance), IsInst.InstType.WithValue(tp)),
                    ValueNone,
                    ValueNone
                )
            )

        static member IsInstPat(token: string, tp: Type) =
            WidgetBuilder<Pattern>(
                IsInst.WidgetKey,
                AttributesBundle(
                    StackList.two(IsInst.Token.WithValue(SingleTextNode.Create(token)), IsInst.InstType.WithValue(tp)),
                    ValueNone,
                    ValueNone
                )
            )
