namespace Fabulous.AST

open Fabulous.Builders
open Fabulous.Builders.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module TypeHashConstraint =
    let TypeWidget = Attributes.defineWidget "Type"

    let WidgetKey =
        Widgets.register "TypeHashConstraint" (fun widget ->
            let typeWidget = Widgets.getNodeFromWidget widget TypeWidget
            Type.HashConstraint(TypeHashConstraintNode(SingleTextNode.hash, typeWidget, Range.Zero)))

[<AutoOpen>]
module TypeHashConstraintBuilders =
    type Ast with
        static member HashConstraint(value: WidgetBuilder<Type>) =
            WidgetBuilder<Type>(
                TypeHashConstraint.WidgetKey,
                AttributesBundle(
                    StackList.empty(),
                    [| TypeHashConstraint.TypeWidget.WithValue(value.Compile()) |],
                    Array.empty
                )
            )

        static member HashConstraint(value: string) = Ast.HashConstraint(Ast.LongIdent value)
