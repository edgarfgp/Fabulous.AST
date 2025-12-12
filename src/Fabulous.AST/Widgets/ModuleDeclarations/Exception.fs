namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fabulous.AST
open Fabulous.AST.StackAllocatedCollections
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module ExceptionDefn =
    let UnionCase = Attributes.defineWidget "UnionCase"

    let Members = Attributes.defineWidgetCollection "MemberDefs"

    let WidgetKey =
        Widgets.register "ExceptionDefn" (fun widget ->

            let xmlDocs =
                Widgets.tryGetNodeFromWidget widget ModuleDecl.XmlDocs
                |> ValueOption.map(Some)
                |> ValueOption.defaultValue None

            let attributes =
                Widgets.tryGetScalarValue widget ModuleDecl.MultipleAttributes
                |> ValueOption.map(fun x -> Some(MultipleAttributeListNode.Create(x)))
                |> ValueOption.defaultValue None

            let accessControl =
                Widgets.tryGetScalarValue widget ModuleDecl.Accessibility
                |> ValueOption.defaultValue AccessControl.Unknown

            let accessControl =
                match accessControl with
                | Public -> Some(SingleTextNode.``public``)
                | Private -> Some(SingleTextNode.``private``)
                | Internal -> Some(SingleTextNode.``internal``)
                | Unknown -> None

            let unionCase = Widgets.getNodeFromWidget<UnionCaseNode> widget UnionCase

            let memberDefs =
                Widgets.tryGetNodesFromWidgetCollection widget Members
                |> ValueOption.defaultValue []

            let withKeyword =
                if not memberDefs.IsEmpty then
                    Some(SingleTextNode.``with``)
                else
                    None

            let node =
                ExceptionDefnNode(xmlDocs, attributes, accessControl, unionCase, withKeyword, memberDefs, Range.Zero)

            ModuleDecl.Exception(node))

[<AutoOpen>]
module ExceptionDefnBuilders =
    type Ast with
        /// <summary>Create an exception definition with a union case.</summary>
        /// <param name="value">The union case.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         ExceptionDefn(UnionCase("Error", [ Field(String()); Field(Int()) ]))
        ///     }
        /// }
        /// </code>
        static member ExceptionDefn(value: WidgetBuilder<UnionCaseNode>) =
            WidgetBuilder<ModuleDecl>(ExceptionDefn.WidgetKey, ExceptionDefn.UnionCase.WithValue(value.Compile()))

        /// <summary>Create an exception definition with a union case.</summary>
        /// <param name="value">The union case.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         ExceptionDefn("Error")
        ///     }
        /// }
        /// </code>
        static member ExceptionDefn(value: string) = Ast.ExceptionDefn(Ast.UnionCase(value))

        /// <summary>Create an exception definition with a union case.</summary>
        /// <param name="value">The union case.</param>
        /// <param name="parameters">The parameters of the union case.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         ExceptionDefn("Error", [ Field(String()); Field(Int()) ])
        ///     }
        /// }
        /// </code>
        static member ExceptionDefn(value: string, parameters: WidgetBuilder<FieldNode> seq) =
            Ast.ExceptionDefn(Ast.UnionCase(value, List.ofSeq parameters))

        /// <summary>Create an exception definition with a union case.</summary>
        /// <param name="value">The union case.</param>
        /// <param name="parameter">The parameters of the union case.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         ExceptionDefn("Error", Field(String()))
        ///     }
        /// }
        /// </code>
        static member ExceptionDefn(value: string, parameter: WidgetBuilder<FieldNode>) =
            Ast.ExceptionDefn(value, [ parameter ])

        /// <summary>Create an exception definition with a union case.</summary>
        /// <param name="value">The union case.</param>
        /// <param name="parameters">The parameters of the union case.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         ExceptionDefn("Error", [ (String()); (Int()) ])
        ///     }
        /// }
        /// </code>
        static member ExceptionDefn(value: string, parameters: WidgetBuilder<Type> seq) =
            let parameters = parameters |> Seq.map Ast.Field
            Ast.ExceptionDefn(Ast.UnionCase(value, List.ofSeq parameters))

        /// <summary>Create an exception definition with a union case.</summary>
        /// <param name="value">The union case.</param>
        /// <param name="parameter">The parameter of the union case.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         ExceptionDefn("Error", String())
        ///     }
        /// }
        /// </code>
        static member ExceptionDefn(value: string, parameter: WidgetBuilder<Type>) =
            Ast.ExceptionDefn(value, [ parameter ])

        /// <summary>Create an exception definition with a union case.</summary>
        /// <param name="value">The union case.</param>
        /// <param name="parameters">The parameters of the union case.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         ExceptionDefn("Error", [ "string"; "int" ])
        ///     }
        /// }
        /// </code>
        static member ExceptionDefn(value: string, parameters: string seq) =
            let parameters = parameters |> Seq.map Ast.Field
            Ast.ExceptionDefn(Ast.UnionCase(value, List.ofSeq parameters))

        /// <summary>Create an exception definition with a union case.</summary>
        /// <param name="value">The union case.</param>
        /// <param name="parameter">The parameter of the union case.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         ExceptionDefn("Error", "string")
        ///     }
        /// }
        /// </code>
        static member ExceptionDefn(value: string, parameter: string) = Ast.ExceptionDefn(value, [ parameter ])

        /// <summary>Create an exception definition with a union case.</summary>
        /// <param name="value">The union case.</param>
        /// <param name="parameters">The parameters of the union case.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         ExceptionDefn("Error", [ ("a", String()); ("b", Int()) ])
        ///     }
        /// }
        /// </code>
        static member ExceptionDefn(value: string, parameters: (string * WidgetBuilder<Type>) seq) =
            Ast.ExceptionDefn(Ast.UnionCase(value, List.ofSeq parameters))

        /// <summary>Create an exception definition with a union case.</summary>
        /// <param name="value">The union case.</param>
        /// <param name="parameters">The parameters of the union case.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         ExceptionDefn("Error", [ ("a", "string"); ("b", "int") ])
        ///     }
        /// }
        /// </code>
        static member ExceptionDefn(value: string, parameters: (string * string) seq) =
            Ast.ExceptionDefn(Ast.UnionCase(value, List.ofSeq parameters))

type ExceptionDefnModifiers =
    /// <summary>Sets the members for the current exception definition.</summary>
    /// <param name="this">Current widget.</param>
    /// <code lang="fsharp">
    /// Oak() {
    ///     AnonymousModule() {
    ///         ExceptionDefn("Error", String())
    ///             .members() {
    ///                 Member("Message", String("")).toStatic()
    ///             }
    ///    }
    /// }
    /// </code>
    [<Extension>]
    static member inline members(this: WidgetBuilder<ModuleDecl>) =
        AttributeCollectionBuilder<ModuleDecl, MemberDefn>(this, ExceptionDefn.Members)
