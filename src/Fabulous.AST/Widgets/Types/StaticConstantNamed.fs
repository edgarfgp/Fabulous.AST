namespace Fabulous.AST

open Fantomas.Core.SyntaxOak
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.FCS.Text

module TypeStaticConstantNamed =
    let Identifier = Attributes.defineWidget "Identifier"
    let Value = Attributes.defineWidget "Value"

    let WidgetKey =
        Widgets.register "TypeStaticConstantNamed" (fun widget ->
            let identifier = Widgets.getNodeFromWidget<Type> widget Identifier
            let value = Widgets.getNodeFromWidget<Type> widget Value
            Type.StaticConstantNamed(TypeStaticConstantNamedNode(identifier, value, Range.Zero)))

[<AutoOpen>]
module TypeStaticConstantNamedBuilders =
    type Ast with
        /// <summary>
        /// Creates a type with an identifier and a value.
        /// </summary>
        /// <param name="identifier">The identifier of the type.</param>
        /// <param name="value">The value of the type.</param>
        static member StaticConstantNamed(identifier: WidgetBuilder<Type>, value: WidgetBuilder<Type>) =
            WidgetBuilder<Type>(
                TypeStaticConstantNamed.WidgetKey,
                AttributesBundle(
                    StackList.empty(),
                    [| TypeStaticConstantNamed.Identifier.WithValue(identifier.Compile())
                       TypeStaticConstantNamed.Value.WithValue(value.Compile()) |],
                    Array.empty
                )
            )
