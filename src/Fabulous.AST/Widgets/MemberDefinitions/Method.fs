namespace Fabulous.AST

open Fabulous.AST
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module BindingMethodNode =
    let Name = Attributes.defineScalar<string> "Name"

    let Parameters = Attributes.defineScalar<Pattern list> "Parameters"

    let WidgetKey =
        Widgets.register "MethodMember" (fun widget ->
            let name = Widgets.getScalarValue widget Name
            let parameters = Widgets.getScalarValue widget Parameters
            let bodyExpr = Widgets.getNodeFromWidget widget BindingNode.BodyExpr
            let isInlined = Widgets.tryGetScalarValue widget BindingNode.IsInlined

            let isStatic =
                Widgets.tryGetScalarValue widget BindingNode.IsStatic
                |> ValueOption.defaultValue false

            let returnType =
                Widgets.tryGetNodeFromWidget widget BindingNode.Return
                |> ValueOption.map(fun value -> Some(BindingReturnInfoNode(SingleTextNode.colon, value, Range.Zero)))
                |> ValueOption.defaultValue None

            let accessControl =
                Widgets.tryGetScalarValue widget BindingNode.Accessibility
                |> ValueOption.defaultValue AccessControl.Unknown

            let accessControl =
                match accessControl with
                | Public -> Some(SingleTextNode.``public``)
                | Private -> Some(SingleTextNode.``private``)
                | Internal -> Some(SingleTextNode.``internal``)
                | Unknown -> None

            let xmlDocs =
                Widgets.tryGetNodeFromWidget widget BindingNode.XmlDocs
                |> ValueOption.map(Some)
                |> ValueOption.defaultValue None

            let attributes =
                Widgets.tryGetScalarValue widget BindingNode.MultipleAttributes
                |> ValueOption.map(fun x -> Some(MultipleAttributeListNode.Create(x)))
                |> ValueOption.defaultValue None

            let inlineNode =
                match isInlined with
                | ValueSome true -> Some(SingleTextNode.``inline``)
                | ValueSome false -> None
                | ValueNone -> None

            let typeParams =
                Widgets.tryGetNodeFromWidget widget BindingNode.TypeParams
                |> ValueOption.map Some
                |> ValueOption.defaultValue None

            let multipleTextsNode =
                [ if isStatic then
                      SingleTextNode.``static``
                      SingleTextNode.``member``
                  else
                      SingleTextNode.``member`` ]

            BindingNode(
                xmlDocs,
                attributes,
                MultipleTextsNode(multipleTextsNode, Range.Zero),
                false,
                inlineNode,
                accessControl,
                Choice1Of2(IdentListNode([ IdentifierOrDot.Ident(SingleTextNode.Create(name)) ], Range.Zero)),
                typeParams,
                parameters,
                returnType,
                SingleTextNode.equals,
                bodyExpr,
                Range.Zero
            ))

[<AutoOpen>]
module BindingMethodBuilders =
    type Ast with
        static member private BaseMember
            (
                name: string,
                parameters: WidgetBuilder<Pattern> list,
                body: WidgetBuilder<Expr>,
                ?returnType: WidgetBuilder<Type>
            ) =
            let parameters = parameters |> List.map Gen.mkOak

            WidgetBuilder<BindingNode>(
                BindingMethodNode.WidgetKey,
                AttributesBundle(
                    StackList.two(
                        BindingMethodNode.Name.WithValue(name),
                        BindingMethodNode.Parameters.WithValue(parameters)
                    ),
                    [| BindingNode.BodyExpr.WithValue(body.Compile())
                       match returnType with
                       | Some returnType -> BindingNode.Return.WithValue(returnType.Compile())
                       | None -> () |],
                    Array.empty
                )
            )

        /// <summary>
        /// Create a method member definition.
        /// </summary>
        /// <param name="name">The name of the method.</param>
        /// <param name="parameters">The parameters of the method.</param>
        /// <param name="body">The body of the method.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         TypeDefn("Person", UnitPat()) {
        ///             Member(
        ///                "this.Name",
        ///                [ ParenPat(ParameterPat("name", String()))
        ///                  ParenPat(ParameterPat("age", Int())) ],
        ///                ConstantExpr(Int 23)
        ///            )
        ///         }
        ///     }
        /// }
        /// </code>
        static member Member(name: string, parameters: WidgetBuilder<Pattern> list, body: WidgetBuilder<Expr>) =
            Ast.BaseMember(name, parameters, body)

        static member Member
            (
                name: string,
                parameters: WidgetBuilder<Pattern> list,
                body: WidgetBuilder<Expr>,
                returnType: WidgetBuilder<Type>
            ) =
            Ast.BaseMember(name, parameters, body, returnType)

        static member Member
            (name: string, parameters: WidgetBuilder<Pattern> list, body: WidgetBuilder<Expr>, returnType: string)
            =
            Ast.BaseMember(name, parameters, body, Ast.LongIdent(returnType))

        /// <summary>
        /// Create a method member definition.
        /// </summary>
        /// <param name="name">The name of the method.</param>
        /// <param name="parameter">The parameter of the method.</param>
        /// <param name="body">The body of the method.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         TypeDefn("Person", UnitPat()) {
        ///             Member(
        ///                 "this.Name",
        ///                 ParenPat(ParameterPat("name", String())),
        ///                 ConstantExpr(Int 23)
        ///             )
        ///         }
        ///     }
        /// }
        /// </code>
        static member Member(name: string, parameter: WidgetBuilder<Pattern>, body: WidgetBuilder<Expr>) =
            Ast.BaseMember(name, [ parameter ], body)

        static member Member
            (name: string, parameter: WidgetBuilder<Pattern>, body: WidgetBuilder<Expr>, returnType: WidgetBuilder<Type>) =
            Ast.BaseMember(name, [ parameter ], body, returnType)

        static member Member
            (name: string, parameter: WidgetBuilder<Pattern>, body: WidgetBuilder<Expr>, returnType: string)
            =
            Ast.BaseMember(name, [ parameter ], body, Ast.LongIdent(returnType))

        /// <summary>
        /// Create a method member definition.
        /// </summary>
        /// <param name="name">The name of the method.</param>
        /// <param name="parameters">The parameters of the method.</param>
        /// <param name="bodyExpr">The body of the method.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         TypeDefn("Person", UnitPat()) {
        ///             Member(
        ///                 "this.Name",
        ///                 [ ParenPat(ParameterPat("name", String()))
        ///                   ParenPat(ParameterPat("age", Int())) ],
        ///                 Int(23)
        ///             )
        ///         }
        ///     }
        /// }
        /// </code>
        static member Member(name: string, parameters: WidgetBuilder<Pattern> list, bodyExpr: WidgetBuilder<Constant>) =
            Ast.BaseMember(name, parameters, Ast.ConstantExpr(bodyExpr))

        static member Member
            (
                name: string,
                parameters: WidgetBuilder<Pattern> list,
                bodyExpr: WidgetBuilder<Constant>,
                returnType: WidgetBuilder<Type>
            ) =
            Ast.BaseMember(name, parameters, Ast.ConstantExpr(bodyExpr), returnType)

        static member Member
            (
                name: string,
                parameters: WidgetBuilder<Pattern> list,
                bodyExpr: WidgetBuilder<Constant>,
                returnType: string
            ) =
            Ast.BaseMember(name, parameters, Ast.ConstantExpr(bodyExpr), Ast.LongIdent(returnType))

        /// <summary>
        /// Create a method member definition.
        /// </summary>
        /// <param name="name">The name of the method.</param>
        /// <param name="parameters">The parameters of the method.</param>
        /// <param name="bodyExpr">The body of the method.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         TypeDefn("Person", UnitPat()) {
        ///             Member(
        ///                 "this.Name",
        ///                 [ ParenPat(ParameterPat("name", String()))
        ///                   ParenPat(ParameterPat("age", Int())) ],
        ///                 "23"
        ///             )
        ///     }
        /// }
        /// </code>
        static member Member(name: string, parameters: WidgetBuilder<Pattern> list, bodyExpr: string) =
            Ast.BaseMember(name, parameters, Ast.ConstantExpr(bodyExpr))

        static member Member
            (name: string, parameters: WidgetBuilder<Pattern> list, bodyExpr: string, returnType: WidgetBuilder<Type>)
            =
            Ast.BaseMember(name, parameters, Ast.ConstantExpr(bodyExpr), returnType)

        static member Member
            (name: string, parameters: WidgetBuilder<Pattern> list, bodyExpr: string, returnType: string)
            =
            Ast.BaseMember(name, parameters, Ast.ConstantExpr(bodyExpr), Ast.LongIdent(returnType))

        /// <summary>
        /// Create a method member definition.
        /// </summary>
        /// <param name="name">The name of the method.</param>
        /// <param name="parameters">The parameters of the method.</param>
        /// <param name="bodyExpr">The body of the method.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         TypeDefn("Person", UnitPat()) {
        ///             Member(
        ///                 "this.Name",
        ///                 [ "(name: string)"; "(age: int)" ],
        ///                 Int(23)
        ///             )
        ///         }
        ///     }
        /// }
        /// </code>
        static member Member(name: string, parameters: string list, bodyExpr: WidgetBuilder<Constant>) =
            let parameters =
                parameters |> List.map(fun p -> Ast.ParameterPat(Ast.ConstantPat(p)))

            Ast.BaseMember(name, parameters, Ast.ConstantExpr(bodyExpr))

        static member Member
            (name: string, parameters: string list, bodyExpr: WidgetBuilder<Constant>, returnType: WidgetBuilder<Type>)
            =
            let parameters =
                parameters |> List.map(fun p -> Ast.ParameterPat(Ast.ConstantPat(p)))

            Ast.BaseMember(name, parameters, Ast.ConstantExpr(bodyExpr), returnType)

        static member Member
            (name: string, parameters: string list, bodyExpr: WidgetBuilder<Constant>, returnType: string)
            =
            let parameters =
                parameters |> List.map(fun p -> Ast.ParameterPat(Ast.ConstantPat(p)))

            Ast.BaseMember(name, parameters, Ast.ConstantExpr(bodyExpr), Ast.LongIdent(returnType))

        /// <summary>
        /// Create a method member definition.
        /// </summary>
        /// <param name="name">The name of the method.</param>
        /// <param name="parameters">The parameters of the method.</param>
        /// <param name="bodyExpr">The body of the method.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         TypeDefn("Person", UnitPat()) {
        ///             Member(
        ///                 "this.Name",
        ///                 [ "(name: string)"; "(age: int)" ],
        ///                 ConstantExpr(Int 23)
        ///             )
        ///         }
        ///     }
        /// }
        /// </code>
        static member Member(name: string, parameters: string list, bodyExpr: WidgetBuilder<Expr>) =
            let parameters =
                parameters |> List.map(fun p -> Ast.ParameterPat(Ast.ConstantPat(p)))

            Ast.BaseMember(name, parameters, bodyExpr)

        static member Member
            (name: string, parameters: string list, bodyExpr: WidgetBuilder<Expr>, returnType: WidgetBuilder<Type>)
            =
            let parameters =
                parameters |> List.map(fun p -> Ast.ParameterPat(Ast.ConstantPat(p)))

            Ast.BaseMember(name, parameters, bodyExpr, returnType)

        static member Member(name: string, parameters: string list, bodyExpr: WidgetBuilder<Expr>, returnType: string) =
            let parameters =
                parameters |> List.map(fun p -> Ast.ParameterPat(Ast.ConstantPat(p)))

            Ast.BaseMember(name, parameters, bodyExpr, Ast.LongIdent(returnType))

        /// <summary>
        /// Create a method member definition.
        /// </summary>
        /// <param name="name">The name of the method.</param>
        /// <param name="parameters">The parameters of the method.</param>
        /// <param name="bodyExpr">The body of the method.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         TypeDefn("Person", UnitPat()) {
        ///             Member(
        ///                 "this.Name",
        ///                 [ "(name: string)"; "(age: int)" ],
        ///                 "23"
        ///             )
        ///         }
        ///     }
        /// }
        /// </code>
        static member Member(name: string, parameters: string list, bodyExpr: string) =
            let parameters =
                parameters |> List.map(fun p -> Ast.ParameterPat(Ast.ConstantPat(p)))

            Ast.BaseMember(name, parameters, Ast.ConstantExpr(bodyExpr))

        static member Member(name: string, parameters: string list, bodyExpr: string, returnType: WidgetBuilder<Type>) =
            let parameters =
                parameters |> List.map(fun p -> Ast.ParameterPat(Ast.ConstantPat(p)))

            Ast.BaseMember(name, parameters, Ast.ConstantExpr(bodyExpr), returnType)

        static member Member(name: string, parameters: string list, bodyExpr: string, returnType: string) =
            let parameters =
                parameters |> List.map(fun p -> Ast.ParameterPat(Ast.ConstantPat(p)))

            Ast.BaseMember(name, parameters, Ast.ConstantExpr(bodyExpr), Ast.LongIdent(returnType))

        /// <summary>
        /// Create a method member definition.
        /// </summary>
        /// <param name="name">The name of the method.</param>
        /// <param name="parameters">The parameters of the method.</param>
        /// <param name="bodyExpr">The body of the method.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         TypeDefn("Person", UnitPat()) {
        ///             Member(
        ///                 "this.Name",
        ///                 ParenPat(ParameterPat("name", String())),
        ///                 Int(23)
        ///             )
        ///         }
        ///     }
        /// }
        /// </code>
        static member Member(name: string, parameters: WidgetBuilder<Pattern>, bodyExpr: WidgetBuilder<Constant>) =
            Ast.BaseMember(name, [ parameters ], Ast.ConstantExpr bodyExpr)

        static member Member
            (
                name: string,
                parameters: WidgetBuilder<Pattern>,
                bodyExpr: WidgetBuilder<Constant>,
                returnType: WidgetBuilder<Type>
            ) =
            Ast.BaseMember(name, [ parameters ], Ast.ConstantExpr bodyExpr, returnType)

        static member Member
            (name: string, parameters: WidgetBuilder<Pattern>, bodyExpr: WidgetBuilder<Constant>, returnType: string)
            =
            Ast.BaseMember(name, [ parameters ], Ast.ConstantExpr bodyExpr, Ast.LongIdent(returnType))

        /// <summary>
        /// Create a method member definition.
        /// </summary>
        /// <param name="name">The name of the method.</param>
        /// <param name="parameters">The parameters of the method.</param>
        /// <param name="bodyExpr">The body of the method.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         TypeDefn("Person", UnitPat()) {
        ///             Member(
        ///                 "this.Name",
        ///                 ParenPat(ParameterPat("name", String())),
        ///                 "23"
        ///             )
        ///         }
        ///     }
        /// }
        /// </code>
        static member Member(name: string, parameters: WidgetBuilder<Pattern>, bodyExpr: string) =
            Ast.BaseMember(name, [ parameters ], Ast.ConstantExpr bodyExpr)

        static member Member
            (name: string, parameters: WidgetBuilder<Pattern>, bodyExpr: string, returnType: WidgetBuilder<Type>)
            =
            Ast.BaseMember(name, [ parameters ], Ast.ConstantExpr bodyExpr, returnType)

        static member Member(name: string, parameters: WidgetBuilder<Pattern>, bodyExpr: string, returnType: string) =
            Ast.BaseMember(name, [ parameters ], Ast.ConstantExpr bodyExpr, Ast.LongIdent(returnType))

        /// <summary>
        /// Create a method member definition.
        /// </summary>
        /// <param name="name">The name of the method.</param>
        /// <param name="parameters">The parameters of the method.</param>
        /// <param name="bodyExpr">The body of the method.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         TypeDefn("Person", UnitPat()) {
        ///             Member(
        ///                 "this.Name",
        ///                 "(name: string)",
        ///                 Int(23)
        ///             )
        ///         }
        ///     }
        /// }
        /// </code>
        static member Member(name: string, parameters: string, bodyExpr: WidgetBuilder<Constant>) =
            Ast.BaseMember(name, [ Ast.ConstantPat parameters ], Ast.ConstantExpr bodyExpr)

        static member Member
            (name: string, parameters: string, bodyExpr: WidgetBuilder<Constant>, returnType: WidgetBuilder<Type>)
            =
            Ast.BaseMember(name, [ Ast.ConstantPat parameters ], Ast.ConstantExpr bodyExpr, returnType)

        static member Member(name: string, parameters: string, bodyExpr: WidgetBuilder<Constant>, returnType: string) =
            Ast.BaseMember(name, [ Ast.ConstantPat parameters ], Ast.ConstantExpr bodyExpr, Ast.LongIdent(returnType))

        /// <summary>
        /// Create a method member definition.
        /// </summary>
        /// <param name="name">The name of the method.</param>
        /// <param name="parameters">The parameters of the method.</param>
        /// <param name="bodyExpr">The body of the method.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         TypeDefn("Person", UnitPat()) {
        ///             Member(
        ///                 "this.Name",
        ///                 "(name: string)",
        ///                 "23"
        ///             )
        ///         }
        ///     }
        /// }
        /// </code>
        static member Member(name: string, parameters: string, bodyExpr: string) =
            Ast.BaseMember(name, [ Ast.ConstantPat parameters ], Ast.ConstantExpr bodyExpr)

        static member Member(name: string, parameters: string, bodyExpr: string, returnType: WidgetBuilder<Type>) =
            Ast.BaseMember(name, [ Ast.ConstantPat parameters ], Ast.ConstantExpr bodyExpr, returnType)

        static member Member(name: string, parameters: string, bodyExpr: string, returnType: string) =
            Ast.BaseMember(name, [ Ast.ConstantPat parameters ], Ast.ConstantExpr bodyExpr, Ast.LongIdent(returnType))

        /// <summary>
        /// Create a method member definition.
        /// </summary>
        /// <param name="name">The name of the method.</param>
        /// <param name="parameters">The parameters of the method.</param>
        /// <param name="bodyExpr">The body of the method.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         TypeDefn("Person", UnitPat()) {
        ///             Member(
        ///                 "this.Name",
        ///                 "(name: string)",
        ///                 ConstantExpr(Int 23)
        ///             )
        ///         }
        ///     }
        /// }
        /// </code>
        static member Member(name: string, parameters: string, bodyExpr: WidgetBuilder<Expr>) =
            Ast.BaseMember(name, [ Ast.ConstantPat parameters ], bodyExpr)

        static member Member
            (name: string, parameters: string, bodyExpr: WidgetBuilder<Expr>, returnType: WidgetBuilder<Type>)
            =
            Ast.BaseMember(name, [ Ast.ConstantPat parameters ], bodyExpr, returnType)

        static member Member(name: string, parameters: string, bodyExpr: WidgetBuilder<Expr>, returnType: string) =
            Ast.BaseMember(name, [ Ast.ConstantPat parameters ], bodyExpr, Ast.LongIdent(returnType))
