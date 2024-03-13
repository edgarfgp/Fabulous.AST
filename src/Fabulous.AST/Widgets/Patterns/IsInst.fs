namespace Fabulous.AST

open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module IsInst =

    let Token = Attributes.defineScalar<SingleTextNode> "Token"

    let InstType = Attributes.defineWidget "InstType"

    let WidgetKey =
        Widgets.register "IsInst" (fun widget ->
            let token = Widgets.getScalarValue widget Token
            let instType = Widgets.getNodeFromWidget widget InstType

            Pattern.IsInst(PatIsInstNode(token, instType, Range.Zero)))

[<AutoOpen>]
module IsInstPatBuilders =
    type Ast with

        static member IsInstPat(value: WidgetBuilder<Type>) =
            WidgetBuilder<Pattern>(
                IsInst.WidgetKey,
                AttributesBundle(
                    StackList.one(IsInst.Token.WithValue(SingleTextNode.isInstance)),
                    [| IsInst.InstType.WithValue(value.Compile()) |],
                    Array.empty
                )
            )

        static member IsInstPat(value: string) =
            WidgetBuilder<Pattern>(
                IsInst.WidgetKey,
                AttributesBundle(
                    StackList.one(IsInst.Token.WithValue(SingleTextNode.isInstance)),
                    [| IsInst.InstType.WithValue(Ast.LongIdent(value).Compile()) |],
                    Array.empty
                )
            )

        static member IsInstPat(token: string, value: string) =
            WidgetBuilder<Pattern>(
                IsInst.WidgetKey,
                AttributesBundle(
                    StackList.one(IsInst.Token.WithValue(SingleTextNode.Create(token))),
                    [| IsInst.InstType.WithValue(Ast.LongIdent(value).Compile()) |],
                    Array.empty
                )
            )
