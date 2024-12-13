namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fabulous.AST
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module ExplicitConstructorMember =
    let XmlDocs = Attributes.defineScalar<string list> "XmlDoc"

    let MultipleAttributes =
        Attributes.defineScalar<AttributeNode list> "MultipleAttributes"

    let Accessibility = Attributes.defineScalar<AccessControl> "Accessibility"

    let Pat = Attributes.defineWidget "Pat"

    let Alias = Attributes.defineScalar<string> "Alias"

    let ExprValue = Attributes.defineWidget "ExprValue"

    let WidgetKey =
        Widgets.register "ExplicitConstructorMember" (fun widget ->
            let lines = Widgets.tryGetScalarValue widget XmlDocs

            let xmlDocs =
                match lines with
                | ValueSome values ->
                    let xmlDocNode = XmlDocNode.Create(values)
                    Some xmlDocNode
                | ValueNone -> None

            let attributes =
                Widgets.tryGetScalarValue widget MultipleAttributes
                |> ValueOption.map(fun x -> Some(MultipleAttributeListNode.Create(x)))
                |> ValueOption.defaultValue None

            let accessControl =
                Widgets.tryGetScalarValue widget Accessibility
                |> ValueOption.defaultValue AccessControl.Unknown

            let accessControl =
                match accessControl with
                | Public -> Some(SingleTextNode.``public``)
                | Private -> Some(SingleTextNode.``private``)
                | Internal -> Some(SingleTextNode.``internal``)
                | Unknown -> None

            let pat = Widgets.getNodeFromWidget widget Pat

            let alias =
                match Widgets.tryGetScalarValue widget Alias with
                | ValueSome value -> Some(SingleTextNode.Create value)
                | ValueNone -> None

            let expr = Widgets.getNodeFromWidget widget ExprValue

            MemberDefnExplicitCtorNode(
                xmlDocs,
                attributes,
                accessControl,
                SingleTextNode.``new``,
                pat,
                alias,
                SingleTextNode.equals,
                expr,
                Range.Zero
            ))

[<AutoOpen>]
module ExplicitConstructorBuilders =
    type Ast with

        static member Constructor(pattern: WidgetBuilder<Pattern>, expr: WidgetBuilder<Expr>) =
            WidgetBuilder<MemberDefnExplicitCtorNode>(
                ExplicitConstructorMember.WidgetKey,
                AttributesBundle(
                    StackList.empty(),
                    [| ExplicitConstructorMember.Pat.WithValue(pattern.Compile())
                       ExplicitConstructorMember.ExprValue.WithValue(expr.Compile()) |],
                    Array.empty
                )
            )

        static member Constructor(pattern: WidgetBuilder<Constant>, expr: WidgetBuilder<Expr>) =
            Ast.Constructor(Ast.ConstantPat(pattern), expr)

        static member Constructor(pattern: string, expr: WidgetBuilder<Expr>) =
            Ast.Constructor(Ast.Constant(pattern), expr)

        static member Constructor(pattern: WidgetBuilder<Pattern>, expr: WidgetBuilder<Constant>) =
            Ast.Constructor(pattern, Ast.ConstantExpr(expr))

        static member Constructor(pattern: WidgetBuilder<Pattern>, expr: string) =
            Ast.Constructor(pattern, Ast.Constant(expr))

        static member Constructor(pattern: string, expr: string) =
            Ast.Constructor(Ast.ConstantPat(pattern), expr)

type ExplicitConstructorModifiers =
    [<Extension>]
    static member inline xmlDocs(this: WidgetBuilder<MemberDefnExplicitCtorNode>, xmlDocs: string list) =
        this.AddScalar(ExplicitConstructorMember.XmlDocs.WithValue(xmlDocs))

    [<Extension>]
    static member inline attributes
        (this: WidgetBuilder<MemberDefnExplicitCtorNode>, attributes: WidgetBuilder<AttributeNode> list)
        =
        this.AddScalar(
            ExplicitConstructorMember.MultipleAttributes.WithValue(
                [ for attr in attributes do
                      Gen.mkOak attr ]
            )
        )

    [<Extension>]
    static member inline attribute
        (this: WidgetBuilder<MemberDefnExplicitCtorNode>, attribute: WidgetBuilder<AttributeNode>)
        =
        ExplicitConstructorModifiers.attributes(this, [ attribute ])

    [<Extension>]
    static member inline toPrivate(this: WidgetBuilder<MemberDefnExplicitCtorNode>) =
        this.AddScalar(ExplicitConstructorMember.Accessibility.WithValue(AccessControl.Private))

    [<Extension>]
    static member inline toPublic(this: WidgetBuilder<MemberDefnExplicitCtorNode>) =
        this.AddScalar(ExplicitConstructorMember.Accessibility.WithValue(AccessControl.Public))

    [<Extension>]
    static member inline toInternal(this: WidgetBuilder<MemberDefnExplicitCtorNode>) =
        this.AddScalar(ExplicitConstructorMember.Accessibility.WithValue(AccessControl.Internal))

    [<Extension>]
    static member inline alias(this: WidgetBuilder<MemberDefnExplicitCtorNode>, alias: string) =
        this.AddScalar(ExplicitConstructorMember.Alias.WithValue(alias))
