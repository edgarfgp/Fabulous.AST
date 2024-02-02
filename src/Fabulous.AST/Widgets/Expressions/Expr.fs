namespace Fabulous.AST

open System.Linq.Expressions
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

[<AutoOpen>]
module ExpressionsHelpers =

    type Expr with

        static member CreatConstant(value: string) =
            Expr.Constant(Constant.FromText(SingleTextNode.Create(value)))

module Expressions =
    let Value = Attributes.defineScalar<string> "Value"

    let WidgetKey =
        Widgets.register "Parameters" (fun widget ->
            let value = Helpers.getScalarValue widget Value
            Expr.Constant(Constant.FromText(SingleTextNode.Create(value))))

[<AutoOpen>]
module ExpressionsBuilders =
    type Ast with

        static member inline Constant(value: string) =
            WidgetBuilder<Expr>(Expressions.WidgetKey, Expressions.Value.WithValue(value))
