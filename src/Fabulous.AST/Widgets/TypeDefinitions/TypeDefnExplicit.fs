namespace Fabulous.AST

open Fabulous.AST
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Syntax
open Fantomas.FCS.Text

module TypeDefnExplicit =
    let Name = Attributes.defineScalar<string> "Name"

    let Constructor = Attributes.defineWidget "Constructor"

    let Kind = Attributes.defineScalar<SingleTextNode> "Kind"

    let Members = Attributes.defineWidgetCollection "Members"

    let WidgetKey =
        Widgets.register "TypeDefnExplicit" (fun widget ->
            let name =
                Widgets.getScalarValue widget Name |> PrettyNaming.NormalizeIdentifierBackticks

            let attributes =
                Widgets.tryGetScalarValue widget TypeDefn.MultipleAttributes
                |> ValueOption.map(fun x -> Some(MultipleAttributeListNode.Create(x)))
                |> ValueOption.defaultValue None

            let xmlDocs =
                Widgets.tryGetNodeFromWidget widget TypeDefn.XmlDocs
                |> ValueOption.map Some
                |> ValueOption.defaultValue None

            let typeParams =
                Widgets.tryGetNodeFromWidget widget TypeDefn.TypeParams
                |> ValueOption.map Some
                |> ValueOption.defaultValue None

            let implicitConstructor =
                Widgets.tryGetNodeFromWidget<ImplicitConstructorNode> widget Constructor

            let implicitConstructor =
                match implicitConstructor with
                | ValueNone -> None
                | ValueSome implicitConstructor -> Some implicitConstructor

            let accessControl =
                Widgets.tryGetScalarValue widget TypeDefn.Accessibility
                |> ValueOption.defaultValue AccessControl.Unknown

            let accessControl =
                match accessControl with
                | Public -> Some(SingleTextNode.``public``)
                | Private -> Some(SingleTextNode.``private``)
                | Internal -> Some(SingleTextNode.``internal``)
                | Unknown -> None

            let kind = Widgets.getScalarValue widget Kind

            let memDefns =
                Widgets.tryGetNodesFromWidgetCollection widget Members
                |> ValueOption.defaultValue []

            TypeDefn.Explicit(
                TypeDefnExplicitNode(
                    TypeNameNode(
                        xmlDocs,
                        attributes,
                        SingleTextNode.``type``,
                        accessControl,
                        IdentListNode([ IdentifierOrDot.Ident(SingleTextNode.Create(name)) ], Range.Zero),
                        typeParams,
                        [],
                        implicitConstructor,
                        Some(SingleTextNode.equals),
                        None,
                        Range.Zero
                    ),
                    TypeDefnExplicitBodyNode(kind, memDefns, SingleTextNode.``end``, Range.Zero),
                    [],
                    Range.Zero
                )
            ))

[<AutoOpen>]
module TypeDefnExplicitBuilders =
    type Ast with
        static member private BaseClassEnd
            (name: string, constructor: WidgetBuilder<ImplicitConstructorNode> voption, kind: SingleTextNode)
            =
            CollectionBuilder<TypeDefn, MemberDefn>(
                TypeDefnExplicit.WidgetKey,
                TypeDefnExplicit.Members,
                AttributesBundle(
                    StackList.two(TypeDefnExplicit.Name.WithValue(name), TypeDefnExplicit.Kind.WithValue(kind)),
                    [| match constructor with
                       | ValueSome constructor -> TypeDefnExplicit.Constructor.WithValue(constructor.Compile())
                       | ValueNone -> () |],
                    Array.empty
                )
            )

        /// <summary>Create a class end with the given name.</summary>
        /// <param name="name">The name of the class.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         ClassEnd("EmptyClass") { }
        ///
        ///         ClassEnd("Class") {
        ///             Member("this.Name", UnitPat(), UnitExpr())
        ///         }
        ///     }
        /// }
        /// </code>
        static member ClassEnd(name: string) =
            Ast.BaseClassEnd(name, ValueNone, SingleTextNode.``class``)

        /// <summary>Create a class end with the given name and constructor.</summary>
        /// <param name="name">The name of the class.</param>
        /// <param name="constructor">The constructor of the class.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///    AnonymousModule() {
        ///         ClassEnd("Class", Constructor(ParameterPat("name", String()))) {
        ///            Member("this.Name", UnitPat(), String("name"))
        ///        }
        ///    }
        /// }
        /// </code>
        static member ClassEnd(name: string, constructor: WidgetBuilder<ImplicitConstructorNode>) =
            Ast.BaseClassEnd(name, ValueSome constructor, SingleTextNode.``class``)

        /// <summary>Create a class end with the given name and constructor.</summary>
        /// <param name="name">The name of the class.</param>
        /// <param name="constructor">The constructor of the class.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         ClassEnd("Class", Constructor(ParameterPat("name", String()))) {
        ///             Member("this.Name", UnitPat(), String("name"))
        ///         }
        ///     }
        /// }
        /// </code>
        static member ClassEnd(name: string, constructor: WidgetBuilder<Pattern>) =
            Ast.BaseClassEnd(name, ValueSome(Ast.Constructor(constructor)), SingleTextNode.``class``)

        /// <summary>Create a struct end with the given name.</summary>
        /// <param name="name">The name of the struct.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         StructEnd("EmptyStruct") { }
        ///     }
        /// }
        /// </code>
        static member StructEnd(name: string) =
            Ast.BaseClassEnd(name, ValueNone, SingleTextNode.``struct``)

        /// <summary>Create a struct end with the given name and constructor.</summary>
        /// <param name="name">The name of the struct.</param>
        /// <param name="constructor">The constructor of the struct.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         StructEnd("Struct", Constructor(ParameterPat("x", Int()))) {
        ///            ValField("x", Int()).toMutable()
        ///            Constructor("x", RecordExpr([ RecordFieldExpr("x", "x") ]))
        ///        }
        ///    }
        /// }
        /// </code>
        static member StructEnd(name: string, constructor: WidgetBuilder<ImplicitConstructorNode>) =
            Ast.BaseClassEnd(name, ValueSome constructor, SingleTextNode.``struct``)

        /// <summary>Create a struct end with the given name and constructor.</summary>
        /// <param name="name">The name of the struct.</param>
        /// <param name="constructor">The constructor of the struct.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         StructEnd("Struct", UnitPat()) {
        ///            ValField("x", Int()).toMutable()
        ///            Constructor("x", RecordExpr([ RecordFieldExpr("x", "x") ]))
        ///        }
        ///    }
        /// }
        /// </code>
        static member StructEnd(name: string, constructor: WidgetBuilder<Pattern>) =
            Ast.BaseClassEnd(name, ValueSome(Ast.Constructor(constructor)), SingleTextNode.``struct``)

        /// <summary>Create an interface end with the given name.</summary>
        /// <param name="name">The name of the interface.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         InterfaceEnd("IMarker") { }
        ///         InterfaceEnd("IMarker") {
        ///             AbstractMember("Name", String())
        ///         }
        ///     }
        /// }
        /// </code>
        static member InterfaceEnd(name: string) =
            Ast.BaseClassEnd(name, ValueNone, SingleTextNode.``interface``)
