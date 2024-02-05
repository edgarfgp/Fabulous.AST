namespace Fabulous.AST

open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module New =
    let Value = Attributes.defineWidget "Value"

    let Type = Attributes.defineScalar<Type> "Type"

    let WidgetKey =
        Widgets.register "New" (fun widget ->
            let expr = Helpers.getNodeFromWidget<Expr> widget Value
            let typ = Helpers.getScalarValue widget Type
            Expr.New(ExprNewNode(SingleTextNode.``new``, typ, expr, Range.Zero)))

[<AutoOpen>]
module NewBuilders =
    type Ast with

        static member NewExpr(t: Type, value: WidgetBuilder<Expr>) =
            WidgetBuilder<Expr>(
                New.WidgetKey,
                AttributesBundle(
                    StackList.one(New.Type.WithValue(t)),
                    ValueSome [| New.Value.WithValue(value.Compile()) |],
                    ValueNone
                )
            )

        static member NewExpr(t: string, value: WidgetBuilder<Expr>) =
            Ast.NewExpr(CommonType.mkLongIdent(t), value)
