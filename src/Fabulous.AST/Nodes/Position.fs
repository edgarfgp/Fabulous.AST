namespace Fabulous.AST

open FSharp.Compiler.Text

type IFabPosition = inherit IFabValueBase

module Position =
    let Line = Attributes.defineScalar<int> "Line"
    let Column = Attributes.defineScalar<int> "Column"
    
    let WidgetKey = Widgets.register(fun (widget: Widget) ->
        let line = Helpers.getScalarValue widget Line
        let column = Helpers.getScalarValue widget Column
        Position.mkPos line column
    )
    
[<AutoOpen>]
module PositionBuilders =
    type Fabulous.AST.Node with
        static member inline Position(line, column) =
            WidgetBuilder<IFabPosition>(
                Position.WidgetKey,
                Position.Line.WithValue(line),
                Position.Column.WithValue(column)
            )