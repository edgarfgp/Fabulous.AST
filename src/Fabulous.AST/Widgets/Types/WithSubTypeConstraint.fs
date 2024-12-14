namespace Fabulous.AST

open Fabulous.AST
open Fabulous.AST.StackAllocatedCollections.StackList
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
        static member WithSubType(value: WidgetBuilder<TypeConstraint>) =
            WidgetBuilder<Type>(
                WithSubTypeConstraint.WidgetKey,
                WithSubTypeConstraint.ConstraintValue.WithValue(value.Compile())
            )
