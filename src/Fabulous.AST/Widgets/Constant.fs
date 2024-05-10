namespace Fabulous.AST

open System
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module Constant =
    let Value = Attributes.defineScalar<Constant> "Value"

    let WidgetKey =
        Widgets.register "Constant" (fun widget ->
            let value = Widgets.getScalarValue widget Value
            value)

[<AutoOpen>]
module ConstantBuilders =
    type Ast with
        static member private BaseConstant(value: string) =
            WidgetBuilder<Constant>(
                Constant.WidgetKey,
                AttributesBundle(
                    StackList.one(Constant.Value.WithValue(Constant.FromText(SingleTextNode.Create(value)))),
                    Array.empty,
                    Array.empty
                )
            )

        static member Bool(value: bool) = Ast.BaseConstant($"{value}".ToLower())

        static member Byte(value: byte) = Ast.BaseConstant($"{value}uy")

        static member SByte(value: sbyte) = Ast.BaseConstant($"{value}y")

        static member Int16(value: int16) = Ast.BaseConstant($"{value}s")

        static member UInt16(value: uint16) = Ast.BaseConstant($"{value}us")

        static member Int(value: int) = Ast.BaseConstant($"{value}")

        static member UInt32(value: uint32) = Ast.BaseConstant($"{value}u")

        static member Int64(value: int64) = Ast.BaseConstant($"{value}L")

        static member UInt64(value: uint64) = Ast.BaseConstant($"{value}UL")

        static member IntPtr(value: IntPtr) = Ast.BaseConstant($"nativeint {value}")

        static member UIntPtr(value: UIntPtr) = Ast.BaseConstant($"unativeint {value}")

        static member Decimal(value: decimal) = Ast.BaseConstant($"{value}m")

        static member Float(value: float) =
            Ast.BaseConstant($"""{value.ToString("F1")}""")

        static member Double(value: double) = Ast.BaseConstant($"{value}")

        static member Float32(value: float32) =
            Ast.BaseConstant($"""{value.ToString("F1")}f""")

        static member Single(value: single) =
            Ast.BaseConstant($"""{value.ToString("F1")}f""")

        static member Char(value: char) = Ast.BaseConstant($"'{value}'")

        static member String(value: string) = Ast.BaseConstant($"\"{value}\"")

        static member Constant(value: string) = Ast.BaseConstant(value)

        static member Backticks(value: string) = Ast.BaseConstant($"```{value}```")

        static member Backticks(value: WidgetBuilder<Constant>) =
            let value =
                match Gen.mkOak value with
                | Constant.FromText node -> Constant.FromText(SingleTextNode.Create($"```{node.Text}```"))
                | Constant.Unit _ as unit -> unit
                | Constant.Measure _ as measure -> measure

            WidgetBuilder<Constant>(
                Constant.WidgetKey,
                AttributesBundle(StackList.one(Constant.Value.WithValue(value)), Array.empty, Array.empty)
            )

        static member String(value: WidgetBuilder<Constant>) =
            let value =
                match Gen.mkOak value with
                | Constant.FromText singleTextNode ->
                    Constant.FromText(SingleTextNode.Create($"\"{singleTextNode.Text}\""))
                | Constant.Unit _ as unit -> unit
                | Constant.Measure _ as measure -> measure

            WidgetBuilder<Constant>(
                Constant.WidgetKey,
                AttributesBundle(StackList.one(Constant.Value.WithValue(value)), Array.empty, Array.empty)
            )

        static member RawString(value: string) =
            Ast.BaseConstant($"\"\"\"{value}\"\"\"")

        static member RawString(value: WidgetBuilder<Constant>) =
            let value =
                match Gen.mkOak value with
                | Constant.FromText node -> Constant.FromText(SingleTextNode.Create($"\"\"\"{node.Text}\"\"\""))
                | Constant.Unit _ as unit -> unit
                | Constant.Measure _ as measure -> measure

            WidgetBuilder<Constant>(
                Constant.WidgetKey,
                AttributesBundle(StackList.one(Constant.Value.WithValue(value)), Array.empty, Array.empty)
            )

module ConstantUnit =
    let WidgetKey =
        Widgets.register "ConstantUnit" (fun _ ->
            Constant.Unit(UnitNode(SingleTextNode.leftParenthesis, SingleTextNode.rightParenthesis, Range.Zero)))

[<AutoOpen>]
module ConstantUnitBuilders =
    type Ast with
        static member ConstantUnit() =
            WidgetBuilder<Constant>(
                ConstantUnit.WidgetKey,
                AttributesBundle(StackList.empty(), Array.empty, Array.empty)
            )

module ConstantMeasure =
    let Value = Attributes.defineWidget "Value"
    let Measure = Attributes.defineWidget "Measure"

    let WidgetKey =
        Widgets.register "ConstantMeasure" (fun widget ->
            let value = Widgets.getNodeFromWidget widget Value
            let measure = Widgets.getNodeFromWidget<Measure> widget Measure

            Constant.Measure(
                ConstantMeasureNode(
                    value,
                    UnitOfMeasureNode(SingleTextNode.lessThan, measure, SingleTextNode.greaterThan, Range.Zero),
                    Range.Zero
                )
            ))

[<AutoOpen>]
module ConstantMeasureBuilders =
    type Ast with
        static member ConstantMeasure(constant: WidgetBuilder<Constant>, measure: WidgetBuilder<Measure>) =
            WidgetBuilder<Constant>(
                ConstantMeasure.WidgetKey,
                AttributesBundle(
                    StackList.empty(),
                    [| ConstantMeasure.Measure.WithValue(measure.Compile())
                       ConstantMeasure.Value.WithValue(constant.Compile()) |],
                    Array.empty
                )
            )

        static member ConstantMeasure(constant: WidgetBuilder<Constant>, measure: string) =
            Ast.ConstantMeasure(constant, Ast.MeasureSingle(measure))

        static member ConstantMeasure(constant: string, measure: WidgetBuilder<Measure>) =
            Ast.ConstantMeasure(Ast.Constant(constant), measure)

        static member ConstantMeasure(constant: string, measure: string) =
            Ast.ConstantMeasure(Ast.Constant(constant), Ast.MeasureSingle(measure))
