namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fantomas.FCS.Text
open Fantomas.Core.SyntaxOak
open Fabulous.AST.StackAllocatedCollections.StackList

module TyparDeclNode =

    let TyPar = Attributes.defineScalar<string> "Return"

    let MultipleAttributes =
        Attributes.defineScalar<AttributeNode list> "MultipleAttributes"

    let IntersectionConstrains =
        Attributes.defineScalar<Type list> "IntersectionConstrains"

    let WidgetKey =
        Widgets.register "TyparDeclNode" (fun widget ->
            let tyPar = Widgets.getScalarValue widget TyPar

            let attributes =
                Widgets.tryGetScalarValue widget MultipleAttributes
                |> ValueOption.map(fun x -> Some(MultipleAttributeListNode.Create(x)))
                |> ValueOption.defaultValue None

            let intersectionConstrains =
                Widgets.getScalarValue widget IntersectionConstrains
                |> List.map(Choice1Of2)
                |> List.intersperse(Choice2Of2 SingleTextNode.comma)

            TyparDeclNode(attributes, SingleTextNode.Create(tyPar), intersectionConstrains, Range.Zero))

type TyparDeclNodeModifiers =
    [<Extension>]
    static member inline attributes(this: WidgetBuilder<TyparDeclNode>, attributes: WidgetBuilder<AttributeNode> list) =
        this.AddScalar(
            TyparDeclNode.MultipleAttributes.WithValue(
                [ for attr in attributes do
                      Gen.mkOak attr ]
            )
        )

    [<Extension>]
    static member inline attribute(this: WidgetBuilder<TyparDeclNode>, attribute: WidgetBuilder<AttributeNode>) =
        TyparDeclNodeModifiers.attributes(this, [ attribute ])

[<AutoOpen>]
module TyparDeclNodeBuilders =
    type Ast with

        static member TyparDecl(tyPar: string, constraints: WidgetBuilder<Type> list) =
            let constrains = constraints |> List.map Gen.mkOak

            WidgetBuilder<TyparDeclNode>(
                TyparDeclNode.WidgetKey,
                AttributesBundle(
                    StackList.two(
                        TyparDeclNode.TyPar.WithValue(tyPar),
                        TyparDeclNode.IntersectionConstrains.WithValue(constrains)
                    ),
                    Array.empty,
                    Array.empty
                )
            )

        static member TyparDecl(tyPar: string, value: WidgetBuilder<Type>) = Ast.TyparDecl(tyPar, [ value ])

        static member TyparDecl(tyPar: string) = Ast.TyparDecl(tyPar, [])

        static member TyparDecl(tyPar: string, constraints: string list) =
            Ast.TyparDecl(tyPar, constraints |> List.map Ast.LongIdent)

        static member TyparDecl(tyPar: string, value: string) = Ast.TyparDecl(tyPar, [ value ])

module TyparDecls =

    let TyparDecl = Attributes.defineWidget "TyparDeclNode"

    let WidgetSinglePrefixKey =
        Widgets.register "SinglePrefix" (fun widget ->
            let value = Widgets.getNodeFromWidget widget TyparDecl
            TyparDecls.SinglePrefix(value))

    let Decls = Attributes.defineScalar<TyparDeclNode list> "Decls"

    let WidgetPrefixListKey =
        Widgets.register "PrefixList" (fun widget ->
            let decls = Widgets.getScalarValue widget Decls

            TyparDecls.PrefixList(
                TyparDeclsPrefixListNode(
                    SingleTextNode.leftParenthesis,
                    decls,
                    SingleTextNode.rightParenthesis,
                    Range.Zero
                )
            ))

    let Constraints = Attributes.defineScalar<TypeConstraint list> "ConstraintValue"

    let WidgetPostfixListKey =
        Widgets.register "PostfixList" (fun widget ->
            let decls = Widgets.getScalarValue widget Decls
            let constraints = Widgets.getScalarValue widget Constraints

            TyparDecls.PostfixList(
                TyparDeclsPostfixListNode(
                    SingleTextNode.lessThan,
                    decls,
                    constraints,
                    SingleTextNode.greaterThan,
                    Range.Zero
                )
            ))

[<AutoOpen>]
module TyparDeclsBuilders =
    type Ast with

        static member SinglePrefix(value: WidgetBuilder<TyparDeclNode>) =
            WidgetBuilder<TyparDecls>(
                TyparDecls.WidgetSinglePrefixKey,
                AttributesBundle(StackList.empty(), [| TyparDecls.TyparDecl.WithValue(value.Compile()) |], Array.empty)
            )

        static member SinglePrefix(value: string) = Ast.SinglePrefix(Ast.TyparDecl(value))

        static member PrefixList(decls: WidgetBuilder<TyparDeclNode> list) =
            let decls = decls |> List.map Gen.mkOak

            WidgetBuilder<TyparDecls>(
                TyparDecls.WidgetPrefixListKey,
                AttributesBundle(StackList.one(TyparDecls.Decls.WithValue(decls)), Array.empty, Array.empty)
            )

        static member PrefixList(decls: string list) =
            let decls = decls |> List.map Ast.TyparDecl
            Ast.PrefixList(decls)

        static member PrefixList(decls: WidgetBuilder<TyparDeclNode>) = Ast.PrefixList([ decls ])

        static member PrefixList(decl: string) = Ast.PrefixList([ decl ])

        static member PostfixList
            (decls: WidgetBuilder<TyparDeclNode> list, constraints: WidgetBuilder<TypeConstraint> list)
            =
            let decls = decls |> List.map Gen.mkOak
            let constraints = constraints |> List.map Gen.mkOak

            WidgetBuilder<TyparDecls>(
                TyparDecls.WidgetPostfixListKey,
                AttributesBundle(
                    StackList.two(TyparDecls.Decls.WithValue(decls), TyparDecls.Constraints.WithValue(constraints)),
                    Array.empty,
                    Array.empty
                )
            )

        static member PostfixList(decls: string list, constraints: WidgetBuilder<TypeConstraint> list) =
            let decls = decls |> List.map Ast.TyparDecl
            Ast.PostfixList(decls, constraints)

        static member PostfixList(decls: WidgetBuilder<TyparDeclNode> list) = Ast.PostfixList(decls, [])

        static member PostfixList(decls: string list) =
            let decls = decls |> List.map Ast.TyparDecl
            Ast.PostfixList(decls)

        static member PostfixList(decl: WidgetBuilder<TyparDeclNode>) = Ast.PostfixList([ decl ], [])

        static member PostfixList(decl: string) = Ast.PostfixList([ decl ])

        static member PostfixList(constraints: WidgetBuilder<TypeConstraint> list) = Ast.PostfixList([], constraints)

        static member PostfixList(constraints: WidgetBuilder<TypeConstraint>) = Ast.PostfixList([], [ constraints ])

        static member PostfixList(decls: WidgetBuilder<TyparDeclNode>, constraints: WidgetBuilder<TypeConstraint>) =
            Ast.PostfixList([ decls ], [ constraints ])

        static member PostfixList(decls: string, constraints: WidgetBuilder<TypeConstraint>) =
            Ast.PostfixList([ decls ], [ constraints ])
