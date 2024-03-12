namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fantomas.FCS.Text
open Fabulous.AST.StackAllocatedCollections
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak

open type Fabulous.AST.Ast
open type Fantomas.Core.SyntaxOak.Oak

module NestedModule =
    let Name = Attributes.defineScalar<string> "Name"
    let IsRecursive = Attributes.defineScalar<bool> "IsRecursive"
    let Accessibility = Attributes.defineScalar<AccessControl> "Accessibility"
    let Decls = Attributes.defineWidgetCollection "Decls"

    let WidgetKey =
        Widgets.register "NestedModule" (fun widget ->
            let name = Widgets.getScalarValue widget Name

            let name =
                name |> StringParsing.normalizeIdentifierBackticks |> SingleTextNode.Create

            let moduleDecls = Widgets.getNodesFromWidgetCollection<ModuleDecl> widget Decls

            let isRecursive =
                Widgets.tryGetScalarValue widget IsRecursive |> ValueOption.defaultValue false

            let accessControl =
                Widgets.tryGetScalarValue widget Accessibility
                |> ValueOption.defaultValue AccessControl.Unknown

            let accessControl =
                match accessControl with
                | Public -> Some(SingleTextNode.``public``)
                | Private -> Some(SingleTextNode.``private``)
                | Internal -> Some(SingleTextNode.``internal``)
                | Unknown -> None

            NestedModuleNode(
                None,
                None,
                SingleTextNode.``module``,
                accessControl,
                isRecursive,
                IdentListNode([ IdentifierOrDot.Ident(name) ], Range.Zero),
                SingleTextNode.equals,
                moduleDecls,
                Range.Zero
            ))

[<AutoOpen>]
module NestedModuleBuilders =
    type Ast with
        static member NestedModule(name: string) =
            CollectionBuilder<NestedModuleNode, ModuleDecl>(
                NestedModule.WidgetKey,
                NestedModule.Decls,
                AttributesBundle(StackList.one(NestedModule.Name.WithValue(name)), ValueNone, ValueNone)
            )

[<Extension>]
type NestedModuleModifiers =
    [<Extension>]
    static member inline toRecursive(this: WidgetBuilder<NestedModuleNode>) =
        this.AddScalar(NestedModule.IsRecursive.WithValue(true))

    [<Extension>]
    static member inline toPrivate(this: WidgetBuilder<NestedModuleNode>) =
        this.AddScalar(NestedModule.Accessibility.WithValue(AccessControl.Private))

    [<Extension>]
    static member inline toPublic(this: WidgetBuilder<NestedModuleNode>) =
        this.AddScalar(NestedModule.Accessibility.WithValue(AccessControl.Public))

    [<Extension>]
    static member inline toInternal(this: WidgetBuilder<NestedModuleNode>) =
        this.AddScalar(NestedModule.Accessibility.WithValue(AccessControl.Internal))

[<Extension>]
type NestedModuleYieldExtensions =
    [<Extension>]
    static member inline Yield
        (_: CollectionBuilder<'parent, ModuleDecl>, x: WidgetBuilder<NestedModuleNode>)
        : CollectionContent =
        let node = Gen.mkOak x
        let moduleDecl = ModuleDecl.NestedModule node
        let widget = Ast.EscapeHatch(moduleDecl).Compile()
        { Widgets = MutStackArray1.One(widget) }
