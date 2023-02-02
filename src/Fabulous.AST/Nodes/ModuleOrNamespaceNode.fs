namespace Fabulous.AST

open Fantomas.Core.SyntaxOak

type IFabModuleOrNamespace = inherit IFabNodeBase

module ModuleOrNamespace =
    let WidgetKey = Widgets.register(fun (widget: Widget) ->
        let header = None
        let decls = Helpers.getWidgetValue widget ModuleDecl.BindingNode |> Helpers.createValueForWidget
        let range = Helpers.getWidgetValue widget NodeBase.Range |> Helpers.createValueForWidget
        ModuleOrNamespaceNode(header, decls, range)
    )
    
[<AutoOpen>]
module ModuleOrNamespaceBuilders =
    type Fabulous.AST.Node with
        static member inline ModuleOrNamespace(range: WidgetBuilder<IFabRange>, declarations: WidgetBuilder<IFabModuleDecl> list) =
            WidgetBuilder<IFabModuleOrNamespace>(
                ModuleOrNamespace.WidgetKey,
                AttributesBundle(
                    StackAllocatedCollections.StackList.StackList.empty(),
                    ValueSome [|
                        NodeBase.Range.WithValue(range.Compile())
                        for decl in declarations do
                            ModuleDecl.BindingNode.WithValue(decl.Compile())
                    |],
                    ValueNone
                )
            )
