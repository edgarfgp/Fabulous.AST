namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fabulous.AST
open Fabulous.AST.StackAllocatedCollections
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Syntax
open Fantomas.FCS.Text

module Union =

    let UnionCaseNode = Attributes.defineWidgetCollection "UnionCaseNode"

    let Name = Attributes.defineScalar<string> "Name"

    let WidgetKey =
        Widgets.register "Union" (fun widget ->
            let name =
                Widgets.getScalarValue widget Name |> PrettyNaming.NormalizeIdentifierBackticks

            let unionCaseNode =
                Widgets.getNodesFromWidgetCollection<UnionCaseNode> widget UnionCaseNode

            let members =
                Widgets.tryGetNodesFromWidgetCollection<MemberDefn> widget TypeDefn.Members
                |> ValueOption.defaultValue []

            let xmlDocs =
                Widgets.tryGetNodeFromWidget widget TypeDefn.XmlDocs
                |> ValueOption.map(Some)
                |> ValueOption.defaultValue None

            let attributes =
                Widgets.tryGetScalarValue widget TypeDefn.MultipleAttributes
                |> ValueOption.map(fun x -> Some(MultipleAttributeListNode.Create(x)))
                |> ValueOption.defaultValue None

            let typeParams =
                Widgets.tryGetNodeFromWidget widget TypeDefn.TypeParams
                |> ValueOption.map Some
                |> ValueOption.defaultValue None

            let accessControl =
                Widgets.tryGetScalarValue widget TypeDefn.Accessibility
                |> ValueOption.defaultValue AccessControl.Unknown

            let accessControl =
                match accessControl with
                | Public -> Some(SingleTextNode.``public``)
                | Private -> Some(SingleTextNode.``private``)
                | Internal -> Some(SingleTextNode.``internal``)
                | Unknown -> None

            let isRecursive =
                Widgets.tryGetScalarValue widget TypeDefn.IsRecursive
                |> ValueOption.defaultValue false

            let leadingKeyword =
                if isRecursive then
                    SingleTextNode.``and``
                else
                    SingleTextNode.``type``

            TypeDefn.Union(
                TypeDefnUnionNode(
                    TypeNameNode(
                        xmlDocs,
                        attributes,
                        leadingKeyword,
                        None,
                        IdentListNode([ IdentifierOrDot.Ident(SingleTextNode.Create(name)) ], Range.Zero),
                        typeParams,
                        [],
                        None,
                        Some(SingleTextNode.equals),
                        None,
                        Range.Zero
                    ),
                    accessControl,
                    unionCaseNode,
                    members,
                    Range.Zero
                )
            ))

[<AutoOpen>]
module UnionBuilders =
    type Ast with
        /// <summary>Creates a new union type with the specified name.</summary>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         Union("Shape") {
        ///             UnionCase("Circle")
        ///             UnionCase("Rectangle")
        ///         }
        ///     }
        /// }
        /// </code>
        static member Union(name: string) =
            CollectionBuilder<TypeDefn, UnionCaseNode>(
                Union.WidgetKey,
                Union.UnionCaseNode,
                Union.Name.WithValue(PrettyNaming.NormalizeIdentifierBackticks name)
            )
