namespace Fabulous.AST

open Fabulous.AST
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module OrType =
    let Identifier = Attributes.defineScalar<struct (Type * Type)> "Identifier"

    let OrTypeNode = Attributes.defineScalar<SingleTextNode> "OrTypeNode"

    let WidgetKey =
        Widgets.register "OrType" (fun widget ->
            let struct (lhs, rhs) = Widgets.getScalarValue widget Identifier
            let orNode = Widgets.getScalarValue widget OrTypeNode
            Type.Or(TypeOrNode(lhs, orNode, rhs, Range.Zero)))

[<AutoOpen>]
module OrTypeBuilders =
    type Ast with
        static member private BaseOr(lhs: WidgetBuilder<Type>, orNode: SingleTextNode, rhs: WidgetBuilder<Type>) =
            WidgetBuilder<Type>(
                OrType.WidgetKey,
                OrType.Identifier.WithValue(Gen.mkOak lhs, Gen.mkOak rhs),
                OrType.OrTypeNode.WithValue(orNode)
            )

        static member Or(lhs: WidgetBuilder<Type>, rhs: WidgetBuilder<Type>) =
            Ast.BaseOr(lhs, SingleTextNode.``or``, rhs)

        static member Or(lhs: string, rhs: WidgetBuilder<Type>) =
            Ast.BaseOr(Ast.LongIdent(lhs), SingleTextNode.``or``, rhs)

        static member Or(lhs: WidgetBuilder<Type>, rhs: string) =
            Ast.BaseOr(lhs, SingleTextNode.``or``, Ast.LongIdent(rhs))

        static member Or(lhs: string, rhs: string) =
            Ast.BaseOr(Ast.LongIdent(lhs), SingleTextNode.``or``, Ast.LongIdent(rhs))

        static member Nullness(lhs: WidgetBuilder<Type>) =
            Ast.BaseOr(lhs, SingleTextNode.bar, Ast.Null())

        static member Nullness(lhs: string) =
            Ast.BaseOr(Ast.LongIdent(lhs), SingleTextNode.bar, Ast.Null())
