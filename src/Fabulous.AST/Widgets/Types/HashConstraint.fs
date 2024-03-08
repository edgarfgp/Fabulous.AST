namespace Fabulous.AST

open Fantomas.FCS.Text
open Fantomas.Core.SyntaxOak
open Fabulous.AST.StackAllocatedCollections.StackList

module TypeHashConstraint =
    let TypeWidget = Attributes.defineWidget "Type"

    let WidgetKey =
        Widgets.register "TypeHashConstraint" (fun widget ->
            let typeWidget = Helpers.getNodeFromWidget widget TypeWidget
            Type.HashConstraint(TypeHashConstraintNode(SingleTextNode.hash, typeWidget, Range.Zero)))

[<AutoOpen>]
module TypeHashConstraintBuilders =
    type Ast with
        static member HashConstraint(value: WidgetBuilder<Type>) =
            WidgetBuilder<Type>(
                TypeHashConstraint.WidgetKey,
                AttributesBundle(
                    StackList.empty(),
                    ValueSome [| TypeHashConstraint.TypeWidget.WithValue(value.Compile()) |],
                    ValueNone
                )
            )
