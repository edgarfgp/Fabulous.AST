namespace Fabulous.AST

open Fantomas.Core.SyntaxOak
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.FCS.Text

module TypeStaticConstantNamed =
    let Identifier = Attributes.defineWidget "Identifier"
    let Value = Attributes.defineWidget "Value"

    let WidgetKey =
        Widgets.register "TypeStaticConstantNamed" (fun widget ->
            let identifier = Helpers.getNodeFromWidget<Type> widget Identifier
            let value = Helpers.getNodeFromWidget<Type> widget Value
            Type.StaticConstantNamed(TypeStaticConstantNamedNode(identifier, value, Range.Zero)))

[<AutoOpen>]
module TypeStaticConstantNamedBuilders =
    type Ast with
        static member TypeStaticConstantNamed(identifier: WidgetBuilder<Type>, value: WidgetBuilder<Type>) =
            WidgetBuilder<Type>(
                TypeStaticConstantNamed.WidgetKey,
                AttributesBundle(
                    StackList.empty(),
                    ValueSome
                        [| TypeStaticConstantNamed.Identifier.WithValue(identifier.Compile())
                           TypeStaticConstantNamed.Value.WithValue(value.Compile()) |],
                    ValueNone
                )
            )
