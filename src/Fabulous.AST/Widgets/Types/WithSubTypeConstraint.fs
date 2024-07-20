namespace Fabulous.AST

open Fabulous.Builders
open Fabulous.Builders.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak

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
