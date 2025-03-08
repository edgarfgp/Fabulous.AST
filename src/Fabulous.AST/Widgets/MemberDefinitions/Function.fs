namespace Fabulous.AST

open Fabulous.AST
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Syntax
open Fantomas.FCS.Text

module BindingFunction =
    let Name = Attributes.defineScalar<string> "Name"

    let Leading = Attributes.defineScalar<SingleTextNode> "Leading"

    let Parameters = Attributes.defineScalar<Pattern list> "Parameters"

    let WidgetKey =
        Widgets.register "Function" (fun widget ->
            let name = Widgets.getScalarValue widget Name
            let bodyExpr = Widgets.getNodeFromWidget<Expr> widget BindingNode.BodyExpr
            let parameters = Widgets.getScalarValue widget Parameters

            let leading =
                Widgets.tryGetScalarValue widget Leading
                |> ValueOption.defaultValue(SingleTextNode.``let``)

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
                |> ValueOption.map(fun x -> Some(x))
                |> ValueOption.defaultValue None

            let attributes =
                Widgets.tryGetScalarValue widget BindingNode.MultipleAttributes
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
                Widgets.tryGetNodeFromWidget widget BindingNode.TypeParams
                |> ValueOption.map Some
                |> ValueOption.defaultValue None

            BindingNode(
                xmlDocs,
                attributes,
                MultipleTextsNode([ leading ], Range.Zero),
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
            ))

[<AutoOpen>]
module BindingFunctionBuilders =
    type Ast with
        static member private BaseFunction
            (
                name: string,
                parameters: WidgetBuilder<Pattern> list,
                bodyExpr: WidgetBuilder<Expr>,
                ?returnType: WidgetBuilder<Type>
            ) =
            let name = PrettyNaming.NormalizeIdentifierBackticks name
            let parameters = parameters |> List.map Gen.mkOak

            WidgetBuilder<BindingNode>(
                BindingFunction.WidgetKey,
                AttributesBundle(
                    StackList.two(
                        BindingFunction.Name.WithValue(name),
                        BindingFunction.Parameters.WithValue(parameters)
                    ),
                    [| BindingNode.BodyExpr.WithValue(bodyExpr.Compile())
                       match returnType with
                       | Some rt -> BindingNode.Return.WithValue(rt.Compile())
                       | None -> () |],
                    Array.empty
                )
            )

        /// <summary>
        /// Create a function with the given name, parameters, and body expression.
        /// </summary>
        /// <param name="name">The name of the function.</param>
        /// <param name="parameters">The parameters of the function.</param>
        /// <param name="bodyExpr">The body expression of the function.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         Function("add", [ ParameterPat("a"); ParameterPat("b") ], InfixAppExpr("a", "+", "b"))
        ///     }
        /// }
        /// </code>
        static member Function(name: string, parameters: WidgetBuilder<Pattern> list, bodyExpr: WidgetBuilder<Expr>) =
            Ast.BaseFunction(name, parameters, bodyExpr)

        /// <summary>
        /// Create a function with the given name, parameters, and body expression.
        /// </summary>
        /// <param name="name">The name of the function.</param>
        /// <param name="parameters">The parameters of the function.</param>
        /// <param name="bodyExpr">The body expression of the function.</param>
        /// <param name="returnType">The return type of the function.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         Function("add", [ ParameterPat("a"); ParameterPat("b") ], InfixAppExpr("a", "+", "b"), Int())
        ///     }
        /// }
        /// </code>
        static member Function
            (
                name: string,
                parameters: WidgetBuilder<Pattern> list,
                bodyExpr: WidgetBuilder<Expr>,
                returnType: WidgetBuilder<Type>
            ) =
            Ast.BaseFunction(name, parameters, bodyExpr, returnType)

        /// <summary>
        /// Create a function with the given name, parameters, and body expression.
        /// </summary>
        /// <param name="name">The name of the function.</param>
        /// <param name="parameters">The parameters of the function.</param>
        /// <param name="bodyExpr">The body expression of the function.</param>
        /// <param name="returnType">The return type of the function.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         Function("add", [ ParameterPat("a"); ParameterPat("b") ], InfixAppExpr("a", "+", "b"), "int")
        ///     }
        /// }
        /// </code>
        static member Function
            (name: string, parameters: WidgetBuilder<Pattern> list, bodyExpr: WidgetBuilder<Expr>, returnType: string) =
            Ast.BaseFunction(name, parameters, bodyExpr, Ast.LongIdent(returnType))

        /// <summary>
        /// Create a function with the given name, parameters, and body expression.
        /// </summary>
        /// <param name="name">The name of the function.</param>
        /// <param name="parameters">The parameters of the function.</param>
        /// <param name="bodyExpr">The body expression of the function.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         Function(
        ///            "cylinderVolume",
        ///            [ ParameterPat "radius"; ParameterPat "length" ],
        ///            [ LetOrUseExpr(Value("pi", Double(3.14159)))
        ///              OtherExpr(
        ///                   InfixAppExpr("length", "*", InfixAppExpr("pi", "*", InfixAppExpr("radius", "*", "radius")))
        ///              ) ]
        ///        )
        ///     }
        /// }
        /// </code>
        static member Function
            (
                name: string,
                parameters: WidgetBuilder<Pattern> list,
                bodyExpr: WidgetBuilder<ComputationExpressionStatement> list
            ) =
            Ast.BaseFunction(name, parameters, Ast.CompExprBodyExpr(bodyExpr))

        /// <summary>
        /// Create a function with the given name, parameters, and body expression.
        /// </summary>
        /// <param name="name">The name of the function.</param>
        /// <param name="parameters">The parameters of the function.</param>
        /// <param name="bodyExpr">The body expression of the function.</param>
        /// <param name="returnType">The return type of the function.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         Function(
        ///             "cylinderVolume",
        ///             [ ParameterPat "radius"; ParameterPat "length" ],
        ///             [ LetOrUseExpr(Value("pi", Double(3.14159)))
        ///               OtherExpr(
        ///                 InfixAppExpr("length", "*", InfixAppExpr("pi", "*", InfixAppExpr("radius", "*", "radius")))
        ///             ) ],
        ///             LongIdent("double")
        ///         )
        ///     }
        /// }
        /// </code>
        static member Function
            (
                name: string,
                parameters: WidgetBuilder<Pattern> list,
                bodyExpr: WidgetBuilder<ComputationExpressionStatement> list,
                returnType: WidgetBuilder<Type>
            ) =
            Ast.BaseFunction(name, parameters, Ast.CompExprBodyExpr(bodyExpr), returnType)

        /// <summary>
        /// Create a function with the given name, parameters, and body expression.
        /// </summary>
        /// <param name="name">The name of the function.</param>
        /// <param name="parameters">The parameters of the function.</param>
        /// <param name="bodyExpr">The body expression of the function.</param>
        /// <param name="returnType">The return type of the function.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         Function(
        ///             "cylinderVolume",
        ///             [ ParameterPat "radius"; ParameterPat "length" ],
        ///             [ LetOrUseExpr(Value("pi", Double(3.14159)))
        ///               OtherExpr(
        ///                 InfixAppExpr("length", "*", InfixAppExpr("pi", "*", InfixAppExpr("radius", "*", "radius")))
        ///             ) ],
        ///             "double"
        ///         )
        ///     }
        /// }
        /// </code>
        static member Function
            (
                name: string,
                parameters: WidgetBuilder<Pattern> list,
                bodyExpr: WidgetBuilder<ComputationExpressionStatement> list,
                returnType: string
            ) =
            Ast.BaseFunction(name, parameters, Ast.CompExprBodyExpr(bodyExpr), Ast.LongIdent(returnType))

        /// <summary>
        /// Create a function with the given name, parameters, and body expression.
        /// </summary>
        /// <param name="name">The name of the function.</param>
        /// <param name="parameters">The parameters of the function.</param>
        /// <param name="bodyExpr">The body expression of the function.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///        Function(
        ///            "cylinderVolume",
        ///            [ ParameterPat "radius"; ParameterPat "length" ],
        ///            [ InfixAppExpr("length", "*", InfixAppExpr("pi", "*", InfixAppExpr("radius", "*", "radius"))) ]
        ///        )
        ///     }
        /// }
        ///</code>
        static member Function
            (name: string, parameters: WidgetBuilder<Pattern> list, bodyExpr: WidgetBuilder<Expr> list)
            =
            Ast.BaseFunction(name, parameters, Ast.CompExprBodyExpr(bodyExpr))

        /// <summary>
        /// Create a function with the given name, parameters, and body expression.
        /// </summary>
        /// <param name="name">The name of the function.</param>
        /// <param name="parameters">The parameters of the function.</param>
        /// <param name="bodyExpr">The body expression of the function.</param>
        /// <param name="returnType">The return type of the function.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         Function(
        ///             "cylinderVolume",
        ///             [ ParameterPat "radius"; ParameterPat "length" ],
        ///             [ InfixAppExpr("length", "*", InfixAppExpr("pi", "*", InfixAppExpr("radius", "*", "radius"))) ],
        ///             LongIdent("double")
        ///         )
        ///     }
        /// }
        /// </code>
        static member Function
            (
                name: string,
                parameters: WidgetBuilder<Pattern> list,
                bodyExpr: WidgetBuilder<Expr> list,
                returnType: WidgetBuilder<Type>
            ) =
            Ast.BaseFunction(name, parameters, Ast.CompExprBodyExpr(bodyExpr), returnType)

        /// <summary>
        /// Create a function with the given name, parameters, and body expression.
        /// </summary>
        /// <param name="name">The name of the function.</param>
        /// <param name="parameters">The parameters of the function.</param>
        /// <param name="bodyExpr">The body expression of the function.</param>
        /// <param name="returnType">The return type of the function.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         Function(
        ///             "cylinderVolume",
        ///             [ ParameterPat "radius"; ParameterPat "length" ],
        ///             [ InfixAppExpr("length", "*", InfixAppExpr("pi", "*", InfixAppExpr("radius", "*", "radius"))) ],
        ///             "double"
        ///         )
        ///     }
        /// }
        /// </code>
        static member Function
            (
                name: string,
                parameters: WidgetBuilder<Pattern> list,
                bodyExpr: WidgetBuilder<Expr> list,
                returnType: string
            ) =
            Ast.BaseFunction(name, parameters, Ast.CompExprBodyExpr(bodyExpr), Ast.LongIdent(returnType))

        /// <summary>
        /// Create a function with the given name, parameters, and body expression.
        /// </summary>
        /// <param name="name">The name of the function.</param>
        /// <param name="parameters">The parameters of the function.</param>
        /// <param name="bodyExpr">The body expression of the function.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         Function(
        ///             "cylinderVolume",
        ///             [ ParameterPat "radius" ],
        ///             [ Value("pi", Double(3.14159))
        ///               Value("length", Int(23))
        ///               Value("radius", Int(23)) ]
        ///         )
        ///     }
        /// }
        /// </code>
        static member Function
            (name: string, parameters: WidgetBuilder<Pattern> list, bodyExpr: WidgetBuilder<BindingNode> list)
            =
            Ast.BaseFunction(name, parameters, Ast.CompExprBodyExpr(bodyExpr))

        /// <summary>
        /// Create a function with the given name, parameters, and body expression.
        /// </summary>
        /// <param name="name">The name of the function.</param>
        /// <param name="parameters">The parameters of the function.</param>
        /// <param name="bodyExpr">The body expression of the function.</param>
        /// <param name="returnType">The return type of the function.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         Function(
        ///             "cylinderVolume",
        ///             [ ParameterPat "radius" ],
        ///             [ Value("pi", Double(3.14159))
        ///               Value("length", Int(23))
        ///               Value("radius", Int(23)) ],
        ///             LongIdent("double")
        ///         )
        ///     }
        /// }
        /// </code>
        static member Function
            (
                name: string,
                parameters: WidgetBuilder<Pattern> list,
                bodyExpr: WidgetBuilder<BindingNode> list,
                returnType: WidgetBuilder<Type>
            ) =
            Ast.BaseFunction(name, parameters, Ast.CompExprBodyExpr(bodyExpr), returnType)

        /// <summary>
        /// Create a function with the given name, parameters, and body expression.
        /// </summary>
        /// <param name="name">The name of the function.</param>
        /// <param name="parameters">The parameters of the function.</param>
        /// <param name="bodyExpr">The body expression of the function.</param>
        /// <param name="returnType">The return type of the function.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         Function(
        ///             "cylinderVolume",
        ///             [ ParameterPat "radius" ],
        ///             [ Value("pi", Double(3.14159))
        ///               Value("length", Int(23))
        ///               Value("radius", Int(23)) ],
        ///             "double"
        ///         )
        ///     }
        /// }
        /// </code>
        static member Function
            (
                name: string,
                parameters: WidgetBuilder<Pattern> list,
                bodyExpr: WidgetBuilder<BindingNode> list,
                returnType: string
            ) =
            Ast.BaseFunction(name, parameters, Ast.CompExprBodyExpr(bodyExpr), Ast.LongIdent(returnType))

        /// <summary>
        /// Create a function with the given name, parameters, and body expression.
        /// </summary>
        /// <param name="name">The name of the function.</param>
        /// <param name="parameter">The parameter of the function.</param>
        /// <param name="bodyExpr">The body expression of the function.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         Function(
        ///             "cylinderVolume",
        ///             ParameterPat "radius",
        ///             [ LetOrUseExpr(Value("pi", Double(3.14159)))
        ///               OtherExpr(
        ///                 InfixAppExpr("length", "*", InfixAppExpr("pi", "*", InfixAppExpr("radius", "*", "radius")))) ]
        ///         )
        ///     }
        /// }
        /// </code>
        static member Function
            (
                name: string,
                parameter: WidgetBuilder<Pattern>,
                bodyExpr: WidgetBuilder<ComputationExpressionStatement> list
            ) =
            Ast.BaseFunction(name, [ parameter ], Ast.CompExprBodyExpr(bodyExpr))

        /// <summary>
        /// Create a function with the given name, parameters, and body expression.
        /// </summary>
        /// <param name="name">The name of the function.</param>
        /// <param name="parameter">The parameter of the function.</param>
        /// <param name="bodyExpr">The body expression of the function.</param>
        /// <param name="returnType">The return type of the function.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         Function(
        ///             "cylinderVolume",
        ///             ParameterPat "radius",
        ///             [ LetOrUseExpr(Value("pi", Double(3.14159)))
        ///               OtherExpr(
        ///                 InfixAppExpr("length", "*", InfixAppExpr("pi", "*", InfixAppExpr("radius", "*", "radius")))) ],
        ///             LongIdent("double")
        ///         )
        ///     }
        /// }
        /// </code>
        static member Function
            (
                name: string,
                parameter: WidgetBuilder<Pattern>,
                bodyExpr: WidgetBuilder<ComputationExpressionStatement> list,
                returnType: WidgetBuilder<Type>
            ) =
            Ast.BaseFunction(name, [ parameter ], Ast.CompExprBodyExpr(bodyExpr), returnType)

        /// <summary>
        /// Create a function with the given name, parameters, and body expression.
        /// </summary>
        /// <param name="name">The name of the function.</param>
        /// <param name="parameter">The parameter of the function.</param>
        /// <param name="bodyExpr">The body expression of the function.</param>
        /// <param name="returnType">The return type of the function.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         Function(
        ///             "cylinderVolume",
        ///             ParameterPat "radius",
        ///             [ LetOrUseExpr(Value("pi", Double(3.14159)))
        ///               OtherExpr(
        ///                 InfixAppExpr("length", "*", InfixAppExpr("pi", "*", InfixAppExpr("radius", "*", "radius")))) ],
        ///             "double"
        ///         )
        ///     }
        /// }
        /// </code>
        static member Function
            (
                name: string,
                parameter: WidgetBuilder<Pattern>,
                bodyExpr: WidgetBuilder<ComputationExpressionStatement> list,
                returnType: string
            ) =
            Ast.BaseFunction(name, [ parameter ], Ast.CompExprBodyExpr(bodyExpr), Ast.LongIdent(returnType))

        /// <summary>
        /// Create a function with the given name, parameters, and body expression.
        /// </summary>
        /// <param name="name">The name of the function.</param>
        /// <param name="parameter">The parameter of the function.</param>
        /// <param name="bodyExpr">The body expression of the function.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///    AnonymousModule() {
        ///         Function(
        ///             "cylinderVolume",
        ///             ParameterPat "radius",
        ///             [ InfixAppExpr("length", "*", InfixAppExpr("pi", "*", InfixAppExpr("radius", "*", "radius"))) ]
        ///         )
        ///     }
        /// }
        /// </code>
        static member Function(name: string, parameter: WidgetBuilder<Pattern>, bodyExpr: WidgetBuilder<Expr> list) =
            Ast.BaseFunction(name, [ parameter ], Ast.CompExprBodyExpr(bodyExpr))

        /// <summary>
        /// Create a function with the given name, parameters, and body expression.
        /// </summary>
        /// <param name="name">The name of the function.</param>
        /// <param name="parameter">The parameter of the function.</param>
        /// <param name="bodyExpr">The body expression of the function.</param>
        /// <param name="returnType">The return type of the function.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         Function(
        ///             "cylinderVolume",
        ///             ParameterPat "radius",
        ///             [ InfixAppExpr("length", "*", InfixAppExpr("pi", "*", InfixAppExpr("radius", "*", "radius"))) ],
        ///             LongIdent("double")
        ///         )
        ///     }
        /// }
        /// </code>
        static member Function
            (
                name: string,
                parameter: WidgetBuilder<Pattern>,
                bodyExpr: WidgetBuilder<Expr> list,
                returnType: WidgetBuilder<Type>
            ) =
            Ast.BaseFunction(name, [ parameter ], Ast.CompExprBodyExpr(bodyExpr), returnType)

        /// <summary>
        /// Create a function with the given name, parameters, and body expression.
        /// </summary>
        /// <param name="name">The name of the function.</param>
        /// <param name="parameter">The parameter of the function.</param>
        /// <param name="bodyExpr">The body expression of the function.</param>
        /// <param name="returnType">The return type of the function.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         Function(
        ///             "cylinderVolume",
        ///             ParameterPat "radius",
        ///             [ InfixAppExpr("length", "*", InfixAppExpr("pi", "*", InfixAppExpr("radius", "*", "radius"))) ],
        ///             "double"
        ///         )
        ///     }
        /// }
        /// </code>
        static member Function
            (name: string, parameter: WidgetBuilder<Pattern>, bodyExpr: WidgetBuilder<Expr> list, returnType: string) =
            Ast.BaseFunction(name, [ parameter ], Ast.CompExprBodyExpr(bodyExpr), Ast.LongIdent(returnType))

        /// <summary>
        /// Create a function with the given name, parameters, and body expression.
        /// </summary>
        /// <param name="name">The name of the function.</param>
        /// <param name="parameter">The parameter of the function.</param>
        /// <param name="bodyExpr">The body expression of the function.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         Function(
        ///             "cylinderVolume",
        ///             ParameterPat "radius",
        ///             [ Value("pi", Double(3.14159))
        ///               Value("length", Int(23))
        ///               Value("radius", Int(23)) ]
        ///         )
        ///     }
        /// }
        /// </code>
        static member Function
            (name: string, parameter: WidgetBuilder<Pattern>, bodyExpr: WidgetBuilder<BindingNode> list)
            =
            Ast.BaseFunction(name, [ parameter ], Ast.CompExprBodyExpr(bodyExpr))

        /// <summary>
        /// Create a function with the given name, parameters, and body expression.
        /// </summary>
        /// <param name="name">The name of the function.</param>
        /// <param name="parameter">The parameter of the function.</param>
        /// <param name="bodyExpr">The body expression of the function.</param>
        /// <param name="returnType">The return type of the function.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         Function(
        ///             "cylinderVolume",
        ///             ParameterPat "radius",
        ///             [ Value("pi", Double(3.14159))
        ///               Value("length", Int(23))
        ///               Value("radius", Int(23)) ],
        ///             LongIdent("double")
        ///         )
        ///     }
        /// }
        /// </code>
        static member Function
            (
                name: string,
                parameter: WidgetBuilder<Pattern>,
                bodyExpr: WidgetBuilder<BindingNode> list,
                returnType: WidgetBuilder<Type>
            ) =
            Ast.BaseFunction(name, [ parameter ], Ast.CompExprBodyExpr(bodyExpr), returnType)

        /// <summary>
        /// Create a function with the given name, parameters, and body expression.
        /// </summary>
        /// <param name="name">The name of the function.</param>
        /// <param name="parameter">The parameter of the function.</param>
        /// <param name="bodyExpr">The body expression of the function.</param>
        /// <param name="returnType">The return type of the function.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         Function(
        ///             "cylinderVolume",
        ///             ParameterPat "radius",
        ///             [ Value("pi", Double(3.14159))
        ///               Value("length", Int(23))
        ///               Value("radius", Int(23)) ],
        ///             "double"
        ///         )
        ///     }
        /// }
        /// </code>
        static member Function
            (
                name: string,
                parameter: WidgetBuilder<Pattern>,
                bodyExpr: WidgetBuilder<BindingNode> list,
                returnType: string
            ) =
            Ast.BaseFunction(name, [ parameter ], Ast.CompExprBodyExpr(bodyExpr), Ast.LongIdent(returnType))

        /// <summary>
        /// Create a function with the given name, parameters, and body expression.
        /// </summary>
        /// <param name="name">The name of the function.</param>
        /// <param name="parameters">The parameters of the function.</param>
        /// <param name="bodyExpr">The body expression of the function.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         Function("add", [ ParameterPat("a"); ParameterPat("b") ], Constant("a + b"))
        ///     }
        /// }
        /// </code>
        static member Function
            (name: string, parameters: WidgetBuilder<Pattern> list, bodyExpr: WidgetBuilder<Constant>)
            =
            Ast.BaseFunction(name, parameters, Ast.ConstantExpr(bodyExpr))

        /// <summary>
        /// Create a function with the given name, parameters, and body expression.
        /// </summary>
        /// <param name="name">The name of the function.</param>
        /// <param name="parameters">The parameters of the function.</param>
        /// <param name="bodyExpr">The body expression of the function.</param>
        /// <param name="returnType">The return type of the function.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         Function("add", [ ParameterPat("a"); ParameterPat("b") ], Constant("a + b"), Int())
        ///     }
        /// }
        /// </code>
        static member Function
            (
                name: string,
                parameters: WidgetBuilder<Pattern> list,
                bodyExpr: WidgetBuilder<Constant>,
                returnType: WidgetBuilder<Type>
            ) =
            Ast.BaseFunction(name, parameters, Ast.ConstantExpr(bodyExpr), returnType)

        /// <summary>
        /// Create a function with the given name, parameters, and body expression.
        /// </summary>
        /// <param name="name">The name of the function.</param>
        /// <param name="parameters">The parameters of the function.</param>
        /// <param name="bodyExpr">The body expression of the function.</param>
        /// <param name="returnType">The return type of the function.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         Function("add", [ ParameterPat("a"); ParameterPat("b") ], Constant("a + b"), "int")
        ///     }
        /// }
        /// </code>
        static member Function
            (
                name: string,
                parameters: WidgetBuilder<Pattern> list,
                bodyExpr: WidgetBuilder<Constant>,
                returnType: string
            ) =
            Ast.BaseFunction(name, parameters, Ast.ConstantExpr(bodyExpr), Ast.LongIdent(returnType))

        /// <summary>
        /// Create a function with the given name, parameters, and body expression.
        /// </summary>
        /// <param name="name">The name of the function.</param>
        /// <param name="parameters">The parameters of the function.</param>
        /// <param name="bodyExpr">The body expression of the function.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         Function("add", [ ParameterPat("a"); ParameterPat("b") ], "a + b")
        ///     }
        /// }
        /// </code>
        static member Function(name: string, parameters: WidgetBuilder<Pattern> list, bodyExpr: string) =
            Ast.BaseFunction(name, parameters, Ast.ConstantExpr(Ast.Constant(bodyExpr)))

        /// <summary>
        /// Create a function with the given name, parameters, and body expression.
        /// </summary>
        /// <param name="name">The name of the function.</param>
        /// <param name="parameters">The parameters of the function.</param>
        /// <param name="bodyExpr">The body expression of the function.</param>
        /// <param name="returnType">The return type of the function.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         Function("add", [ ParameterPat("a"); ParameterPat("b") ], "a + b", Int())
        ///     }
        /// }
        /// </code>
        static member Function
            (name: string, parameters: WidgetBuilder<Pattern> list, bodyExpr: string, returnType: WidgetBuilder<Type>) =
            Ast.BaseFunction(name, parameters, Ast.ConstantExpr(Ast.Constant(bodyExpr)), returnType)

        /// <summary>
        /// Create a function with the given name, parameters, and body expression.
        /// </summary>
        /// <param name="name">The name of the function.</param>
        /// <param name="parameters">The parameters of the function.</param>
        /// <param name="bodyExpr">The body expression of the function.</param>
        /// <param name="returnType">The return type of the function.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         Function("add", [ ParameterPat("a"); ParameterPat("b") ], "a + b", "int")
        ///     }
        /// }
        /// </code>
        static member Function
            (name: string, parameters: WidgetBuilder<Pattern> list, bodyExpr: string, returnType: string)
            =
            Ast.BaseFunction(name, parameters, Ast.ConstantExpr(Ast.Constant(bodyExpr)), Ast.LongIdent(returnType))

        /// <summary>
        /// Create a function with the given name, parameters, and body expression.
        /// </summary>
        /// <param name="name">The name of the function.</param>
        /// <param name="parameters">The parameters of the function.</param>
        /// <param name="bodyExpr">The body expression of the function.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         Function("add", [ "a"; "b" ], InfixAppExpr("a", "+", "b"))
        ///     }
        /// }
        /// </code>
        static member Function(name: string, parameters: string list, bodyExpr: WidgetBuilder<Expr>) =
            let parameters =
                parameters
                |> List.map(fun p -> Ast.ParameterPat(Ast.ConstantPat(Ast.Constant(p))))

            Ast.BaseFunction(name, parameters, bodyExpr)

        /// <summary>
        /// Create a function with the given name, parameters, and body expression.
        /// </summary>
        /// <param name="name">The name of the function.</param>
        /// <param name="parameters">The parameters of the function.</param>
        /// <param name="bodyExpr">The body expression of the function.</param>
        /// <param name="returnType">The return type of the function.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         Function("add", [ "a"; "b" ], InfixAppExpr("a", "+", "b"), Int())
        ///     }
        /// }
        /// </code>
        static member Function
            (name: string, parameters: string list, bodyExpr: WidgetBuilder<Expr>, returnType: WidgetBuilder<Type>) =
            let parameters =
                parameters
                |> List.map(fun p -> Ast.ParameterPat(Ast.ConstantPat(Ast.Constant(p))))

            Ast.Function(name, parameters, bodyExpr, returnType)

        /// <summary>
        /// Create a function with the given name, parameters, and body expression.
        /// </summary>
        /// <param name="name">The name of the function.</param>
        /// <param name="parameters">The parameters of the function.</param>
        /// <param name="bodyExpr">The body expression of the function.</param>
        /// <param name="returnType">The return type of the function.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         Function("add", [ "a"; "b" ], InfixAppExpr("a", "+", "b"), "int")
        ///     }
        /// }
        /// </code>
        static member Function
            (name: string, parameters: string list, bodyExpr: WidgetBuilder<Expr>, returnType: string)
            =
            let parameters =
                parameters
                |> List.map(fun p -> Ast.ParameterPat(Ast.ConstantPat(Ast.Constant(p))))

            Ast.Function(name, parameters, bodyExpr, Ast.LongIdent(returnType))

        /// <summary>
        /// Create a function with the given name, parameters, and body expression.
        /// </summary>
        /// <param name="name">The name of the function.</param>
        /// <param name="parameters">The parameters of the function.</param>
        /// <param name="bodyExpr">The body expression of the function.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         Function("add", [ "a"; "b" ], Constant("a + b"))
        ///     }
        /// }
        /// </code>
        static member Function(name: string, parameters: string list, bodyExpr: WidgetBuilder<Constant>) =
            let parameters =
                parameters |> List.map(fun p -> Ast.ParameterPat(Ast.ConstantPat(p)))

            Ast.Function(name, parameters, bodyExpr)

        /// <summary>
        /// Create a function with the given name, parameters, and body expression.
        /// </summary>
        /// <param name="name">The name of the function.</param>
        /// <param name="parameters">The parameters of the function.</param>
        /// <param name="bodyExpr">The body expression of the function.</param>
        /// <param name="returnType">The return type of the function.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         Function("add", [ "a"; "b" ], Constant("a + b"), Int())
        ///     }
        /// }
        /// </code>
        static member Function
            (name: string, parameters: string list, bodyExpr: WidgetBuilder<Constant>, returnType: WidgetBuilder<Type>) =
            let parameters =
                parameters |> List.map(fun p -> Ast.ParameterPat(Ast.ConstantPat(p)))

            Ast.Function(name, parameters, bodyExpr, returnType)

        /// <summary>
        /// Create a function with the given name, parameters, and body expression.
        /// </summary>
        /// <param name="name">The name of the function.</param>
        /// <param name="parameters">The parameters of the function.</param>
        /// <param name="bodyExpr">The body expression of the function.</param>
        /// <param name="returnType">The return type of the function.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         Function("add", [ "a"; "b" ], Constant("a + b"), "int")
        ///     }
        /// }
        /// </code>
        static member Function
            (name: string, parameters: string list, bodyExpr: WidgetBuilder<Constant>, returnType: string)
            =
            let parameters =
                parameters |> List.map(fun p -> Ast.ParameterPat(Ast.ConstantPat(p)))

            Ast.Function(name, parameters, bodyExpr, Ast.LongIdent(returnType))

        /// <summary>
        /// Create a function with the given name, parameters, and body expression.
        /// </summary>
        /// <param name="name">The name of the function.</param>
        /// <param name="parameters">The parameters of the function.</param>
        /// <param name="bodyExpr">The body expression of the function.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         Function("add", [ "a"; "b" ], "a + b")
        ///     }
        /// }
        /// </code>
        static member Function(name: string, parameters: string list, bodyExpr: string) =
            let parameters =
                parameters |> List.map(fun p -> Ast.ParameterPat(Ast.ConstantPat(p)))

            Ast.Function(name, parameters, bodyExpr)

        /// <summary>
        /// Create a function with the given name, parameters, and body expression.
        /// </summary>
        /// <param name="name">The name of the function.</param>
        /// <param name="parameters">The parameters of the function.</param>
        /// <param name="bodyExpr">The body expression of the function.</param>
        /// <param name="returnType">The return type of the function.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         Function("add", [ "a"; "b" ], "a + b", Int())
        ///     }
        /// }
        /// </code>
        static member Function
            (name: string, parameters: string list, bodyExpr: string, returnType: WidgetBuilder<Type>)
            =
            let parameters =
                parameters |> List.map(fun p -> Ast.ParameterPat(Ast.ConstantPat(p)))

            Ast.Function(name, parameters, bodyExpr, returnType)

        /// <summary>
        /// Create a function with the given name, parameters, and body expression.
        /// </summary>
        /// <param name="name">The name of the function.</param>
        /// <param name="parameters">The parameters of the function.</param>
        /// <param name="bodyExpr">The body expression of the function.</param>
        /// <param name="returnType">The return type of the function.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         Function("add", [ "a"; "b" ], "a + b", "int")
        ///     }
        /// }
        /// </code>
        static member Function(name: string, parameters: string list, bodyExpr: string, returnType: string) =
            let parameters =
                parameters |> List.map(fun p -> Ast.ParameterPat(Ast.ConstantPat(p)))

            Ast.Function(name, parameters, bodyExpr, Ast.LongIdent(returnType))

        /// <summary>
        /// Create a function with the given name, parameters, and body expression.
        /// </summary>
        /// <param name="name">The name of the function.</param>
        /// <param name="parameters">The parameters of the function.</param>
        /// <param name="bodyExpr">The body expression of the function.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         Function("add", ParameterPat("a b"), InfixAppExpr("a", "+", "b"))
        ///     }
        /// }
        /// </code>
        static member Function(name: string, parameters: WidgetBuilder<Pattern>, bodyExpr: WidgetBuilder<Expr>) =
            Ast.Function(name, [ parameters ], bodyExpr)

        /// <summary>
        /// Create a function with the given name, parameters, and body expression.
        /// </summary>
        /// <param name="name">The name of the function.</param>
        /// <param name="parameters">The parameters of the function.</param>
        /// <param name="bodyExpr">The body expression of the function.</param>
        /// <param name="returnType">The return type of the function.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         Function("add", ParameterPat("a b"), InfixAppExpr("a", "+", "b"), Int())
        ///     }
        /// }
        /// </code>
        static member Function
            (
                name: string,
                parameters: WidgetBuilder<Pattern>,
                bodyExpr: WidgetBuilder<Expr>,
                returnType: WidgetBuilder<Type>
            ) =
            Ast.Function(name, [ parameters ], bodyExpr, returnType)

        /// <summary>
        /// Create a function with the given name, parameters, and body expression.
        /// </summary>
        /// <param name="name">The name of the function.</param>
        /// <param name="parameters">The parameters of the function.</param>
        /// <param name="bodyExpr">The body expression of the function.</param>
        /// <param name="returnType">The return type of the function.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         Function("add", ParameterPat("a b"), InfixAppExpr("a", "+", "b"), "int")
        ///     }
        /// }
        /// </code>
        static member Function
            (name: string, parameters: WidgetBuilder<Pattern>, bodyExpr: WidgetBuilder<Expr>, returnType: string) =
            Ast.Function(name, [ parameters ], bodyExpr, Ast.LongIdent(returnType))

        /// <summary>
        /// Create a function with the given name, parameters, and body expression.
        /// </summary>
        /// <param name="name">The name of the function.</param>
        /// <param name="parameters">The parameters of the function.</param>
        /// <param name="bodyExpr">The body expression of the function.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         Function("add", ParameterPat("a b"), Constant("a + b"))
        ///     }
        /// }
        /// </code>
        static member Function(name: string, parameters: WidgetBuilder<Pattern>, bodyExpr: WidgetBuilder<Constant>) =
            Ast.Function(name, [ parameters ], bodyExpr)

        /// <summary>
        /// Create a function with the given name, parameters, and body expression.
        /// </summary>
        /// <param name="name">The name of the function.</param>
        /// <param name="parameters">The parameters of the function.</param>
        /// <param name="bodyExpr">The body expression of the function.</param>
        /// <param name="returnType">The return type of the function.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         Function("add", ParameterPat("a b"), Constant("a + b"), Int())
        ///     }
        /// }
        /// </code>
        static member Function
            (
                name: string,
                parameters: WidgetBuilder<Pattern>,
                bodyExpr: WidgetBuilder<Constant>,
                returnType: WidgetBuilder<Type>
            ) =
            Ast.Function(name, [ parameters ], bodyExpr, returnType)

        /// <summary>
        /// Create a function with the given name, parameters, and body expression.
        /// </summary>
        /// <param name="name">The name of the function.</param>
        /// <param name="parameters">The parameters of the function.</param>
        /// <param name="bodyExpr">The body expression of the function.</param>
        /// <param name="returnType">The return type of the function.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         Function("add", ParameterPat("a b"), Constant("a + b"), "int")
        ///     }
        /// }
        /// </code>
        static member Function
            (name: string, parameters: WidgetBuilder<Pattern>, bodyExpr: WidgetBuilder<Constant>, returnType: string) =
            Ast.Function(name, [ parameters ], bodyExpr, Ast.LongIdent(returnType))

        /// <summary>
        /// Create a function with the given name, parameters, and body expression.
        /// </summary>
        /// <param name="name">The name of the function.</param>
        /// <param name="parameters">The parameters of the function.</param>
        /// <param name="bodyExpr">The body expression of the function.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         Function("add", ParameterPat("a b"), "a + b")
        ///     }
        /// }
        /// </code>
        static member Function(name: string, parameters: WidgetBuilder<Pattern>, bodyExpr: string) =
            Ast.Function(name, [ parameters ], bodyExpr)

        /// <summary>
        /// Create a function with the given name, parameters, and body expression.
        /// </summary>
        /// <param name="name">The name of the function.</param>
        /// <param name="parameters">The parameters of the function.</param>
        /// <param name="bodyExpr">The body expression of the function.</param>
        /// <param name="returnType">The return type of the function.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         Function("add", ParameterPat("a b"), "a + b", Int())
        ///     }
        /// }
        /// </code>
        static member Function
            (name: string, parameters: WidgetBuilder<Pattern>, bodyExpr: string, returnType: WidgetBuilder<Type>) =
            Ast.Function(name, [ parameters ], bodyExpr, returnType)

        /// <summary>
        /// Create a function with the given name, parameters, and body expression.
        /// </summary>
        /// <param name="name">The name of the function.</param>
        /// <param name="parameters">The parameters of the function.</param>
        /// <param name="bodyExpr">The body expression of the function.</param>
        /// <param name="returnType">The return type of the function.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         Function("add", ParameterPat("a b"), "a + b", "int")
        ///     }
        /// }
        /// </code>
        static member Function(name: string, parameters: WidgetBuilder<Pattern>, bodyExpr: string, returnType: string) =
            Ast.Function(name, [ parameters ], bodyExpr, Ast.LongIdent(returnType))

        /// <summary>
        /// Create a function with the given name, parameters, and body expression.
        /// </summary>
        /// <param name="name">The name of the function.</param>
        /// <param name="parameters">The parameters of the function.</param>
        /// <param name="bodyExpr">The body expression of the function.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         Function("add", "a b", InfixAppExpr("a", "+", "b"))
        ///     }
        /// }
        /// </code>
        static member Function(name: string, parameters: string, bodyExpr: WidgetBuilder<Expr>) =
            Ast.Function(name, [ parameters ], bodyExpr)

        /// <summary>
        /// Create a function with the given name, parameters, and body expression.
        /// </summary>
        /// <param name="name">The name of the function.</param>
        /// <param name="parameters">The parameters of the function.</param>
        /// <param name="bodyExpr">The body expression of the function.</param>
        /// <param name="returnType">The return type of the function.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         Function("add", "a b", InfixAppExpr("a", "+", "b"), Int())
        ///     }
        /// }
        /// </code>
        static member Function
            (name: string, parameters: string, bodyExpr: WidgetBuilder<Expr>, returnType: WidgetBuilder<Type>)
            =
            Ast.Function(name, [ parameters ], bodyExpr, returnType)

        /// <summary>
        /// Create a function with the given name, parameters, and body expression.
        /// </summary>
        /// <param name="name">The name of the function.</param>
        /// <param name="parameters">The parameters of the function.</param>
        /// <param name="bodyExpr">The body expression of the function.</param>
        /// <param name="returnType">The return type of the function.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         Function("add", "a b", InfixAppExpr("a", "+", "b"), "int")
        ///     }
        /// }
        /// </code>
        static member Function(name: string, parameters: string, bodyExpr: WidgetBuilder<Expr>, returnType: string) =
            Ast.Function(name, [ parameters ], bodyExpr, Ast.LongIdent(returnType))

        /// <summary>
        /// Create a function with the given name, parameters, and body expression.
        /// </summary>
        /// <param name="name">The name of the function.</param>
        /// <param name="parameters">The parameters of the function.</param>
        /// <param name="bodyExpr">The body expression of the function.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         Function("add", "a b", Constant("a + b"))
        ///     }
        /// }
        /// </code>
        static member Function(name: string, parameters: string, bodyExpr: WidgetBuilder<Constant>) =
            Ast.Function(name, [ parameters ], bodyExpr)

        /// <summary>
        /// Create a function with the given name, parameters, and body expression.
        /// </summary>
        /// <param name="name">The name of the function.</param>
        /// <param name="parameters">The parameters of the function.</param>
        /// <param name="bodyExpr">The body expression of the function.</param>
        /// <param name="returnType">The return type of the function.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         Function("add", "a b", Constant("a + b"), Int())
        ///     }
        /// }
        /// </code>
        static member Function
            (name: string, parameters: string, bodyExpr: WidgetBuilder<Constant>, returnType: WidgetBuilder<Type>) =
            Ast.Function(name, [ parameters ], bodyExpr, returnType)

        /// <summary>
        /// Create a function with the given name, parameters, and body expression.
        /// </summary>
        /// <param name="name">The name of the function.</param>
        /// <param name="parameters">The parameters of the function.</param>
        /// <param name="bodyExpr">The body expression of the function.</param>
        /// <param name="returnType">The return type of the function.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         Function("add", "a b", Constant("a + b"), "int")
        ///     }
        /// }
        /// </code>
        static member Function
            (name: string, parameters: string, bodyExpr: WidgetBuilder<Constant>, returnType: string)
            =
            Ast.Function(name, [ parameters ], bodyExpr, Ast.LongIdent(returnType))

        /// <summary>
        /// Create a function with the given name, parameters, and body expression.
        /// </summary>
        /// <param name="name">The name of the function.</param>
        /// <param name="parameters">The parameters of the function.</param>
        /// <param name="bodyExpr">The body expression of the function.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         Function("add", "a b", "a + b")
        ///     }
        /// }
        /// </code>
        static member Function(name: string, parameters: string, bodyExpr: string) =
            Ast.Function(name, [ parameters ], bodyExpr)

        /// <summary>
        /// Create a function with the given name, parameters, and body expression.
        /// </summary>
        /// <param name="name">The name of the function.</param>
        /// <param name="parameters">The parameters of the function.</param>
        /// <param name="bodyExpr">The body expression of the function.</param>
        /// <param name="returnType">The return type of the function.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         Function("add", "a b", "a + b", Int())
        ///     }
        /// }
        /// </code>
        static member Function(name: string, parameters: string, bodyExpr: string, returnType: WidgetBuilder<Type>) =
            Ast.Function(name, [ parameters ], bodyExpr, returnType)

        /// <summary>
        /// Create a function with the given name, parameters, and body expression.
        /// </summary>
        /// <param name="name">The name of the function.</param>
        /// <param name="parameters">The parameters of the function.</param>
        /// <param name="bodyExpr">The body expression of the function.</param>
        /// <param name="returnType">The return type of the function.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         Function("add", "a b", "a + b", "int")
        ///     }
        /// }
        /// </code>
        static member Function(name: string, parameters: string, bodyExpr: string, returnType: string) =
            Ast.Function(name, [ parameters ], bodyExpr, Ast.LongIdent(returnType))
