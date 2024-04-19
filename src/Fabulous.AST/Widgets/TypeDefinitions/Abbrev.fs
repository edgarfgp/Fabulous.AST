namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fantomas.FCS.Text
open Fabulous.AST.StackAllocatedCollections
open Fantomas.Core.SyntaxOak
open Fabulous.AST.StackAllocatedCollections.StackList

module Abbrev =

    let Name = Attributes.defineScalar<string> "Name"

    let AliasType = Attributes.defineScalar<StringOrWidget<Type>> "AliasType"

    let WidgetKey =
        Widgets.register "Alias" (fun widget ->
            let name = Widgets.getScalarValue widget Name

            let aliasType = Widgets.getScalarValue widget AliasType

            let aliasType =
                match aliasType with
                | StringOrWidget.StringExpr value ->
                    let value = StringParsing.normalizeIdentifierBackticks value
                    Type.LongIdent(IdentListNode([ IdentifierOrDot.Ident(SingleTextNode.Create(value)) ], Range.Zero))
                | StringOrWidget.WidgetExpr value -> value

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
                    StackList.two(
                        Abbrev.Name.WithValue(name),
                        Abbrev.AliasType.WithValue(StringOrWidget.WidgetExpr(Gen.mkOak alias))
                    ),
                    Array.empty,
                    Array.empty
                )
            )

        static member Abbrev(name: string, alias: string) =
            WidgetBuilder<TypeDefnAbbrevNode>(
                Abbrev.WidgetKey,
                AttributesBundle(
                    StackList.two(
                        Abbrev.Name.WithValue(name),
                        Abbrev.AliasType.WithValue(StringOrWidget.StringExpr(Unquoted alias))
                    ),
                    Array.empty,
                    Array.empty
                )
            )

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
