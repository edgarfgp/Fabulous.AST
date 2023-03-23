namespace Fabulous.AST

open System.Runtime.CompilerServices
open FSharp.Compiler.Text
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak

open type Fabulous.AST.Ast

module Unit =
    let WidgetKey =
        Widgets.register "Unit" (fun _ ->
            UnitNode(SingleTextNode("(", Range.Zero), SingleTextNode(")", Range.Zero), Range.Zero))

[<AutoOpen>]
module UnitBuilders =
    type Fabulous.AST.Ast with

        static member inline Unit() =
            WidgetBuilder<UnitNode>(Unit.WidgetKey, AttributesBundle(StackList.empty (), ValueNone, ValueNone))
