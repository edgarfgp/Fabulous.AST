namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fantomas.FCS.Text
open Fabulous.AST.StackAllocatedCollections
open Fantomas.Core.SyntaxOak
open Fabulous.AST.StackAllocatedCollections.StackList

module Measure =

    let Name = Attributes.defineScalar<string> "Name"

    let PowerType = Attributes.defineScalar<Type> "PowerType"

    let XmlDocs = Attributes.defineScalar<string list> "XmlDoc"

    let XmlDocsAbbrev = Attributes.defineScalar<string list> "XmlDoc"

    let WidgetKey =
        Widgets.register "Measure" (fun widget ->
            let name = Helpers.getScalarValue widget Name
            let lines = Helpers.tryGetScalarValue widget XmlDocs

            let xmlDocs =
                match lines with
                | ValueSome values ->
                    let xmlDocNode = XmlDocNode.Create(values)
                    Some xmlDocNode
                | ValueNone -> None

            TypeNameNode(
                xmlDocs,
                Some(MultipleAttributeListNode.Create([ "Measure" ])),
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

    let WidgetAbbrevKey =
        Widgets.register "Measure" (fun widget ->
            let name = Helpers.getScalarValue widget Name
            let powerType = Helpers.getScalarValue widget PowerType
            let lines = Helpers.tryGetScalarValue widget XmlDocsAbbrev

            let xmlDocs =
                match lines with
                | ValueSome values ->
                    let xmlDocNode = XmlDocNode.Create(values)
                    Some xmlDocNode
                | ValueNone -> None

            TypeDefnAbbrevNode(
                TypeNameNode(
                    xmlDocs,
                    Some(MultipleAttributeListNode.Create([ "Measure" ])),
                    SingleTextNode.``type``,
                    Some(SingleTextNode.Create(name)),
                    IdentListNode([ IdentifierOrDot.Ident(SingleTextNode.equals) ], Range.Zero),
                    None,
                    [],
                    None,
                    None,
                    None,
                    Range.Zero
                ),
                powerType,
                [],
                Range.Zero
            ))


[<AutoOpen>]
module MeasureBuilders =
    type Ast with

        static member inline Measure(name: string) =
            WidgetBuilder<TypeNameNode>(
                Measure.WidgetKey,
                AttributesBundle(StackList.one(Measure.Name.WithValue(name)), ValueNone, ValueNone)
            )


        static member inline Measure(name: string, powerType: Type) =
            WidgetBuilder<TypeDefnAbbrevNode>(
                Measure.WidgetAbbrevKey,
                AttributesBundle(
                    StackList.two(Measure.Name.WithValue(name), Measure.PowerType.WithValue(powerType)),
                    ValueNone,
                    ValueNone
                )
            )

        static member inline Measure(name: string, powerType: string) =
            Ast.Measure(name, CommonType.mkLongIdent(powerType))

[<Extension>]
type TypeNameNodeModifiers =
    [<Extension>]
    static member xmlDocs(this: WidgetBuilder<TypeNameNode>, comments: string list) =
        this.AddScalar(Measure.XmlDocs.WithValue(comments))

[<Extension>]
type UnitsOfMeasureAbbrevModifiers =

    [<Extension>]
    static member xmlDocs(this: WidgetBuilder<TypeDefnAbbrevNode>, comments: string list) =
        this.AddScalar(Measure.XmlDocsAbbrev.WithValue(comments))

[<Extension>]
type MeasureYieldExtensions =
    [<Extension>]
    static member inline Yield
        (
            _: CollectionBuilder<'parent, ModuleDecl>,
            x: WidgetBuilder<TypeNameNode>
        ) : CollectionContent =
        let node = Tree.compile x
        let typeDefn = TypeDefn.None(node)
        let typeDefn = ModuleDecl.TypeDefn(typeDefn)
        let widget = Ast.EscapeHatch(typeDefn).Compile()
        { Widgets = MutStackArray1.One(widget) }

    [<Extension>]
    static member inline Yield(_: CollectionBuilder<'parent, ModuleDecl>, x: TypeNameNode) : CollectionContent =
        let moduleDecl = ModuleDecl.TypeDefn(TypeDefn.None(x))
        let widget = Ast.EscapeHatch(moduleDecl).Compile()
        { Widgets = MutStackArray1.One(widget) }
