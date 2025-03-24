namespace Fabulous.AST

open System
open System.Text
open Fabulous.AST
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
                Constant.Value.WithValue(Constant.FromText(SingleTextNode.Create(value)))
            )

        /// <summary>Creates a boolean constant.</summary>
        /// <param name="value">The boolean value.</param>
        static member Bool(value: bool) = Ast.BaseConstant($"{value}".ToLower())

        /// <summary>Creates a byte constant.</summary>
        /// <param name="value">The byte value.</param>
        static member Byte(value: byte) = Ast.BaseConstant($"{value}uy")

        /// <summary>Creates a signed byte constant.</summary>
        /// <param name="value">The signed byte value.</param>
        static member SByte(value: sbyte) = Ast.BaseConstant($"{value}y")

        /// <summary>Creates a 16-bit integer constant.</summary>
        /// <param name="value">The 16-bit integer value.</param>
        static member Int16(value: int16) = Ast.BaseConstant($"{value}s")

        /// <summary>Creates an unsigned 16-bit integer constant.</summary>
        /// <param name="value">The unsigned 16-bit integer value.</param>
        static member UInt16(value: uint16) = Ast.BaseConstant($"{value}us")

        /// <summary>Creates a 32-bit integer constant.</summary>
        /// <param name="value">The 32-bit integer value.</param>
        static member Int(value: int) = Ast.BaseConstant($"{value}")

        /// <summary>Creates an unsigned 32-bit integer constant.</summary>
        /// <param name="value">The unsigned 32-bit integer value.</param>
        static member UInt32(value: uint32) = Ast.BaseConstant($"{value}u")

        /// <summary>Creates a 64-bit integer constant.</summary>
        /// <param name="value">The 64-bit integer value.</param>
        static member Int64(value: int64) = Ast.BaseConstant($"{value}L")

        /// <summary>Creates an unsigned 64-bit integer constant.</summary>
        /// <param name="value">The unsigned 64-bit integer value.</param>
        static member UInt64(value: uint64) = Ast.BaseConstant($"{value}UL")

        /// <summary>Creates a native integer constant.</summary>
        /// <param name="value">The native integer value.</param>
        static member IntPtr(value: IntPtr) = Ast.BaseConstant($"nativeint {value}")

        /// <summary>Creates an unsigned native integer constant.</summary>
        /// <param name="value">The unsigned native integer value.</param>
        static member UIntPtr(value: UIntPtr) = Ast.BaseConstant($"unativeint {value}")

        /// <summary>Creates a decimal constant.</summary>
        /// <param name="value">The decimal value.</param>
        static member Decimal(value: decimal) = Ast.BaseConstant($"{value}m")

        /// <summary>Creates a floating-point constant.</summary>
        /// <param name="value">The floating-point value.</param>
        static member Float(value: float) =
            Ast.BaseConstant($"""{value.ToString("F1")}""")

        /// <summary>Creates a double-precision floating-point constant.</summary>
        /// <param name="value">The double-precision floating-point value.</param>
        static member Double(value: double) = Ast.BaseConstant($"{value}")

        /// <summary>Creates a 32-bit floating-point constant.</summary>
        /// <param name="value">The 32-bit floating-point value.</param>
        static member Float32(value: float32) =
            Ast.BaseConstant($"""{value.ToString("F1")}f""")

        /// <summary>Creates a single-precision floating-point constant.</summary>
        /// <param name="value">The single-precision floating-point value.</param>
        static member Single(value: single) =
            Ast.BaseConstant($"""{value.ToString("F1")}f""")

        /// <summary>Creates a character constant.</summary>
        /// <param name="value">The character value.</param>
        static member Char(value: char) = Ast.BaseConstant($"'{value}'")

        /// <summary>Creates a string constant with proper escaping.</summary>
        /// <param name="value">The string value.</param>
        static member String(value: string) = Ast.BaseConstant(String.escape value)

        /// <summary>Creates a custom constant with the specified value.</summary>
        /// <param name="value">The constant value as a string.</param>
        static member Constant(value: string) = Ast.BaseConstant(value)

        /// <summary>Creates a backtick-quoted identifier constant.</summary>
        /// <param name="value">The identifier value.</param>
        static member Backticks(value: string) = Ast.BaseConstant($"```{value}```")

        /// <summary>Creates a backtick-quoted identifier constant from another constant.</summary>
        /// <param name="value">The constant value to wrap in backticks.</param>
        static member Backticks(value: WidgetBuilder<Constant>) =
            let value =
                match Gen.mkOak value with
                | Constant.FromText node -> Constant.FromText(SingleTextNode.Create($"```{node.Text}```"))
                | Constant.Unit _ as unit -> unit
                | Constant.Measure _ as measure -> measure

            WidgetBuilder<Constant>(Constant.WidgetKey, Constant.Value.WithValue(value))

        /// <summary>Creates a string constant from another constant.</summary>
        /// <param name="value">The constant value to wrap as a string.</param>
        static member String(value: WidgetBuilder<Constant>) =
            let value =
                match Gen.mkOak value with
                | Constant.FromText singleTextNode ->
                    Constant.FromText(SingleTextNode.Create($"\"{singleTextNode.Text}\""))
                | Constant.Unit _ as unit -> unit
                | Constant.Measure _ as measure -> measure

            WidgetBuilder<Constant>(Constant.WidgetKey, Constant.Value.WithValue(value))

        /// <summary>Creates a raw (verbatim) string constant.</summary>
        /// <param name="value">The raw string value.</param>
        static member VerbatimString(value: string) =
            Ast.BaseConstant($"\"\"\"{value}\"\"\"")

        /// <summary>Creates a raw (verbatim) string constant from another constant.</summary>
        /// <param name="value">The constant value to wrap as a raw string.</param>
        static member VerbatimString(value: WidgetBuilder<Constant>) =
            let value =
                match Gen.mkOak value with
                | Constant.FromText node -> Constant.FromText(SingleTextNode.Create($"\"\"\"{node.Text}\"\"\""))
                | Constant.Unit _ as unit -> unit
                | Constant.Measure _ as measure -> measure

            WidgetBuilder<Constant>(Constant.WidgetKey, Constant.Value.WithValue(value))

module ConstantUnit =
    let WidgetKey =
        Widgets.register "ConstantUnit" (fun _ ->
            Constant.Unit(UnitNode(SingleTextNode.leftParenthesis, SingleTextNode.rightParenthesis, Range.Zero)))

[<AutoOpen>]
module ConstantUnitBuilders =
    type Ast with
        /// <summary>Creates a unit constant represented as ().</summary>
        static member ConstantUnit() =
            WidgetBuilder<Constant>(ConstantUnit.WidgetKey)

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
        /// <summary>Creates a constant with a unit of measure.</summary>
        /// <param name="constant">The constant value.</param>
        /// <param name="measure">The unit of measure.</param>
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

        /// <summary>Creates a constant with a simple string unit of measure.</summary>
        /// <param name="constant">The constant value.</param>
        /// <param name="measure">The unit of measure as a string.</param>
        static member ConstantMeasure(constant: WidgetBuilder<Constant>, measure: string) =
            Ast.ConstantMeasure(constant, Ast.MeasureSingle(measure))

        /// <summary>Creates a constant from a string with a unit of measure.</summary>
        /// <param name="constant">The constant value as a string.</param>
        /// <param name="measure">The unit of measure.</param>
        static member ConstantMeasure(constant: string, measure: WidgetBuilder<Measure>) =
            Ast.ConstantMeasure(Ast.Constant(constant), measure)

        /// <summary>Creates a constant from a string with a simple string unit of measure.</summary>
        /// <param name="constant">The constant value as a string.</param>
        /// <param name="measure">The unit of measure as a string.</param>
        static member ConstantMeasure(constant: string, measure: string) =
            Ast.ConstantMeasure(Ast.Constant(constant), Ast.MeasureSingle(measure))
