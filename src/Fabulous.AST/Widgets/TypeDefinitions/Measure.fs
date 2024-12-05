namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fabulous.AST
open Fabulous.AST.StackAllocatedCollections
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Syntax
open Fantomas.FCS.Text

module TypeNameNode =

    let Name = Attributes.defineScalar<string> "Name"

    let PowerType = Attributes.defineWidget "PowerType"

    let XmlDocs = Attributes.defineScalar<string list> "XmlDoc"

    let WidgetKey =
        Widgets.register "TypeDefnAbbrevNode" (fun widget ->
            let name = Widgets.getScalarValue widget Name
            let lines = Widgets.tryGetScalarValue widget XmlDocs

            let xmlDocs =
                match lines with
                | ValueSome values ->
                    let xmlDocNode = XmlDocNode.Create(values)
                    Some xmlDocNode
                | ValueNone -> None

            TypeNameNode(
                xmlDocs,
                Some(
                    MultipleAttributeListNode.Create(
                        [ AttributeNode(
                              IdentListNode([ IdentifierOrDot.Ident(SingleTextNode.measure) ], Range.Zero),
                              None,
                              None,
                              Range.Zero
                          ) ]
                    )
                ),
                SingleTextNode.``type``,
                Some(SingleTextNode.Create(name)),
                IdentListNode([], Range.Zero),
                None,
                [],
                None,
                None,
                None,
                Range.Zero
            ))

[<AutoOpen>]
module TypeNameNodeBuilders =
    type Ast with

        static member Measure(name: string) =
            let name = PrettyNaming.NormalizeIdentifierBackticks name

            WidgetBuilder<TypeNameNode>(TypeNameNode.WidgetKey, TypeNameNode.Name.WithValue(name))

type TypeNameNodeModifiers =
    [<Extension>]
    static member xmlDocs(this: WidgetBuilder<TypeNameNode>, comments: string list) =
        this.AddScalar(TypeNameNode.XmlDocs.WithValue(comments))

type TypeDefnAbbrevNodeYieldExtensions =
    [<Extension>]
    static member inline Yield(_: CollectionBuilder<'parent, ModuleDecl>, x: TypeNameNode) : CollectionContent =
        let typeDefn = TypeDefn.None(x)
        let typeDefn = ModuleDecl.TypeDefn(typeDefn)
        let widget = Ast.EscapeHatch(typeDefn).Compile()
        { Widgets = MutStackArray1.One(widget) }

    [<Extension>]
    static member inline Yield
        (this: CollectionBuilder<'parent, ModuleDecl>, x: WidgetBuilder<TypeNameNode>)
        : CollectionContent =
        let node = Gen.mkOak x
        TypeDefnAbbrevNodeYieldExtensions.Yield(this, node)
