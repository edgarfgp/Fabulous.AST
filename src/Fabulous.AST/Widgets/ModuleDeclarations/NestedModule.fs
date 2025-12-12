namespace Fabulous.AST

open Fabulous.AST
open Fabulous.AST.StackAllocatedCollections
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Syntax
open Fantomas.FCS.Text

module NestedModule =
    let Name = Attributes.defineScalar<string> "Name"
    let Decls = Attributes.defineWidgetCollection "Decls"

    let WidgetKey =
        Widgets.register "NestedModule" (fun widget ->
            let name = Widgets.getScalarValue widget Name

            let name =
                name |> PrettyNaming.NormalizeIdentifierBackticks |> SingleTextNode.Create

            let moduleDecls = Widgets.getNodesFromWidgetCollection<ModuleDecl> widget Decls

            let isRecursive =
                Widgets.tryGetScalarValue widget ModuleDecl.IsRecursive
                |> ValueOption.defaultValue false

            let accessControl =
                Widgets.tryGetScalarValue widget ModuleDecl.Accessibility
                |> ValueOption.defaultValue AccessControl.Unknown

            let accessControl =
                match accessControl with
                | Public -> Some(SingleTextNode.``public``)
                | Private -> Some(SingleTextNode.``private``)
                | Internal -> Some(SingleTextNode.``internal``)
                | Unknown -> None

            let xmlDocs =
                Widgets.tryGetNodeFromWidget widget ModuleDecl.XmlDocs
                |> ValueOption.map(Some)
                |> ValueOption.defaultValue None

            let attributes =
                Widgets.tryGetScalarValue widget ModuleDecl.MultipleAttributes
                |> ValueOption.map(fun x -> Some(MultipleAttributeListNode.Create(x)))
                |> ValueOption.defaultValue None

            let node =
                NestedModuleNode(
                    xmlDocs,
                    attributes,
                    SingleTextNode.``module``,
                    accessControl,
                    isRecursive,
                    IdentListNode([ IdentifierOrDot.Ident(name) ], Range.Zero),
                    SingleTextNode.equals,
                    moduleDecls,
                    Range.Zero
                )

            ModuleDecl.NestedModule(node))

[<AutoOpen>]
module NestedModuleBuilders =
    type Ast with
        /// <summary>Creates a module widget with the specified name.</summary>
        /// <param name="name">The name of the module.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         Module("MyModule") {
        ///             Value("x", "1")
        ///         }
        ///     }
        /// }
        /// </code>
        static member Module(name: string) =
            CollectionBuilder<ModuleDecl, ModuleDecl>(
                NestedModule.WidgetKey,
                NestedModule.Decls,
                NestedModule.Name.WithValue(name)
            )
