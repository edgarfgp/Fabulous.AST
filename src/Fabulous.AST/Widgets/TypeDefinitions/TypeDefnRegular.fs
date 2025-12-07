namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fabulous.AST
open Fabulous.AST.StackAllocatedCollections
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Syntax
open Fantomas.FCS.Text

module TypeDefnRegular =
    let Name = Attributes.defineScalar<string> "Name"
    let ImplicitConstructor = Attributes.defineWidget "SimplePats"
    let Members = Attributes.defineWidgetCollection "Members"

    let WidgetKey =
        Widgets.register "TypeDefnRegular" (fun widget ->
            let name =
                Widgets.getScalarValue widget Name |> PrettyNaming.NormalizeIdentifierBackticks

            let constructor =
                Widgets.tryGetNodeFromWidget<ImplicitConstructorNode> widget ImplicitConstructor

            let members =
                Widgets.tryGetNodesFromWidgetCollection<MemberDefn> widget Members
                |> ValueOption.defaultValue []

            let typeParams =
                Widgets.tryGetNodeFromWidget widget TypeDefn.TypeParams
                |> ValueOption.map Some
                |> ValueOption.defaultValue None

            let xmlDocs =
                Widgets.tryGetNodeFromWidget widget TypeDefn.XmlDocs
                |> ValueOption.map(Some)
                |> ValueOption.defaultValue None

            let attributes =
                Widgets.tryGetScalarValue widget TypeDefn.MultipleAttributes
                |> ValueOption.map(fun x -> Some(MultipleAttributeListNode.Create(x)))
                |> ValueOption.defaultValue None

            let constructor =
                match constructor with
                | ValueNone -> None
                | ValueSome constructor -> Some constructor

            let accessControl =
                Widgets.tryGetScalarValue widget TypeDefn.Accessibility
                |> ValueOption.defaultValue AccessControl.Unknown

            let accessControl =
                match accessControl with
                | Public -> Some(SingleTextNode.``public``)
                | Private -> Some(SingleTextNode.``private``)
                | Internal -> Some(SingleTextNode.``internal``)
                | Unknown -> None

            let leadingKeyword =
                Widgets.tryGetScalarValue widget TypeDefn.IsRecursive
                |> ValueOption.map(fun _ -> SingleTextNode.``and``)
                |> ValueOption.defaultValue SingleTextNode.``type``

            TypeDefn.Regular(
                TypeDefnRegularNode(
                    TypeNameNode(
                        xmlDocs,
                        attributes,
                        leadingKeyword,
                        accessControl,
                        IdentListNode([ IdentifierOrDot.Ident(SingleTextNode.Create(name)) ], Range.Zero),
                        typeParams,
                        [],
                        constructor,
                        Some(SingleTextNode.equals),
                        None,
                        Range.Zero
                    ),
                    members,
                    Range.Zero
                )
            ))

[<AutoOpen>]
module TypeDefnRegularBuilders =
    type Ast with
        static member BaseTypeDefn(name: string, constructor: WidgetBuilder<ImplicitConstructorNode> voption) =
            CollectionBuilder<TypeDefn, MemberDefn>(
                TypeDefnRegular.WidgetKey,
                TypeDefnRegular.Members,
                AttributesBundle(
                    StackList.one(TypeDefnRegular.Name.WithValue(name)),
                    [| match constructor with
                       | ValueSome constructor -> TypeDefnRegular.ImplicitConstructor.WithValue(constructor.Compile())
                       | ValueNone -> () |],
                    Array.empty
                )
            )

        /// <summary>Create a regular type definition with the given name.</summary>
        /// <param name="name">The name of the type definition.</param>
        /// <param name="constructor">The parameters of the type definition.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///    AnonymousModule() {
        ///        TypeDefn("Person", ParenPat()) {
        ///            Member(
        ///                "this.Name",
        ///                ParenPat(ParameterPat(ConstantPat(Constant("p")), String())),
        ///                ConstantExpr(Int 23)
        ///            )
        ///        }
        ///
        ///        TypeDefn("IFoo") {
        ///             AbstractMember("Name", String())
        ///        }
        ///    }
        ///}
        /// </code>
        static member TypeDefn(name: string, constructor: WidgetBuilder<Pattern>) =
            Ast.BaseTypeDefn(name, ValueSome(Ast.Constructor constructor))

        /// <summary>Create a regular type definition with the given name.</summary>
        /// <param name="name">The name of the type definition.</param>
        /// <param name="constructor">The constructor of the type definition.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         TypeDefn(
        ///             "Person",
        ///              Constructor(
        ///                  ParenPat(
        ///                     TuplePat(
        ///                         [ ParameterPat(ConstantPat(Constant("name")), String())
        ///                           ParameterPat(ConstantPat(Constant("age")), Int()) ])
        ///                     )
        ///                 )
        ///             ) {
        ///                   MemberVal("Name", ConstantExpr(Constant("name")), true, true)
        ///                  MemberVal("Age", ConstantExpr(Constant("age")), true, true)
        ///              }
        ///     }
        /// }
        /// </code>
        static member TypeDefn(name: string, constructor: WidgetBuilder<ImplicitConstructorNode>) =
            Ast.BaseTypeDefn(name, ValueSome constructor)

        /// <summary>Create a regular type definition with the given name.</summary>
        /// <param name="name">The name of the type definition.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         TypeDefn("ITriangle") {
        ///            AbstractMember("Area", Float(), true)
        ///         }
        ///    }
        /// }
        /// </code>
        static member TypeDefn(name: string) = Ast.BaseTypeDefn(name, ValueNone)
