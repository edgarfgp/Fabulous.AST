namespace Fabulous.AST

open Fabulous.AST
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module ModuleAbbrev =
    let Name = Attributes.defineScalar<string> "Name"

    let Alias = Attributes.defineScalar<string> "Alias"

    let WidgetKey =
        Widgets.register "ModuleAbbrev" (fun widget ->
            let name = Widgets.getScalarValue widget Name
            let alias = Widgets.getScalarValue widget Alias

            let node =
                ModuleAbbrevNode(
                    SingleTextNode.``module``,
                    SingleTextNode.Create(name),
                    IdentListNode([ IdentifierOrDot.Ident(SingleTextNode.Create(alias)) ], Range.Zero),
                    Range.Zero
                )

            ModuleDecl.ModuleAbbrev(node))

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
            WidgetBuilder<ModuleDecl>(
                ModuleAbbrev.WidgetKey,
                AttributesBundle(
                    StackList.two(ModuleAbbrev.Name.WithValue(name), ModuleAbbrev.Alias.WithValue(alias)),
                    Array.empty,
                    Array.empty
                )
            )
