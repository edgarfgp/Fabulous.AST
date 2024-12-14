namespace Fabulous.AST

open Fabulous.AST
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module NamedIndexedPropertySet =
    let Identifier = Attributes.defineScalar<string> "SingleNode"

    let IndexExpr = Attributes.defineWidget "Identifier"

    let ValueExpr = Attributes.defineWidget "Value"

    let WidgetKey =
        Widgets.register "NamedIndexedPropertySet" (fun widget ->
            let identifier = Widgets.getScalarValue widget Identifier
            let indexExpr = Widgets.getNodeFromWidget widget IndexExpr
            let valueExpr = Widgets.getNodeFromWidget widget ValueExpr

            Expr.NamedIndexedPropertySet(
                ExprNamedIndexedPropertySetNode(
                    IdentListNode([ IdentifierOrDot.Ident(SingleTextNode.Create(identifier)) ], Range.Zero),
                    indexExpr,
                    valueExpr,
                    Range.Zero
                )
            ))

[<AutoOpen>]
module NamedIndexedPropertySetBuilders =
    type Ast with

        static member NamedIndexedPropertySetExpr
            (identifier: string, indexExpr: WidgetBuilder<Expr>, valueExpr: WidgetBuilder<Expr>)
            =
            WidgetBuilder<Expr>(
                NamedIndexedPropertySet.WidgetKey,
                AttributesBundle(
                    StackList.one(NamedIndexedPropertySet.Identifier.WithValue(identifier)),
                    [| NamedIndexedPropertySet.IndexExpr.WithValue(indexExpr.Compile())
                       NamedIndexedPropertySet.ValueExpr.WithValue(valueExpr.Compile()) |],
                    Array.empty
                )
            )

        static member NamedIndexedPropertySetExpr
            (identifier: string, indexExpr: WidgetBuilder<Constant>, valueExpr: WidgetBuilder<Expr>)
            =
            Ast.NamedIndexedPropertySetExpr(identifier, Ast.ConstantExpr indexExpr, valueExpr)

        static member NamedIndexedPropertySetExpr
            (identifier: string, indexExpr: WidgetBuilder<Expr>, valueExpr: WidgetBuilder<Constant>)
            =
            Ast.NamedIndexedPropertySetExpr(identifier, indexExpr, Ast.ConstantExpr valueExpr)

        static member NamedIndexedPropertySetExpr
            (identifier: string, indexExpr: WidgetBuilder<Constant>, valueExpr: WidgetBuilder<Constant>)
            =
            Ast.NamedIndexedPropertySetExpr(identifier, Ast.ConstantExpr indexExpr, Ast.ConstantExpr valueExpr)

        static member NamedIndexedPropertySetExpr
            (identifier: string, indexExpr: WidgetBuilder<Expr>, valueExpr: string)
            =
            Ast.NamedIndexedPropertySetExpr(identifier, indexExpr, Ast.ConstantExpr valueExpr)

        static member NamedIndexedPropertySetExpr
            (identifier: string, indexExpr: string, valueExpr: WidgetBuilder<Expr>)
            =
            Ast.NamedIndexedPropertySetExpr(identifier, Ast.ConstantExpr indexExpr, valueExpr)

        static member NamedIndexedPropertySetExpr(identifier: string, indexExpr: string, valueExpr: string) =
            Ast.NamedIndexedPropertySetExpr(identifier, Ast.ConstantExpr indexExpr, Ast.ConstantExpr valueExpr)

        static member NamedIndexedPropertySetExpr
            (objExpr: string, indexExpr: string, valueExpr: WidgetBuilder<Constant>)
            =
            Ast.NamedIndexedPropertySetExpr(objExpr, Ast.Constant indexExpr, valueExpr)

        static member NamedIndexedPropertySetExpr
            (objExpr: string, indexExpr: WidgetBuilder<Constant>, valueExpr: string)
            =
            Ast.NamedIndexedPropertySetExpr(objExpr, indexExpr, Ast.Constant valueExpr)
