namespace Fabulous.AST

open FSharp.Compiler.Text
open Fantomas.Core.SyntaxOak

type IFabSingleText = inherit IFabNodeBase

module SingleText =
    let Text = Attributes.defineScalar "Text"
    
    let WidgetKey = Widgets.register "SingleText" (fun widget ->
        let text = Helpers.getScalarValue widget Text
        SingleTextNode(text, Range.Zero)
    )

[<AutoOpen>]
module SingleTextBuilders =
    type Fabulous.AST.Ast with
        static member inline SingleText(text: string) =
            WidgetBuilder<IFabSingleText>(
                SingleText.WidgetKey,
                SingleText.Text.WithValue(text)
            )