namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fabulous.AST
open Fabulous.AST.StackAllocatedCollections
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Syntax
open Fantomas.FCS.Text

module Augmentation =
    let Name = Attributes.defineScalar<string> "Name"
    let Members = Attributes.defineWidgetCollection "Members"

    let WidgetKey =
        Widgets.register "Augmentation" (fun widget ->
            let name =
                Widgets.getScalarValue widget Name |> PrettyNaming.NormalizeIdentifierBackticks

            let members =
                Widgets.tryGetNodesFromWidgetCollection<MemberDefn> widget Members
                |> ValueOption.defaultValue []

            let attributes =
                Widgets.tryGetScalarValue widget TypeDefn.MultipleAttributes
                |> ValueOption.map(fun x -> Some(MultipleAttributeListNode.Create(x)))
                |> ValueOption.defaultValue None

            let typeParams =
                Widgets.tryGetNodeFromWidget widget TypeDefn.TypeParams
                |> ValueOption.map Some
                |> ValueOption.defaultValue None

            let xmlDocs =
                Widgets.tryGetNodeFromWidget widget TypeDefn.XmlDocs
                |> ValueOption.map(fun x -> Some(x))
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

            TypeDefn.Augmentation(
                TypeDefnAugmentationNode(
                    TypeNameNode(
                        xmlDocs,
                        attributes,
                        SingleTextNode.``type``,
                        accessControl,
                        IdentListNode([ IdentifierOrDot.Ident(SingleTextNode.Create(name)) ], Range.Zero),
                        typeParams,
                        [],
                        None,
                        None,
                        Some SingleTextNode.``with``,
                        Range.Zero
                    ),
                    members,
                    Range.Zero
                )
            ))

[<AutoOpen>]
module AugmentBuilders =
    type Ast with
        /// <summary>Create a type augmentation with the given name.</summary>
        /// <param name="name">The name of the type augmentation.</param>
        ///<code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         Augmentation("IEnumerable") {
        ///            Member(
        ///                "xs.RepeatElements",
        ///                ParenPat(ParameterPat("n", Int())),
        ///                SeqExpr(ForEachDoExpr("x", "xs", ForEachArrowExpr("_", "1 .. n", "x")))
        ///            )
        ///        }
        /// }
        /// </code>
        static member Augmentation(name: string) =
            CollectionBuilder<TypeDefn, MemberDefn>(
                Augmentation.WidgetKey,
                Augmentation.Members,
                Augmentation.Name.WithValue(name)
            )
