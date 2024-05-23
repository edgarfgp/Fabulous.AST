namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fantomas.FCS.Syntax
open Fantomas.FCS.Text
open Fabulous.AST.StackAllocatedCollections
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak

open type Fabulous.AST.Ast
open type Fantomas.Core.SyntaxOak.Oak

module ModuleAbbrev =
    let Name = Attributes.defineScalar<string> "Name"

    let Alias = Attributes.defineScalar<string> "Alias"

    let WidgetKey =
        Widgets.register "ModuleAbbrev" (fun widget ->
            let name = Widgets.getScalarValue widget Name
            let alias = Widgets.getScalarValue widget Alias

            ModuleAbbrevNode(
                SingleTextNode.``module``,
                SingleTextNode.Create(name),
                IdentListNode([ IdentifierOrDot.Ident(SingleTextNode.Create(alias)) ], Range.Zero),
                Range.Zero
            ))

[<AutoOpen>]
module ModuleAbbrevBuilders =
    type Ast with
        static member ModuleAbbrev(name: string, alias: string) =
            WidgetBuilder<ModuleAbbrevNode>(
                ModuleAbbrev.WidgetKey,
                AttributesBundle(
                    StackList.two(ModuleAbbrev.Name.WithValue(name), ModuleAbbrev.Alias.WithValue(alias)),
                    Array.empty,
                    Array.empty
                )
            )

type ModuleAbbrevYieldExtensions =
    [<Extension>]
    static member inline Yield
        (_: CollectionBuilder<'parent, ModuleDecl>, x: WidgetBuilder<ModuleAbbrevNode>)
        : CollectionContent =
        let node = Gen.mkOak x
        let moduleDecl = ModuleDecl.ModuleAbbrev node
        let widget = Ast.EscapeHatch(moduleDecl).Compile()
        { Widgets = MutStackArray1.One(widget) }
