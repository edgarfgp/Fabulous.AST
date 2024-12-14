namespace Fabulous.AST

open Fabulous.AST
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module StaticOptimizationConstraint =
    let Typar = Attributes.defineScalar<string> "Identifier"

    let TypedValue = Attributes.defineWidget "TypedValue"

    let WidgetWhenTyparIsStructKey =
        Widgets.register "WhenTyparIsStruct" (fun widget ->
            let typar = Widgets.getScalarValue widget Typar
            StaticOptimizationConstraint.WhenTyparIsStruct(SingleTextNode.Create(typar)))

    let WidgetWhenTyparTyconEqualsTyconKey =
        Widgets.register "WhenTyparTyconEqualsTycon" (fun widget ->
            let typar = Widgets.getScalarValue widget Typar
            let typedValue = Widgets.getNodeFromWidget<Type> widget TypedValue

            StaticOptimizationConstraint.WhenTyparTyconEqualsTycon(
                StaticOptimizationConstraintWhenTyparTyconEqualsTyconNode(
                    SingleTextNode.Create(typar),
                    typedValue,
                    Range.Zero
                )
            ))

[<AutoOpen>]
module StaticOptimizationConstraintBuilders =
    type Ast with

        static member WhenTyparIsStruct(identifier: string) =
            WidgetBuilder<StaticOptimizationConstraint>(
                StaticOptimizationConstraint.WidgetWhenTyparIsStructKey,
                StaticOptimizationConstraint.Typar.WithValue(identifier)
            )

        static member WhenTyparTyconEqualsTycon(identifier: string, typedValue: WidgetBuilder<Type>) =
            WidgetBuilder<StaticOptimizationConstraint>(
                StaticOptimizationConstraint.WidgetWhenTyparTyconEqualsTyconKey,
                AttributesBundle(
                    StackList.one(StaticOptimizationConstraint.Typar.WithValue(identifier)),
                    [| StaticOptimizationConstraint.TypedValue.WithValue(typedValue.Compile()) |],
                    Array.empty
                )
            )

        static member WhenTyparTyconEqualsTycon(identifier: string, typedValue: string) =
            Ast.WhenTyparTyconEqualsTycon(identifier, Ast.EscapeHatch(Type.Create(typedValue)))
