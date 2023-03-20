namespace Fabulous.AST

open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak

type IFabConstant = interface end

module Constant =
    let Text = Attributes.defineWidget "Text"
    // let Unit = Attributes.defineWidget "Unit"
    // let Measure = Attributes.defineWidget "Measure"
    
    let ConstantTextWidgetKey = Widgets.register "Constant.FromText" (fun widget ->
        let text = Helpers.getNodeFromWidget<SingleTextNode> widget Text
        Constant.FromText text
    )

[<AutoOpen>]
module ConstantBuilders =
    type Fabulous.AST.Ast with
        static member inline Constant(text: WidgetBuilder<#IFabSingleText>) =
            WidgetBuilder<IFabConstant>(
                Constant.ConstantTextWidgetKey,
                AttributesBundle(
                    StackList.empty(),
                    ValueSome [| Constant.Text.WithValue(text.Compile()) |],
                    ValueNone
                )
            )