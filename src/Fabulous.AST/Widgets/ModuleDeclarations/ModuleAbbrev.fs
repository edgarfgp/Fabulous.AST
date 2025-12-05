namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fabulous.AST
open Fabulous.AST.StackAllocatedCollections
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

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
        /// <summary>Creates a ModuleAbbrev widget with the specified name and alias.</summary>
        /// <param name="name">The name of the module.</param>
        /// <param name="alias">The alias of the module.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         ModuleAbbrev("Module1", "M1")
        ///     }
        /// }
        /// </code>
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
    static member inline Yield(_: CollectionBuilder<'parent, ModuleDecl>, x: ModuleAbbrevNode) : CollectionContent =
        let moduleDecl = ModuleDecl.ModuleAbbrev x
        let widget = Ast.EscapeHatch(moduleDecl).Compile()
        { Widgets = MutStackArray1.One(widget) }

    [<Extension>]
    static member inline Yield
        (this: CollectionBuilder<'parent, ModuleDecl>, x: WidgetBuilder<ModuleAbbrevNode>)
        : CollectionContent =
        let node = Gen.mkOak x
        ModuleAbbrevYieldExtensions.Yield(this, node)

    [<Extension>]
    static member inline YieldFrom
        (_: CollectionBuilder<'parent, ModuleDecl>, x: ModuleAbbrevNode seq)
        : CollectionContent =
        let widgets =
            x
            |> Seq.map(fun node ->
                let moduleDecl = ModuleDecl.ModuleAbbrev node
                Ast.EscapeHatch(moduleDecl).Compile())
            |> Seq.toArray
            |> MutStackArray1.fromArray

        { Widgets = widgets }

    [<Extension>]
    static member inline YieldFrom
        (this: CollectionBuilder<'parent, ModuleDecl>, x: WidgetBuilder<ModuleAbbrevNode> seq)
        : CollectionContent =
        let nodes = x |> Seq.map Gen.mkOak
        ModuleAbbrevYieldExtensions.YieldFrom(this, nodes)
