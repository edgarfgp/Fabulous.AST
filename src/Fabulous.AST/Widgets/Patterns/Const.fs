namespace Fabulous.AST

open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak

module Const =
    let Value = Attributes.defineScalar<Constant> "Value"

    let WidgetKey =
        Widgets.register "Const" (fun widget ->
            let value = Widgets.getScalarValue widget Value
            Pattern.Const(value))

[<AutoOpen>]
module ConstBuilders =
    type Ast with

        static member ConstantPat(value: Constant) =
            WidgetBuilder<Pattern>(
                Const.WidgetKey,
                AttributesBundle(StackList.one(Const.Value.WithValue(value)), Array.empty, Array.empty)
            )

        static member ConstantPat(value: string) =
            Ast.ConstantPat(Constant.FromText(SingleTextNode.Create(value)))
