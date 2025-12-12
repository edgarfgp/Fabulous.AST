namespace Fabulous.AST

open Fabulous.AST
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module DefaultMember =
    let Name = Attributes.defineScalar<string> "Name"

    let Parameters = Attributes.defineScalar<Pattern seq> "Parameters"

    let WidgetKey =
        Widgets.register "DefaultMember" (fun widget ->
            let name = Widgets.getScalarValue widget Name
            let bodyExpr = Widgets.getNodeFromWidget<Expr> widget BindingNode.BodyExpr
            let parameters = Widgets.getScalarValue widget Parameters |> List.ofSeq

            let accessControl =
                Widgets.tryGetScalarValue widget MemberDefn.Accessibility
                |> ValueOption.defaultValue AccessControl.Unknown

            let accessControl =
                match accessControl with
                | Public -> Some(SingleTextNode.``public``)
                | Private -> Some(SingleTextNode.``private``)
                | Internal -> Some(SingleTextNode.``internal``)
                | Unknown -> None

            let xmlDocs =
                Widgets.tryGetNodeFromWidget widget MemberDefn.XmlDocs
                |> ValueOption.map(fun x -> Some(x))
                |> ValueOption.defaultValue None

            let attributes =
                Widgets.tryGetScalarValue widget MemberDefn.MultipleAttributes
                |> ValueOption.map(fun x -> Some(MultipleAttributeListNode.Create(x)))
                |> ValueOption.defaultValue None

            let isInlined =
                Widgets.tryGetScalarValue widget BindingNode.IsInlined
                |> ValueOption.defaultValue false

            let returnType = Widgets.tryGetNodeFromWidget widget BindingNode.Return

            let returnType =
                match returnType with
                | ValueNone -> None
                | ValueSome value -> Some(BindingReturnInfoNode(SingleTextNode.colon, value, Range.Zero))

            let typeParams =
                Widgets.tryGetNodeFromWidget widget MemberDefn.TypeParams
                |> ValueOption.map Some
                |> ValueOption.defaultValue None

            let node =
                BindingNode(
                    xmlDocs,
                    attributes,
                    MultipleTextsNode([ SingleTextNode.``default`` ], Range.Zero),
                    false,
                    (if isInlined then Some(SingleTextNode.``inline``) else None),
                    accessControl,
                    Choice1Of2(IdentListNode([ IdentifierOrDot.Ident(SingleTextNode.Create(name)) ], Range.Zero)),
                    typeParams,
                    parameters,
                    returnType,
                    SingleTextNode.equals,
                    bodyExpr,
                    Range.Zero
                )

            MemberDefn.Member(node))

[<AutoOpen>]
module DefaultMemberBuilders =
    type Ast with
        /// <summary>
        /// Creates a default member definition.
        /// </summary>
        /// <param name="name">The name of the member.</param>
        /// <param name="parameters">The parameters of the member.</param>
        /// <param name="bodyExpr">The body expression of the member.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///    AnonymousModule() {
        ///        TypeDefn("Person", UnitPat()) {
        ///            AbstractMember("GetValue", [ Unit() ], String())
        ///            Default("this.GetValue", [ UnitPat() ], ConstantExpr(String("Hello, World!")))
        ///        }
        ///    }
        /// }
        /// </code>
        static member Default(name: string, parameters: WidgetBuilder<Pattern> seq, bodyExpr: WidgetBuilder<Expr>) =
            let parameters = parameters |> Seq.map Gen.mkOak |> List.ofSeq

            WidgetBuilder<MemberDefn>(
                DefaultMember.WidgetKey,
                AttributesBundle(
                    StackList.two(DefaultMember.Name.WithValue(name), DefaultMember.Parameters.WithValue(parameters)),
                    [| BindingNode.BodyExpr.WithValue(bodyExpr.Compile()) |],
                    Array.empty
                )
            )

        /// <summary>
        /// Creates a default member definition.
        /// </summary>
        /// <param name="name">The name of the member.</param>
        /// <param name="parameters">The parameters of the member.</param>
        /// <param name="bodyExpr">The body expression of the member.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         TypeDefn("Person", UnitPat()) {
        ///             AbstractMember("GetValue", [ Unit() ], String())
        ///             Default("this.GetValue", [ UnitPat() ], String("Hello, World!"))
        ///         }
        ///     }
        /// }
        /// </code>
        static member Default(name: string, parameters: WidgetBuilder<Pattern> seq, bodyExpr: WidgetBuilder<Constant>) =
            Ast.Default(name, parameters, Ast.ConstantExpr(bodyExpr))

        /// <summary>
        /// Creates a default member definition.
        /// </summary>
        /// <param name="name">The name of the member.</param>
        /// <param name="parameters">The parameters of the member.</param>
        /// <param name="bodyExpr">The body expression of the member.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         TypeDefn("Person", UnitPat()) {
        ///             AbstractMember("GetValue", [ Unit() ], String())
        ///             Default("this.GetValue", [ UnitPat() ], "Hello, World!")
        ///         }
        ///     }
        /// }
        /// </code>
        static member Default(name: string, parameters: WidgetBuilder<Pattern> seq, bodyExpr: string) =
            Ast.Default(name, parameters, Ast.Constant(bodyExpr))

        /// <summary>
        /// Creates a default member definition.
        /// </summary>
        /// <param name="name">The name of the member.</param>
        /// <param name="parameters">The parameters of the member.</param>
        /// <param name="bodyExpr">The body expression of the member.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         TypeDefn("Person", UnitPat()) {
        ///             AbstractMember("GetValue", [ Unit() ], String())
        ///             Default("this.GetValue", UnitPat(), ConstantExpr(String("Hello, World!")))
        ///         }
        ///     }
        /// }
        /// </code>
        static member Default(name: string, parameters: string seq, bodyExpr: WidgetBuilder<Expr>) =
            let parameters =
                parameters
                |> Seq.map(fun p -> Ast.ParameterPat(Ast.ConstantPat(Ast.Constant(p))))

            Ast.Default(name, parameters, bodyExpr)

        /// <summary>
        /// Creates a default member definition.
        /// </summary>
        /// <param name="name">The name of the member.</param>
        /// <param name="parameters">The parameters of the member.</param>
        /// <param name="bodyExpr">The body expression of the member.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         TypeDefn("Person", UnitPat()) {
        ///             AbstractMember("GetValue", [ Unit() ], String())
        ///             Default("this.GetValue", [ "()" ], String("Hello, World!"))
        ///         }
        ///     }
        /// }
        /// </code>
        static member Default(name: string, parameters: string seq, bodyExpr: WidgetBuilder<Constant>) =
            let parameters =
                parameters |> Seq.map(fun p -> Ast.ParameterPat(Ast.ConstantPat(p)))

            Ast.Default(name, parameters, bodyExpr)

        /// <summary>
        /// Creates a default member definition.
        /// </summary>
        /// <param name="name">The name of the member.</param>
        /// <param name="parameters">The parameters of the member.</param>
        /// <param name="bodyExpr">The body expression of the member.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         TypeDefn("Person", UnitPat()) {
        ///             AbstractMember("GetValue", [ Unit() ], String())
        ///             Default("this.GetValue", [ "()" ], "Hello, World!")
        ///         }
        ///     }
        /// }
        /// </code>
        static member Default(name: string, parameters: string seq, bodyExpr: string) =
            let parameters =
                parameters |> Seq.map(fun p -> Ast.ParameterPat(Ast.ConstantPat(p)))

            Ast.Default(name, parameters, bodyExpr)

        /// <summary>
        /// Creates a default member definition.
        /// </summary>
        /// <param name="name">The name of the member.</param>
        /// <param name="parameters">The parameters of the member.</param>
        /// <param name="bodyExpr">The body expression of the member.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         TypeDefn("Person", UnitPat()) {
        ///             AbstractMember("GetValue", [ Unit() ], String())
        ///             Default("this.GetValue", UnitPat(), ConstantExpr(String("Hello, World!")))
        ///         }
        ///     }
        /// }
        /// </code>
        static member Default(name: string, parameters: WidgetBuilder<Pattern>, bodyExpr: WidgetBuilder<Expr>) =
            Ast.Default(name, [ parameters ], bodyExpr)

        /// <summary>
        /// Creates a default member definition.
        /// </summary>
        /// <param name="name">The name of the member.</param>
        /// <param name="parameters">The parameters of the member.</param>
        /// <param name="bodyExpr">The body expression of the member.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         TypeDefn("Person", UnitPat()) {
        ///             AbstractMember("GetValue", [ Unit() ], String())
        ///             Default("this.GetValue", UnitPat(), String("Hello, World!"))
        ///         }
        ///     }
        /// }
        /// </code>
        static member Default(name: string, parameters: WidgetBuilder<Pattern>, bodyExpr: WidgetBuilder<Constant>) =
            Ast.Default(name, [ parameters ], bodyExpr)

        /// <summary>
        /// Creates a default member definition.
        /// </summary>
        /// <param name="name">The name of the member.</param>
        /// <param name="parameters">The parameters of the member.</param>
        /// <param name="bodyExpr">The body expression of the member.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         TypeDefn("Person", UnitPat()) {
        ///             AbstractMember("GetValue", [ Unit() ], String())
        ///             Default("this.GetValue", UnitPat(), "Hello, World!")
        ///         }
        ///     }
        /// }
        /// </code>
        static member Default(name: string, parameters: WidgetBuilder<Pattern>, bodyExpr: string) =
            Ast.Default(name, [ parameters ], bodyExpr)

        /// <summary>
        /// Creates a default member definition.
        /// </summary>
        /// <param name="name">The name of the member.</param>
        /// <param name="parameters">The parameters of the member.</param>
        /// <param name="bodyExpr">The body expression of the member.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         TypeDefn("Person", UnitPat()) {
        ///             AbstractMember("GetValue", [ Unit() ], String())
        ///             Default("this.GetValue", "()", ConstantExpr(String("Hello, World!")))
        ///         }
        ///     }
        /// }
        /// </code>
        static member Default(name: string, parameters: string, bodyExpr: WidgetBuilder<Expr>) =
            Ast.Default(name, [ parameters ], bodyExpr)

        /// <summary>
        /// Creates a default member definition.
        /// </summary>
        /// <param name="name">The name of the member.</param>
        /// <param name="parameters">The parameters of the member.</param>
        /// <param name="bodyExpr">The body expression of the member.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         TypeDefn("Person", UnitPat()) {
        ///             AbstractMember("GetValue", [ Unit() ], String())
        ///             Default("this.GetValue", "()", String("Hello, World!"))
        ///         }
        ///     }
        /// }
        /// </code>
        static member Default(name: string, parameters: string, bodyExpr: WidgetBuilder<Constant>) =
            Ast.Default(name, [ parameters ], bodyExpr)

        /// <summary>
        /// Creates a default member definition.
        /// </summary>
        /// <param name="name">The name of the member.</param>
        /// <param name="parameters">The parameters of the member.</param>
        /// <param name="bodyExpr">The body expression of the member.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         TypeDefn("Person", UnitPat()) {
        ///             AbstractMember("GetValue", [ Unit() ], String())
        ///             Default("this.GetValue", "()", "Hello, World!")
        ///         }
        ///     }
        /// }
        /// </code>
        static member Default(name: string, parameters: string, bodyExpr: string) =
            Ast.Default(name, [ parameters ], bodyExpr)
