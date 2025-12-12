namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fabulous.AST
open Fabulous.AST.StackAllocatedCollections
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Syntax
open Fantomas.FCS.Text

module Enum =

    let EnumCaseNode = Attributes.defineWidgetCollection "UnionCaseNode"

    let Name = Attributes.defineScalar<string> "Name"

    let WidgetKey =
        Widgets.register "Enum" (fun widget ->
            let name =
                Widgets.getScalarValue widget Name |> PrettyNaming.NormalizeIdentifierBackticks

            let enumCaseNodes =
                Widgets.getNodesFromWidgetCollection<EnumCaseNode> widget EnumCaseNode

            let xmlDocs =
                Widgets.tryGetNodeFromWidget widget TypeDefn.XmlDocs
                |> ValueOption.map(Some)
                |> ValueOption.defaultValue None

            let attributes =
                Widgets.tryGetScalarValue widget TypeDefn.MultipleAttributes
                |> ValueOption.map(fun x -> Some(MultipleAttributeListNode.Create(x)))
                |> ValueOption.defaultValue None

            TypeDefn.Enum(
                TypeDefnEnumNode(
                    TypeNameNode(
                        xmlDocs,
                        attributes,
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
                    enumCaseNodes,
                    [],
                    Range.Zero
                )
            ))

[<AutoOpen>]
module EnumBuilders =
    type Ast with

        /// <summary>Create an enum with the given name.</summary>
        /// <param name="name">The name of the enum.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         Enum("Color") {
        ///             EnumCase("Red", Int 0)
        ///             EnumCase("Green", Int 1)
        ///             EnumCase("Blue", Int 2)
        ///         }
        ///     }
        /// }
        /// </code>
        static member Enum(name: string) =
            let name = PrettyNaming.NormalizeIdentifierBackticks name
            CollectionBuilder<TypeDefn, EnumCaseNode>(Enum.WidgetKey, Enum.EnumCaseNode, Enum.Name.WithValue(name))
