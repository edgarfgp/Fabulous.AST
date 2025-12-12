namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fabulous.AST
open Fabulous.AST.StackAllocatedCollections
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Syntax
open Fantomas.FCS.Text

module TypeDefnAbbrevNode =

    let Name = Attributes.defineScalar<string> "Name"

    let AliasType = Attributes.defineWidget "AliasType"

    let WidgetKey =
        Widgets.register "Abbrev" (fun widget ->
            let name = Widgets.getScalarValue widget Name

            let xmlDocs =
                Widgets.tryGetNodeFromWidget widget TypeDefn.XmlDocs
                |> ValueOption.map(Some)
                |> ValueOption.defaultValue None

            let aliasType = Widgets.getNodeFromWidget widget AliasType

            let typeParams =
                Widgets.tryGetNodeFromWidget widget TypeDefn.TypeParams
                |> ValueOption.map Some
                |> ValueOption.defaultValue None

            let attributes =
                Widgets.tryGetScalarValue widget TypeDefn.MultipleAttributes
                |> ValueOption.map(fun x -> Some(MultipleAttributeListNode.Create(x)))
                |> ValueOption.defaultValue None

            TypeDefn.Abbrev(
                TypeDefnAbbrevNode(
                    TypeNameNode(
                        xmlDocs,
                        attributes,
                        SingleTextNode.``type``,
                        None,
                        IdentListNode([ IdentifierOrDot.Ident(SingleTextNode.Create(name)) ], Range.Zero),
                        typeParams,
                        [],
                        None,
                        Some(SingleTextNode.equals),
                        None,
                        Range.Zero
                    ),
                    aliasType,
                    [],
                    Range.Zero
                )
            ))

[<AutoOpen>]
module TypeDefnAbbrevNodeBuilders =
    type Ast with

        /// <summary>Create a type abbreviation with the given name and alias type.</summary>
        /// <param name="name">The name of the type abbreviation.</param>
        /// <param name="alias">The alias type of the type abbreviation.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         Abbrev("SizeType", "uint32")
        ///    }
        /// }
        /// </code>
        static member Abbrev(name: string, alias: WidgetBuilder<Type>) =
            let name = PrettyNaming.NormalizeIdentifierBackticks name

            WidgetBuilder<TypeDefn>(
                TypeDefnAbbrevNode.WidgetKey,
                AttributesBundle(
                    StackList.one(TypeDefnAbbrevNode.Name.WithValue(name)),
                    [| TypeDefnAbbrevNode.AliasType.WithValue(alias.Compile()) |],
                    Array.empty
                )
            )

        /// <summary>Create a type abbreviation with the given name and alias type.</summary>
        /// <param name="name">The name of the type abbreviation.</param>
        /// <param name="alias">The alias type of the type abbreviation.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         Abbrev("SizeType", "uint32")
        ///     }
        /// }
        /// </code>
        static member Abbrev(name: string, alias: string) = Ast.Abbrev(name, Ast.LongIdent(alias))
