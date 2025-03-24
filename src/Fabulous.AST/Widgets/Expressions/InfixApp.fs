namespace Fabulous.AST

open Fabulous.AST
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module InfixApp =
    let LeftHandSide = Attributes.defineWidget "LeftHandSide"
    let Operator = Attributes.defineScalar<string> "Operator"
    let RightHandSide = Attributes.defineWidget "RightHandSide"

    let WidgetKey =
        Widgets.register "InfixApp" (fun widget ->
            let lhs = Widgets.getNodeFromWidget widget LeftHandSide
            let operator = Widgets.getScalarValue widget Operator
            let rhs = Widgets.getNodeFromWidget widget RightHandSide
            Expr.InfixApp(ExprInfixAppNode(lhs, SingleTextNode.Create(operator), rhs, Range.Zero)))

[<AutoOpen>]
module InfixAppBuilders =
    type Ast with
        /// <summary>
        /// Create an infix application expression with two expressions connected by an operator.
        /// </summary>
        /// <param name="lhs">The left-hand side expression.</param>
        /// <param name="operator">The infix operator.</param>
        /// <param name="rhs">The right-hand side expression.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         InfixAppExpr(ConstantExpr(Int(1)), "+", ConstantExpr(Int(2)))
        ///     }
        /// }
        /// </code>
        static member InfixAppExpr(lhs: WidgetBuilder<Expr>, operator: string, rhs: WidgetBuilder<Expr>) =
            WidgetBuilder<Expr>(
                InfixApp.WidgetKey,
                AttributesBundle(
                    StackList.one(InfixApp.Operator.WithValue(operator)),
                    [| InfixApp.LeftHandSide.WithValue(lhs.Compile())
                       InfixApp.RightHandSide.WithValue(rhs.Compile()) |],
                    Array.empty
                )
            )

        /// <summary>
        /// Create an infix application expression with a constant left-hand side.
        /// </summary>
        /// <param name="lhs">The left-hand side constant.</param>
        /// <param name="operator">The infix operator.</param>
        /// <param name="rhs">The right-hand side expression.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         InfixAppExpr(Int(1), "+", ConstantExpr(Int(2)))
        ///     }
        /// }
        /// </code>
        static member InfixAppExpr(lhs: WidgetBuilder<Constant>, operator: string, rhs: WidgetBuilder<Expr>) =
            Ast.InfixAppExpr(Ast.ConstantExpr(lhs), operator, rhs)

        /// <summary>
        /// Create an infix application expression with a string literal left-hand side.
        /// </summary>
        /// <param name="lhs">The left-hand side string literal.</param>
        /// <param name="operator">The infix operator.</param>
        /// <param name="rhs">The right-hand side expression.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         InfixAppExpr("value", "+", ConstantExpr(Int(2)))
        ///     }
        /// }
        /// </code>
        static member InfixAppExpr(lhs: string, operator: string, rhs: WidgetBuilder<Expr>) =
            Ast.InfixAppExpr(Ast.Constant(lhs), operator, rhs)

        /// <summary>
        /// Create an infix application expression with constant operands.
        /// </summary>
        /// <param name="lhs">The left-hand side constant.</param>
        /// <param name="operator">The infix operator.</param>
        /// <param name="rhs">The right-hand side constant.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         InfixAppExpr(Int(1), "+", Int(2))
        ///     }
        /// }
        /// </code>
        static member InfixAppExpr(lhs: WidgetBuilder<Constant>, operator: string, rhs: WidgetBuilder<Constant>) =
            Ast.InfixAppExpr(lhs, operator, Ast.ConstantExpr(rhs))

        /// <summary>
        /// Create an infix application expression with a string literal left-hand side and constant right-hand side.
        /// </summary>
        /// <param name="lhs">The left-hand side string literal.</param>
        /// <param name="operator">The infix operator.</param>
        /// <param name="rhs">The right-hand side constant.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         InfixAppExpr("value", "+", Int(2))
        ///     }
        /// }
        /// </code>
        static member InfixAppExpr(lhs: string, operator: string, rhs: WidgetBuilder<Constant>) =
            Ast.InfixAppExpr(Ast.Constant(lhs), operator, rhs)

        /// <summary>
        /// Create an infix application expression with string literals for both operands.
        /// </summary>
        /// <param name="lhs">The left-hand side string literal.</param>
        /// <param name="operator">The infix operator.</param>
        /// <param name="rhs">The right-hand side string literal.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         InfixAppExpr("1", "+", "2")
        ///     }
        /// }
        /// </code>
        static member InfixAppExpr(lhs: string, operator: string, rhs: string) =
            Ast.InfixAppExpr(lhs, operator, Ast.Constant(rhs))

        /// <summary>
        /// Create an infix application expression with an expression left-hand side and constant right-hand side.
        /// </summary>
        /// <param name="lhs">The left-hand side expression.</param>
        /// <param name="operator">The infix operator.</param>
        /// <param name="rhs">The right-hand side constant.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         InfixAppExpr(ConstantExpr(Int(1)), "+", Int(2))
        ///     }
        /// }
        /// </code>
        static member InfixAppExpr(lhs: WidgetBuilder<Expr>, operator: string, rhs: WidgetBuilder<Constant>) =
            Ast.InfixAppExpr(lhs, operator, Ast.ConstantExpr(rhs))

        /// <summary>
        /// Create an infix application expression with a constant left-hand side and string literal right-hand side.
        /// </summary>
        /// <param name="lhs">The left-hand side constant.</param>
        /// <param name="operator">The infix operator.</param>
        /// <param name="rhs">The right-hand side string literal.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         InfixAppExpr(Int(1), "+", "2")
        ///     }
        /// }
        /// </code>
        static member InfixAppExpr(lhs: WidgetBuilder<Constant>, operator: string, rhs: string) =
            Ast.InfixAppExpr(Ast.ConstantExpr(lhs), operator, Ast.Constant(rhs))

        /// <summary>
        /// Create a pipe-right (|>) expression.
        /// </summary>
        /// <param name="lhs">The left-hand side expression.</param>
        /// <param name="rhs">The right-hand side expression.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         PipeRightExpr(ConstantExpr(Int(1)), LambdaExpr([], ConstantExpr(Int(2))))
        ///     }
        /// }
        /// </code>
        static member PipeRightExpr(lhs: WidgetBuilder<Expr>, rhs: WidgetBuilder<Expr>) =
            Ast.InfixAppExpr(lhs, "|>", rhs)

        /// <summary>
        /// Create a pipe-right (|>) expression with a constant left-hand side.
        /// </summary>
        /// <param name="lhs">The left-hand side constant.</param>
        /// <param name="rhs">The right-hand side expression.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         PipeRightExpr(Int(1), LambdaExpr([], ConstantExpr(Int(2))))
        ///     }
        /// }
        /// </code>
        static member PipeRightExpr(lhs: WidgetBuilder<Constant>, rhs: WidgetBuilder<Expr>) =
            Ast.InfixAppExpr(lhs, "|>", rhs)

        /// <summary>
        /// Create a pipe-right (|>) expression with a string literal left-hand side.
        /// </summary>
        /// <param name="lhs">The left-hand side string literal.</param>
        /// <param name="rhs">The right-hand side expression.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         PipeRightExpr("1", LambdaExpr([], ConstantExpr(Int(2))))
        ///     }
        /// }
        /// </code>
        static member PipeRightExpr(lhs: string, rhs: WidgetBuilder<Expr>) = Ast.InfixAppExpr(lhs, "|>", rhs)

        /// <summary>
        /// Create a pipe-right (|>) expression with an expression left-hand side and constant right-hand side.
        /// </summary>
        /// <param name="lhs">The left-hand side expression.</param>
        /// <param name="rhs">The right-hand side constant.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         PipeRightExpr(ConstantExpr(Int(1)), Int(2))
        ///     }
        /// }
        /// </code>
        static member PipeRightExpr(lhs: WidgetBuilder<Expr>, rhs: WidgetBuilder<Constant>) =
            Ast.InfixAppExpr(lhs, "|>", rhs)

        /// <summary>
        /// Create a pipe-right (|>) expression with constant operands.
        /// </summary>
        /// <param name="lhs">The left-hand side constant.</param>
        /// <param name="rhs">The right-hand side constant.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         PipeRightExpr(Int(1), Int(2))
        ///     }
        /// }
        /// </code>
        static member PipeRightExpr(lhs: WidgetBuilder<Constant>, rhs: WidgetBuilder<Constant>) =
            Ast.InfixAppExpr(lhs, "|>", rhs)

        /// <summary>
        /// Create a pipe-right (|>) expression with a constant left-hand side and string literal right-hand side.
        /// </summary>
        /// <param name="lhs">The left-hand side constant.</param>
        /// <param name="rhs">The right-hand side string literal.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         PipeRightExpr(Int(1), "functionName")
        ///     }
        /// }
        /// </code>
        static member PipeRightExpr(lhs: WidgetBuilder<Constant>, rhs: string) = Ast.InfixAppExpr(lhs, "|>", rhs)

        /// <summary>
        /// Create a pipe-right (|>) expression with string literals for both operands.
        /// </summary>
        /// <param name="lhs">The left-hand side string literal.</param>
        /// <param name="rhs">The right-hand side string literal.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         PipeRightExpr("1", "functionName")
        ///     }
        /// }
        /// </code>
        static member PipeRightExpr(lhs: string, rhs: string) = Ast.InfixAppExpr(lhs, "|>", rhs)

        /// <summary>
        /// Create a pipe-left (&lt;|) expression.
        /// </summary>
        /// <param name="lhs">The left-hand side expression.</param>
        /// <param name="rhs">The right-hand side expression.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         PipeLeftExpr(LambdaExpr([], ConstantExpr(Int(1))), ConstantExpr(Int(2)))
        ///     }
        /// }
        /// </code>
        static member PipeLeftExpr(lhs: WidgetBuilder<Expr>, rhs: WidgetBuilder<Expr>) =
            Ast.InfixAppExpr(lhs, "<|", rhs)

        /// <summary>
        /// Create a pipe-left (&lt;|) expression with a constant left-hand side.
        /// </summary>
        /// <param name="lhs">The left-hand side constant.</param>
        /// <param name="rhs">The right-hand side expression.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         PipeLeftExpr(Int(1), ConstantExpr(Int(2)))
        ///     }
        /// }
        /// </code>
        static member PipeLeftExpr(lhs: WidgetBuilder<Constant>, rhs: WidgetBuilder<Expr>) =
            Ast.InfixAppExpr(lhs, "<|", rhs)

        /// <summary>
        /// Create a pipe-left (&lt;|) expression with constant operands.
        /// </summary>
        /// <param name="lhs">The left-hand side constant.</param>
        /// <param name="rhs">The right-hand side constant.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         PipeLeftExpr(Int(1), Int(2))
        ///     }
        /// }
        /// </code>
        static member PipeLeftExpr(lhs: WidgetBuilder<Constant>, rhs: WidgetBuilder<Constant>) =
            Ast.InfixAppExpr(lhs, "<|", rhs)

        /// <summary>
        /// Create a pipe-left (&lt;|) expression with a string literal left-hand side.
        /// </summary>
        /// <param name="lhs">The left-hand side string literal.</param>
        /// <param name="rhs">The right-hand side expression.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         PipeLeftExpr("functionName", ConstantExpr(Int(2)))
        ///     }
        /// }
        /// </code>
        static member PipeLeftExpr(lhs: string, rhs: WidgetBuilder<Expr>) = Ast.InfixAppExpr(lhs, "<|", rhs)

        /// <summary>
        /// Create a pipe-left (&lt;|) expression with an expression left-hand side and constant right-hand side.
        /// </summary>
        /// <param name="lhs">The left-hand side expression.</param>
        /// <param name="rhs">The right-hand side constant.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         PipeLeftExpr(LambdaExpr([], ConstantExpr(Int(1))), Int(2))
        ///     }
        /// }
        /// </code>
        static member PipeLeftExpr(lhs: WidgetBuilder<Expr>, rhs: WidgetBuilder<Constant>) =
            Ast.InfixAppExpr(lhs, "<|", rhs)

        /// <summary>
        /// Create a pipe-left (&lt;|) expression with a constant left-hand side and string literal right-hand side.
        /// </summary>
        /// <param name="lhs">The left-hand side constant.</param>
        /// <param name="rhs">The right-hand side string literal.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         PipeLeftExpr(Int(1), "2")
        ///     }
        /// }
        /// </code>
        static member PipeLeftExpr(lhs: WidgetBuilder<Constant>, rhs: string) = Ast.InfixAppExpr(lhs, "<|", rhs)

        /// <summary>
        /// Create a pipe-left (&lt;|) expression with string literals for both operands.
        /// </summary>
        /// <param name="lhs">The left-hand side string literal.</param>
        /// <param name="rhs">The right-hand side string literal.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         PipeLeftExpr("functionName", "2")
        ///     }
        /// }
        /// </code>
        static member PipeLeftExpr(lhs: string, rhs: string) = Ast.InfixAppExpr(lhs, "<|", rhs)
