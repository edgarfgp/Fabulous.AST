namespace Fabulous.AST

open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module TypeApp =
    let IdentifierExpr = Attributes.defineWidget "Value"

    let TypeParameters = Attributes.defineScalar<Type list> "TypeParameters"

    let WidgetKey =
        Widgets.register "TypeApp" (fun widget ->
            let parameters = Widgets.getScalarValue widget TypeParameters
            let identifierExpr = Widgets.getNodeFromWidget widget IdentifierExpr

            Expr.TypeApp(
                ExprTypeAppNode(
                    identifierExpr,
                    SingleTextNode.lessThan,
                    parameters,
                    SingleTextNode.greaterThan,
                    Range.Zero
                )
            ))

[<AutoOpen>]
module TypeAppBuilders =
    type Ast with

        static member TypeAppExpr(value: WidgetBuilder<Expr>, parameters: WidgetBuilder<Type> list) =
            let parameters = parameters |> List.map Gen.mkOak

            WidgetBuilder<Expr>(
                TypeApp.WidgetKey,
                AttributesBundle(
                    StackList.one(TypeApp.TypeParameters.WithValue(parameters)),
                    [| TypeApp.IdentifierExpr.WithValue(value.Compile()) |],
                    Array.empty
                )
            )

        static member TypeAppExpr(value: WidgetBuilder<Constant>, parameters: WidgetBuilder<Type> list) =
            Ast.TypeAppExpr(Ast.ConstantExpr(value), parameters)

        static member TypeAppExpr(value: string, parameters: WidgetBuilder<Type> list) =
            Ast.TypeAppExpr(Ast.Constant(value), parameters)

        static member TypeAppExpr(value: string, parameters: string list) =
            let parameters = parameters |> List.map Ast.LongIdent
            Ast.TypeAppExpr(Ast.Constant(value), parameters)

        static member TypeAppExpr(value: WidgetBuilder<Expr>, parameter: WidgetBuilder<Type>) =
            Ast.TypeAppExpr(value, [ parameter ])

        static member TypeAppExpr(value: WidgetBuilder<Expr>, parameter: string) =
            Ast.TypeAppExpr(value, [ Ast.LongIdent(parameter) ])

        static member TypeAppExpr(value: WidgetBuilder<Constant>, parameter: WidgetBuilder<Type>) =
            Ast.TypeAppExpr(value, [ parameter ])

        static member TypeAppExpr(value: string, parameter: WidgetBuilder<Type>) = Ast.TypeAppExpr(value, [ parameter ])

        static member TypeAppExpr(value: string, parameter: string) = Ast.TypeAppExpr(value, [ parameter ])
