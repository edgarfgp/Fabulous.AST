namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fabulous.AST
open Fabulous.AST.StackAllocatedCollections
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Syntax
open Fantomas.FCS.Text

module TypeNameNode =

    let Name = Attributes.defineScalar<string> "Name"

    let PowerType = Attributes.defineWidget "PowerType"

    let MeasureAttribute = Attributes.defineScalar<AttributeNode seq> "MeasureAttribute"

    let WidgetKey =
        Widgets.register "Measure" (fun widget ->
            let name = Widgets.getScalarValue widget Name

            let xmlDocs =
                Widgets.tryGetNodeFromWidget widget TypeDefn.XmlDocs
                |> ValueOption.map(Some)
                |> ValueOption.defaultValue None

            let measureAttribute = Widgets.getScalarValue widget MeasureAttribute |> List.ofSeq

            let multipleAttributes =
                Widgets.tryGetScalarValue widget TypeDefn.MultipleAttributes
                |> ValueOption.map Some
                |> ValueOption.defaultValue None

            let attributes =
                match multipleAttributes with
                | Some(multipleAttributes) -> measureAttribute @ List.ofSeq multipleAttributes
                | None -> measureAttribute

            TypeDefn.None(
                TypeNameNode(
                    xmlDocs,
                    Some(MultipleAttributeListNode.Create(attributes)),
                    SingleTextNode.``type``,
                    Some(SingleTextNode.Create(name)),
                    IdentListNode([], Range.Zero),
                    None,
                    [],
                    None,
                    None,
                    None,
                    Range.Zero
                )
            ))

[<AutoOpen>]
module TypeNameNodeBuilders =
    type Ast with
        /// <summary>Create a measure type with the given name.</summary>
        /// <param name="name">The name of the measure type.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         Measure("cm")
        ///     }
        /// }
        /// </code>
        static member Measure(name: string) =
            let name = PrettyNaming.NormalizeIdentifierBackticks name

            WidgetBuilder<TypeDefn>(
                TypeNameNode.WidgetKey,
                TypeNameNode.Name.WithValue(name),
                TypeNameNode.MeasureAttribute.WithValue([ Gen.mkOak(Ast.Attribute("Measure")) ])
            )

        /// <summary>Create a measure type with the given name and power type.</summary>
        /// <param name="name">The name of the measure type.</param>
        /// <param name="powerType">The power type of the measure type.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         Measure("ml", MeasurePower(LongIdent "cm", Integer "3"))
        ///     }
        /// }
        /// </code>
        static member Measure(name: string, powerType: WidgetBuilder<Type>) =
            let name = PrettyNaming.NormalizeIdentifierBackticks name
            Ast.Abbrev(name, powerType).attribute(Ast.Attribute("Measure"))

        /// <summary>Create a measure type with the given name and power type.</summary>
        /// <param name="name">The name of the measure type.</param>
        /// <param name="powerType">The power type of the measure type.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         Measure("ml", "cm^3")
        ///     }
        /// }
        /// </code>
        static member Measure(name: string, powerType: string) =
            Ast.Measure(name, Ast.LongIdent(powerType))
