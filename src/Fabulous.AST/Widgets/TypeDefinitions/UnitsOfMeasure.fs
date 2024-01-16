namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fantomas.FCS.Text
open Fabulous.AST.StackAllocatedCollections
open Fantomas.Core.SyntaxOak
open Fabulous.AST.StackAllocatedCollections.StackList

module UnitsOfMeasure =

    let Name = Attributes.defineWidget "Name"

    let PowerType = Attributes.defineScalar "PowerType"

    let XmlDocs = Attributes.defineScalar<string list> "XmlDoc"

    let XmlDocsAbbrev = Attributes.defineScalar<string list> "XmlDoc"

    let WidgetKey =
        Widgets.register "UnitsOfMeasure" (fun widget ->
            let name = Helpers.getNodeFromWidget<SingleTextNode> widget Name
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
                Some(name),
                IdentListNode([], Range.Zero),
                None,
                [],
                None,
                None,
                None,
                Range.Zero
            ))

    let WidgetAbbrevKey =
        Widgets.register "UnitsOfMeasureAbbrev" (fun widget ->
            let name = Helpers.getNodeFromWidget<SingleTextNode> widget Name
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
                    Some(name),
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
module UnitsOfMeasureBuilders =
    type Ast with

        static member inline UnitsOfMeasure(name: WidgetBuilder<#SingleTextNode>) =
            WidgetBuilder<TypeNameNode>(
                UnitsOfMeasure.WidgetKey,
                AttributesBundle(
                    StackList.empty(),
                    ValueSome [| UnitsOfMeasure.Name.WithValue(name.Compile()) |],
                    ValueNone
                )
            )

        static member inline UnitsOfMeasure(name: SingleTextNode) =
            Ast.UnitsOfMeasure(Ast.EscapeHatch(name))

        static member inline UnitsOfMeasure(name: string) =
            Ast.UnitsOfMeasure(SingleTextNode(name, Range.Zero))

        static member inline UnitsOfMeasure(name: WidgetBuilder<#SingleTextNode>, powerType: Type) =
            WidgetBuilder<TypeDefnAbbrevNode>(
                UnitsOfMeasure.WidgetAbbrevKey,
                AttributesBundle(
                    StackList.one(UnitsOfMeasure.PowerType.WithValue(powerType)),
                    ValueSome [| UnitsOfMeasure.Name.WithValue(name.Compile()) |],
                    ValueNone
                )
            )

        static member inline UnitsOfMeasure(name: SingleTextNode, powerType: Type) =
            Ast.UnitsOfMeasure(Ast.EscapeHatch(name), powerType)

        static member inline UnitsOfMeasure(name: string, powerType: Type) =
            Ast.UnitsOfMeasure(SingleTextNode(name, Range.Zero), powerType)

[<Extension>]
type TypeNameNodeModifiers =
    [<Extension>]
    static member xmlDocs(this: WidgetBuilder<TypeNameNode>, comments: string list) =
        this.AddScalar(UnitsOfMeasure.XmlDocs.WithValue(comments))

[<Extension>]
type UnitsOfMeasureAbbrevModifiers =

    [<Extension>]
    static member xmlDocs(this: WidgetBuilder<TypeDefnAbbrevNode>, comments: string list) =
        this.AddScalar(UnitsOfMeasure.XmlDocsAbbrev.WithValue(comments))


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
