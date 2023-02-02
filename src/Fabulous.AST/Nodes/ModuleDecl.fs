namespace Fabulous.AST

open FSharp.Compiler.Text
open Fantomas.Core.SyntaxOak


type IFabModuleDecl = inherit IFabValueBase


module ModuleDecl =
    let WidgetKey = Widgets.register(fun (widget: Widget) ->
        let xmlDoc: XmlDocNode option = None
        let attributes: MultipleAttributeListNode option = None
        let leadingKeyword: MultipleTextsNode = MultipleTextsNode([], Range())
        let accessibility: SingleTextNode option = None
        let isMutable: bool = false
        let inlineNode: SingleTextNode option = None
        let functionName: Choice<IdentListNode, Pattern> = failwith "Not implemented"
        let genericTypeParameters: TyparDecls option = None
        let parameters: Pattern list = []
        let returnType: BindingReturnInfoNode option = None
        let equals: SingleTextNode = SingleTextNode("", Range())
        let expr: Expr = failwith "Not implemented"
        let range = Helpers.getWidgetValue widget NodeBase.Range |> Helpers.createValueForWidget
        BindingNode(xmlDoc, attributes, leadingKeyword, isMutable, inlineNode, accessibility, functionName, genericTypeParameters, parameters, returnType, equals, expr, range)
    )
    
    let BindingNode = Attributes.defineWidget "NodeDeclarations"
    
[<AutoOpen>]
module ModuleDeclBuilders =
    type Fabulous.AST.Node with
        static member inline ModuleDecl(node: WidgetBuilder<IFabModuleDecl>) =
            WidgetBuilder<IFabModuleDecl>(
                ModuleDecl.WidgetKey,
                AttributesBundle(
                    StackAllocatedCollections.StackList.StackList.empty(),
                    ValueSome [|
                        ModuleDecl.BindingNode.WithValue(node.Compile())
                    |],
                    ValueNone
                )
            )
