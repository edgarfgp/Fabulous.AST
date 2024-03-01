namespace Fabulous.AST

open Fantomas.Core.SyntaxOak
open Fabulous.AST.StackAllocatedCollections.StackList

module TypeStaticConstant =
    let TypeWidget = Attributes.defineWidget "Type"

    let WidgetKey =
        Widgets.register "TypeStaticConstant" (fun widget ->
            let typeWidget = Helpers.getNodeFromWidget<Constant> widget TypeWidget
            Type.StaticConstant(typeWidget))

[<AutoOpen>]
module TypeStaticConstantBuilders =
    type Ast with
        static member TypeStaticConstant(t: WidgetBuilder<Constant>) =
            WidgetBuilder<Type>(
                TypeStaticConstant.WidgetKey,
                AttributesBundle(
                    StackList.empty(),
                    ValueSome [| TypeStaticConstant.TypeWidget.WithValue(t.Compile()) |],
                    ValueNone
                )
            )
