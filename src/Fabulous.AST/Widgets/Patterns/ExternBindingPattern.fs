namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fabulous.AST
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module ExternBindingPattern =
    let PatternVal = Attributes.defineWidget "PatternVal"

    let MultipleAttributes =
        Attributes.defineScalar<AttributeNode seq> "MultipleAttributes"

    let TypeValue = Attributes.defineWidget "Type"

    let WidgetKey =
        Widgets.register "ExternBindingPattern" (fun widget ->
            let pat =
                Widgets.tryGetNodeFromWidget widget PatternVal
                |> ValueOption.map Some
                |> ValueOption.defaultValue None

            let attributes =
                Widgets.tryGetScalarValue widget MultipleAttributes
                |> ValueOption.map(fun x -> Some(MultipleAttributeListNode.Create(x)))
                |> ValueOption.defaultValue None

            let tp =
                Widgets.tryGetNodeFromWidget widget TypeValue
                |> ValueOption.map Some
                |> ValueOption.defaultValue None

            ExternBindingPatternNode(attributes, tp, pat, Range.Zero))

[<AutoOpen>]
module ExternBindingPatternNodeBuilders =
    type Ast with
        static member ExternBindingPat(value: WidgetBuilder<Type>, pat: WidgetBuilder<Pattern>) =
            WidgetBuilder<ExternBindingPatternNode>(
                ExternBindingPattern.WidgetKey,
                AttributesBundle(
                    StackList.empty(),
                    [| ExternBindingPattern.PatternVal.WithValue(pat.Compile())
                       ExternBindingPattern.TypeValue.WithValue(value.Compile()) |],
                    Array.empty
                )
            )

        static member ExternBindingPat(value: string, pat: WidgetBuilder<Pattern>) =
            Ast.ExternBindingPat(Ast.LongIdent(value), pat)

        static member ExternBindingPat(value: string, pat: WidgetBuilder<Constant>) =
            Ast.ExternBindingPat(value, Ast.ConstantPat(pat))

        static member ExternBindingPat(value: WidgetBuilder<Type>, pat: string) =
            Ast.ExternBindingPat(value, Ast.ConstantPat(pat))

        static member ExternBindingPat(value: string, pat: string) =
            Ast.ExternBindingPat(Ast.LongIdent(value), Ast.ConstantPat(pat))

type ExternBindingPatternNodeModifiers =
    [<Extension>]
    static member inline attributes
        (this: WidgetBuilder<ExternBindingPatternNode>, attributes: WidgetBuilder<AttributeNode> seq)
        =
        this.AddScalar(ExternBindingPattern.MultipleAttributes.WithValue(attributes |> Seq.map Gen.mkOak))

    [<Extension>]
    static member inline attribute
        (this: WidgetBuilder<ExternBindingPatternNode>, attribute: WidgetBuilder<AttributeNode>)
        =
        ExternBindingPatternNodeModifiers.attributes(this, [ attribute ])
