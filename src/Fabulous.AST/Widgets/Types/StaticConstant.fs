namespace Fabulous.AST

open Fabulous.Builders
open Fabulous.Builders.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak

module TypeStaticConstant =
    let TypeWidget = Attributes.defineWidget "Type"

    let WidgetKey =
        Widgets.register "TypeStaticConstant" (fun widget ->
            let typeWidget = Widgets.getNodeFromWidget<Constant> widget TypeWidget
            Type.StaticConstant(typeWidget))

[<AutoOpen>]
module TypeStaticConstantBuilders =
    type Ast with
        static member StaticConstant(value: WidgetBuilder<Constant>) =
            WidgetBuilder<Type>(
                TypeStaticConstant.WidgetKey,
                AttributesBundle(
                    StackList.empty(),
                    [| TypeStaticConstant.TypeWidget.WithValue(value.Compile()) |],
                    Array.empty
                )
            )

        static member StaticConstant(value: string) = Ast.StaticConstant(Ast.Constant value)
