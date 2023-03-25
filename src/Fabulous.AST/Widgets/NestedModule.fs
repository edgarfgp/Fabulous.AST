namespace Fabulous.AST

open System.Runtime.CompilerServices
open FSharp.Compiler.Text
open Fantomas.Core.SyntaxOak
open Fantomas.Core

open type Fabulous.AST.Ast
open type Fantomas.Core.SyntaxOak.Oak

module NestedModule =
    let Name = Attributes.defineScalar<string> "Name"
    let IsRecursive = Attributes.defineScalar<bool> "IsRecursive"
    let Accessibility = Attributes.defineScalar<AccessControl> "Accessibility"

    let ModuleDecls = Attributes.defineWidgetCollection "ModuleDecls"

    let WidgetKey =
        Widgets.register "NestedModule" (fun widget ->
            let name = Helpers.getScalarValue widget Name

            let moduleDecls =
                Helpers.getNodesFromWidgetCollection<ModuleDecl> widget ModuleDecls

            let isRecursive =
                Helpers.tryGetScalarValue widget IsRecursive |> ValueOption.defaultValue false

            let accessControl =
                Helpers.tryGetScalarValue widget Accessibility
                |> ValueOption.defaultValue AccessControl.Public

            let accessControl =
                match accessControl with
                | Public -> None
                | Private -> Some(SingleTextNode("private", Range.Zero))
                | Internal -> Some(SingleTextNode("internal", Range.Zero))


            let nestedModuleNode =
                NestedModuleNode(
                    None,
                    None,
                    SingleTextNode("module", Range.Zero),
                    accessControl,
                    isRecursive,
                    IdentListNode([ IdentifierOrDot.Ident(SingleTextNode(name, Range.Zero)) ], Range.Zero),
                    SingleTextNode("=", Range.Zero),
                    moduleDecls,
                    Range.Zero
                )

            SyntaxOak.Oak(
                List.Empty,
                [ ModuleOrNamespaceNode(None, [ ModuleDecl.NestedModule(nestedModuleNode) ], Range.Zero) ],
                Range.Zero
            ))


[<AutoOpen>]
module NestedModuleBuilders =
    type Fabulous.AST.Ast with

        static member inline NestedModule(name: string) =
            CollectionBuilder<Oak, ModuleDecl>(
                NestedModule.WidgetKey,
                NestedModule.ModuleDecls,
                NestedModule.Name.WithValue(name)
            )

[<Extension>]
type NestedModuleModifiers =
    [<Extension>]
    static member inline isRecursive(this: WidgetBuilder<Oak>) =
        this.AddScalar(NestedModule.IsRecursive.WithValue(true))

    [<Extension>]
    static member inline accessibility(this: WidgetBuilder<Oak>, ?value: AccessControl) =
        match value with
        | Some value -> this.AddScalar(NestedModule.Accessibility.WithValue(value))
        | None -> this.AddScalar(NestedModule.Accessibility.WithValue(AccessControl.Public))
