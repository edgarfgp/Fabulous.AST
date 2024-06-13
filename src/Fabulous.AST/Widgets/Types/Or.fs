namespace Fabulous.AST

open Fantomas.Core.SyntaxOak
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.FCS.Text

module OrType =
    let Identifier = Attributes.defineScalar<struct (Type * Type)> "Identifier"

    let WidgetKey =
        Widgets.register "OrType" (fun widget ->
            let struct (lhs, rhs) = Widgets.getScalarValue widget Identifier
            Type.Or(TypeOrNode(lhs, SingleTextNode.``or``, rhs, Range.Zero)))

[<AutoOpen>]
module OrTypeBuilders =
    type Ast with
        static member Or(lhs: WidgetBuilder<Type>, rhs: WidgetBuilder<Type>) =
            WidgetBuilder<Type>(
                OrType.WidgetKey,
                AttributesBundle(
                    StackList.one(OrType.Identifier.WithValue(Gen.mkOak lhs, Gen.mkOak rhs)),
                    Array.empty,
                    Array.empty
                )
            )

        static member Or(lhs: string, rhs: WidgetBuilder<Type>) = Ast.Or(Ast.LongIdent(lhs), rhs)

        static member Or(lhs: WidgetBuilder<Type>, rhs: string) = Ast.Or(lhs, Ast.LongIdent(rhs))

        static member Or(lhs: string, rhs: string) =
            Ast.Or(Ast.LongIdent(lhs), Ast.LongIdent(rhs))
