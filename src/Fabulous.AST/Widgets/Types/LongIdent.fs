namespace Fabulous.AST

open Fantomas.FCS.Text
open Fantomas.Core.SyntaxOak
open Fabulous.AST.StackAllocatedCollections.StackList

module TypeLongIdent =

    let Value = Attributes.defineScalar<string list> "Return"

    let WidgetKey =
        Widgets.register "LongIdentType" (fun widget ->
            let value = Widgets.getScalarValue widget Value

            Type.LongIdent(
                IdentListNode(
                    [ for v in value do
                          IdentifierOrDot.Ident(SingleTextNode.Create(v)) ],
                    Range.Zero
                )
            ))

[<AutoOpen>]
module LongIdentTypeBuilders =
    type Ast with

        static member LongIdent(value: string) =
            WidgetBuilder<Type>(
                TypeLongIdent.WidgetKey,
                AttributesBundle(StackList.one(TypeLongIdent.Value.WithValue([ value ])), Array.empty, Array.empty)
            )

        static member LongIdent(value: string list) =
            WidgetBuilder<Type>(
                TypeLongIdent.WidgetKey,
                AttributesBundle(StackList.one(TypeLongIdent.Value.WithValue(value)), Array.empty, Array.empty)
            )
