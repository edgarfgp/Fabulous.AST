namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fantomas.FCS.Text
open Fabulous.AST.StackAllocatedCollections
open Fantomas.Core.SyntaxOak
open Fabulous.AST.StackAllocatedCollections.StackList

module UnitsOfMeasure =

    let Name = Attributes.defineWidget "Name"

    let WidgetKey =
        Widgets.register "UnitsOfMeasure" (fun widget ->
            let name = Helpers.getNodeFromWidget<SingleTextNode> widget Name

            TypeNameNode(
                None,
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
