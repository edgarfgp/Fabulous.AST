namespace Fabulous.AST

open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module OptVar =
    let Identifier = Attributes.defineScalar<string> "Identifier"

    let IsOptional = Attributes.defineScalar<bool> "IsOptional"

    let WidgetKey =
        Widgets.register "Lazy" (fun widget ->
            let identifier = Widgets.getScalarValue widget Identifier
            let isOptional = Widgets.getScalarValue widget IsOptional

            Expr.OptVar(
                ExprOptVarNode(
                    isOptional,
                    IdentListNode([ IdentifierOrDot.Ident(SingleTextNode.Create(identifier)) ], Range.Zero),
                    Range.Zero
                )
            ))

[<AutoOpen>]
module OptVarBuilders =
    type Ast with
        static member OptVarExpr(value: string) =
            WidgetBuilder<Expr>(
                OptVar.WidgetKey,
                AttributesBundle(
                    StackList.two(OptVar.Identifier.WithValue(value), OptVar.IsOptional.WithValue(false)),
                    Array.empty,
                    Array.empty
                )
            )

        static member OptVarExpr(value: string, isOptional: bool) =
            WidgetBuilder<Expr>(
                OptVar.WidgetKey,
                AttributesBundle(
                    StackList.two(OptVar.Identifier.WithValue(value), OptVar.IsOptional.WithValue(isOptional)),
                    Array.empty,
                    Array.empty
                )
            )
