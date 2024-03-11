namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fantomas.FCS.Text
open Fabulous.AST.StackAllocatedCollections
open Fantomas.Core.SyntaxOak
open Fabulous.AST.StackAllocatedCollections.StackList

module Abbrev =

    let Name = Attributes.defineScalar<string> "Name"

    let AliasType = Attributes.defineWidget "AliasType"

    let WidgetKey =
        Widgets.register "Alias" (fun widget ->
            let name = Widgets.getScalarValue widget Name

            let aliasType = Widgets.getNodeFromWidget widget AliasType

            TypeDefnAbbrevNode(
                TypeNameNode(
                    None,
                    None,
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
                aliasType,
                [],
                Range.Zero
            ))

[<AutoOpen>]
module AbbrevBuilders =
    type Ast with

        static member Abbrev(name: string, alias: WidgetBuilder<Type>) =
            WidgetBuilder<TypeDefnAbbrevNode>(
                Abbrev.WidgetKey,
                AttributesBundle(
                    StackList.one(Abbrev.Name.WithValue(name)),
                    ValueSome [| Abbrev.AliasType.WithValue(alias.Compile()) |],
                    ValueNone
                )
            )

        static member Abbrev(name: string, alias: string) = Ast.Abbrev(name, Ast.LongIdent(alias))

[<Extension>]
type AbbrevYieldExtensions =
    [<Extension>]
    static member inline Yield
        (_: CollectionBuilder<'parent, ModuleDecl>, x: WidgetBuilder<TypeDefnAbbrevNode>)
        : CollectionContent =
        let node = Gen.mkOak x
        let typeDefn = TypeDefn.Abbrev(node)
        let typeDefn = ModuleDecl.TypeDefn(typeDefn)
        let widget = Ast.EscapeHatch(typeDefn).Compile()
        { Widgets = MutStackArray1.One(widget) }

    [<Extension>]
    static member inline Yield(_: CollectionBuilder<'parent, ModuleDecl>, x: TypeDefnAbbrevNode) : CollectionContent =
        let moduleDecl = ModuleDecl.TypeDefn(TypeDefn.Abbrev(x))
        let widget = Ast.EscapeHatch(moduleDecl).Compile()
        { Widgets = MutStackArray1.One(widget) }
