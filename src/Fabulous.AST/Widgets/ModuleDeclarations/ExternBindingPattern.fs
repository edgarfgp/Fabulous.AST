namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fantomas.FCS.Text
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak

module ExternBindingPattern =
    let PatternVal = Attributes.defineWidget "DoExpression"

    let MultipleAttributes =
        Attributes.defineScalar<AttributeNode list> "MultipleAttributes"

    let TypeValue = Attributes.defineWidget "Type"

    let WidgetKey =
        Widgets.register "ExternBindingPattern" (fun widget ->
            let pat = Widgets.tryGetNodeFromWidget widget PatternVal

            let attributes = Widgets.tryGetScalarValue widget MultipleAttributes

            let tp = Widgets.tryGetNodeFromWidget widget TypeValue

            let tp =
                match tp with
                | ValueSome tp -> Some tp
                | ValueNone -> None

            let multipleAttributes =
                match attributes with
                | ValueSome values ->
                    Some(
                        MultipleAttributeListNode(
                            [ AttributeListNode(
                                  SingleTextNode.leftAttribute,
                                  values,
                                  SingleTextNode.rightAttribute,
                                  Range.Zero
                              ) ],
                            Range.Zero
                        )
                    )
                | ValueNone -> None

            let pat =
                match pat with
                | ValueSome value -> Some value
                | ValueNone -> None

            ExternBindingPatternNode(multipleAttributes, tp, pat, Range.Zero))

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
        (this: WidgetBuilder<ExternBindingPatternNode>, attributes: WidgetBuilder<AttributeNode> list)
        =
        this.AddScalar(
            ExternBindingPattern.MultipleAttributes.WithValue(
                [ for attr in attributes do
                      Gen.mkOak attr ]
            )
        )

    [<Extension>]
    static member inline attribute
        (this: WidgetBuilder<ExternBindingPatternNode>, attribute: WidgetBuilder<AttributeNode>)
        =
        ExternBindingPatternNodeModifiers.attributes(this, [ attribute ])
