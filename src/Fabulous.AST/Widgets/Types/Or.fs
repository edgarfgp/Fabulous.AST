namespace Fabulous.AST

open Fantomas.Core.SyntaxOak
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.FCS.Text

module OrType =
    let Identifier = Attributes.defineScalar<struct (Type * string * Type)> "Identifier"

    let WidgetKey =
        Widgets.register "OrType" (fun widget ->
            let struct (lhs, node, rhs) = Widgets.getScalarValue widget Identifier
            Type.Or(TypeOrNode(lhs, SingleTextNode.Create(node), rhs, Range.Zero)))

[<AutoOpen>]
module OrTypeBuilders =
    type Ast with
        static member OrType(lhs: WidgetBuilder<Type>, node: string, rhs: WidgetBuilder<Type>) =
            WidgetBuilder<Type>(
                OrType.WidgetKey,
                AttributesBundle(
                    StackList.one(OrType.Identifier.WithValue(Gen.mkOak lhs, node, Gen.mkOak rhs)),
                    Array.empty,
                    Array.empty
                )
            )

        static member OrType(lhs: string, node: string, rhs: WidgetBuilder<Type>) =
            Ast.OrType(Ast.LongIdent(lhs), node, rhs)

        static member OrType(lhs: WidgetBuilder<Type>, node: string, rhs: string) =
            Ast.OrType(lhs, node, Ast.LongIdent(rhs))

        static member OrType(lhs: string, node: string, rhs: string) =
            Ast.OrType(Ast.LongIdent(lhs), node, Ast.LongIdent(rhs))
