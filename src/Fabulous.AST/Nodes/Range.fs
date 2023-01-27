namespace Fabulous.AST

open FSharp.Compiler.Text

type IFabRange = inherit IFabValueBase

module Range =
    let FileName = Attributes.defineScalar<string> "FileName"
    let Start = Attributes.defineWidget "Start"
    let End = Attributes.defineWidget "End"
    
    let WidgetKey = Widgets.register(fun (widget: Widget) ->
        let fileName = Helpers.getScalarValue widget FileName
        let line = Helpers.getWidgetValue widget Start |> Helpers.createValueForWidget
        let column = Helpers.getWidgetValue widget End |> Helpers.createValueForWidget
        Range.mkRange fileName line column
    )
    
[<AutoOpen>]
module RangeBuilders =
    type Fabulous.AST.Node with
        static member inline Range(fileName: string, start: WidgetBuilder<IFabPosition>, ``end``: WidgetBuilder<IFabPosition>) =
            WidgetBuilder<IFabRange>(
                Range.WidgetKey,
                AttributesBundle(
                    StackAllocatedCollections.StackList.StackList.one(Range.FileName.WithValue(fileName)),
                    ValueSome [|
                        Range.Start.WithValue(start.Compile())
                        Range.End.WithValue(``end``.Compile())
                    |],
                    ValueNone
                )
            )