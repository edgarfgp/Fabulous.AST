namespace Fabulous.AST

open FSharp.Compiler.Text
open Fantomas.Core.SyntaxOak

type IFabModuleOrNamespaceHeaderNode = inherit IFabValueBase


module ModuleOrNamespaceHeader =
    let WidgetKey = Widgets.register(fun (widget: Widget) ->
        let xmlDoc: XmlDocNode option = None
        let attributes: MultipleAttributeListNode option = None
        let leadingKeyword: MultipleTextsNode = MultipleTextsNode([], Range())
        let accessibility: SingleTextNode option = None
        let isRecursive: bool = false
        let name: IdentListNode option = None
        let range = Helpers.getWidgetValue widget NodeBase.Range |> Helpers.createValueForWidget            
        ModuleOrNamespaceHeaderNode(xmlDoc, attributes, leadingKeyword, accessibility, isRecursive, name, range)
    )
    
[<AutoOpen>]
module ModuleOrNamespaceHeaderBuilders =
    type Fabulous.AST.Node with
        static member inline ModuleOrNamespaceHeader(range: WidgetBuilder<IFabRange>) =
            WidgetBuilder<IFabModuleOrNamespaceHeaderNode>(
                ModuleOrNamespaceHeader.WidgetKey,
                AttributesBundle(
                    StackAllocatedCollections.StackList.StackList.empty(),
                    ValueSome [|
                        NodeBase.Range.WithValue(range.Compile())
                    |],
                    ValueNone
                )
            )
