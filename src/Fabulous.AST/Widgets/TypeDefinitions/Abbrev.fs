namespace Fabulous.AST

open System.Runtime.CompilerServices
open FSharp.Compiler.Text
open Fabulous.AST.StackAllocatedCollections
open Fantomas.Core.SyntaxOak
open Fabulous.AST.StackAllocatedCollections.StackList

module Abbrev =

    let Name = Attributes.defineWidget "Name"

    let AliasType = Attributes.defineScalar<Type> "AliasType"

    let WidgetKey =
        Widgets.register "Alias" (fun widget ->
            let name = Helpers.getNodeFromWidget<SingleTextNode> widget Name

            let aliasType = Helpers.getScalarValue widget AliasType

            TypeDefnAbbrevNode(
                TypeNameNode(
                    None,
                    None,
                    SingleTextNode("type", Range.Zero),
                    Some(name),
                    IdentListNode([ IdentifierOrDot.Ident(SingleTextNode("=", Range.Zero)) ], Range.Zero),
                    None,
                    [],
                    None,
                    None,
                    None,
                    Range.Zero
                ),
                aliasType,
                [],
                Range.Zero
            ))

[<AutoOpen>]
module AbbrevBuilders =
    type Fabulous.AST.Ast with

        static member inline Abbrev(name: WidgetBuilder<#SingleTextNode>, aliasType: Type) =
            WidgetBuilder<TypeDefnAbbrevNode>(
                Abbrev.WidgetKey,
                AttributesBundle(
                    StackList.one(Abbrev.AliasType.WithValue(aliasType)),
                    ValueSome [| Abbrev.Name.WithValue(name.Compile()) |],
                    ValueNone
                )
            )

        static member inline Abbrev(name: SingleTextNode, aliasType: Type) =
            Ast.Abbrev(Ast.EscapeHatch(name), aliasType)

        static member inline Abbrev(name: string, aliasType: Type) =
            Ast.Abbrev(SingleTextNode(name, Range.Zero), aliasType)

[<Extension>]
type AbbrevYieldExtensions =
    [<Extension>]
    static member inline Yield
        (
            _: CollectionBuilder<'parent, ModuleDecl>,
            x: WidgetBuilder<TypeDefnAbbrevNode>
        ) : CollectionContent =
        let node = Tree.compile x
        let typeDefn = TypeDefn.Abbrev(node)
        let typeDefn = ModuleDecl.TypeDefn(typeDefn)
        let widget = Ast.EscapeHatch(typeDefn).Compile()
        { Widgets = MutStackArray1.One(widget) }

    [<Extension>]
    static member inline Yield(_: CollectionBuilder<'parent, ModuleDecl>, x: TypeDefnAbbrevNode) : CollectionContent =
        let moduleDecl = ModuleDecl.TypeDefn(TypeDefn.Abbrev(x))
        let widget = Ast.EscapeHatch(moduleDecl).Compile()
        { Widgets = MutStackArray1.One(widget) }
