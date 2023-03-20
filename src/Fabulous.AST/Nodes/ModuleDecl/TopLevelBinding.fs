namespace Fabulous.AST

open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak

type IFabTopLevelBinding = inherit IFabModuleDecl

module TopLevelBinding =
    let BindingNode = Attributes.defineWidget "BindingNode"
    
    let WidgetKey = Widgets.register "TopLevelBinding" (fun (widget: Widget) ->
        let bindingNode =
            Helpers.getWidgetValue widget BindingNode
            |> Helpers.createValueForWidget<BindingNode>
            
        ModuleDecl.TopLevelBinding bindingNode
    )
    
[<AutoOpen>]
module TopLevelBindingBuilders =
    type Fabulous.AST.Node with
        static member inline TopLevelBinding(bindingNode: WidgetBuilder<#IFabBindingNode>) =
            WidgetBuilder<IFabTopLevelBinding>(
                TopLevelBinding.WidgetKey,
                AttributesBundle(
                    StackList.empty(),
                    ValueSome [| TopLevelBinding.BindingNode.WithValue(bindingNode.Compile()) |],
                    ValueNone
                )
            ) 
    

