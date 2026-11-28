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
                Widgets.tryGetNodeFromWidget widget TypeDefn.XmlDocs |> ValueOption.toOption

            let attributes =
                Widgets.tryGetScalarValue widget TypeDefn.MultipleAttributes
                |> ValueOption.map MultipleAttributeListNode.Create
                |> ValueOption.toOption

            let typeParams =
                Widgets.tryGetNodeFromWidget widget TypeDefn.TypeParams |> ValueOption.toOption

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
                        TypeDefn.accessibility widget,
                        Type.LongIdent(
                            IdentListNode([ IdentifierOrDot.Ident(SingleTextNode.Create(name)) ], Range.range0)
                        ),
                        typeParams,
                        TypeDefn.constraints widget,
                        None,
                        Some(SingleTextNode.equals),
                        Range.range0
                    ),
                    TypeDefn.representationAccessibility widget,
                    unionCaseNode,
                    None,
                    members,
                    Range.range0
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
