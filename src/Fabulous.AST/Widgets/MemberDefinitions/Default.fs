namespace Fabulous.AST

open Fabulous.AST
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak

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

            WidgetBuilder<BindingNode>(
                BindingFunction.WidgetKey,
                AttributesBundle(
                    StackList.three(
                        BindingFunction.Name.WithValue(name),
                        BindingFunction.Parameters.WithValue(parameters),
                        BindingFunction.Leading.WithValue(SingleTextNode.``default``)
                    ),
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
