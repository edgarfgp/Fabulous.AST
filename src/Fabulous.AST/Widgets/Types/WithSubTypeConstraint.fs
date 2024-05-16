namespace Fabulous.AST

open Fantomas.Core.SyntaxOak
open Fabulous.AST.StackAllocatedCollections.StackList

module WithSubTypeConstraint =
    let ConstraintValue = Attributes.defineWidget "ConstraintValue"

    let WidgetKey =
        Widgets.register "WithSubTypeConstraint" (fun widget ->
            let value = Widgets.getNodeFromWidget widget ConstraintValue
            Type.WithSubTypeConstraint(value))

[<AutoOpen>]
module WithSubTypeConstraintBuilders =
    type Ast with
        static member WithSubTypeConstraint(value: WidgetBuilder<TypeConstraint>) =
            WidgetBuilder<Type>(
                WithSubTypeConstraint.WidgetKey,
                AttributesBundle(
                    StackList.empty(),
                    [| WithSubTypeConstraint.ConstraintValue.WithValue(value.Compile()) |],
                    Array.empty
                )
            )
