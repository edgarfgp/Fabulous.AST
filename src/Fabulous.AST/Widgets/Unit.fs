namespace Fabulous.AST

open FSharp.Compiler.Text
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak

module Unit =
    let WidgetKey = Widgets.register "Unit" (fun _ ->
        UnitNode(
            SingleTextNode("(", Range.Zero),
            SingleTextNode(")", Range.Zero),
            Range.Zero
        )
    )
    
[<AutoOpen>]
module UnitBuilders =
    type Fabulous.AST.Ast with
        static member inline Unit() =
            WidgetBuilder<UnitNode>(
                Unit.WidgetKey,
                AttributesBundle(
                    StackList.empty(),
                    ValueNone,
                    ValueNone
                )
            )