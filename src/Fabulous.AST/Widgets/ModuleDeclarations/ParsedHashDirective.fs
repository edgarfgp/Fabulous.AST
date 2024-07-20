namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fabulous.AST.StackAllocatedCollections
open Fantomas.FCS.Text
open Fantomas.Core.SyntaxOak
open type Fabulous.AST.Ast

module ParsedHashDirectives =
    let Ident = Attributes.defineScalar<string> "Ident"
    let Arguments = Attributes.defineScalar<string list> "Args"

    let WidgetKey =
        Widgets.register "HashDirective" (fun widget ->
            let ident = Widgets.getScalarValue widget Ident

            let arguments =
                Widgets.getScalarValue widget Arguments
                |> List.map(fun arg ->
                    Choice2Of2(IdentListNode([ IdentifierOrDot.Ident(SingleTextNode.Create arg) ], Range.Zero)))

            ParsedHashDirectiveNode(ident, arguments, Range.Zero))

[<AutoOpen>]
module HashDirectiveBuilders =
    type Ast with

        static member private BaseHashDirective(ident: string, arguments: WidgetBuilder<Constant> list) =
            let arguments =
                arguments
                |> List.choose(fun arg ->
                    match Gen.mkOak arg with
                    | Constant.FromText node -> Some node.Text
                    | Constant.Unit _ -> None
                    | Constant.Measure _ -> None)

            WidgetBuilder<ParsedHashDirectiveNode>(
                ParsedHashDirectives.WidgetKey,
                ParsedHashDirectives.Ident.WithValue(ident),
                ParsedHashDirectives.Arguments.WithValue(arguments)
            )

        static member NoWarn(args: WidgetBuilder<Constant> list) = Ast.BaseHashDirective("nowarn", args)

        static member NoWarn(args: string list) =
            Ast.NoWarn(args |> List.map(Ast.Constant))

        static member NoWarn(value: WidgetBuilder<Constant>) = Ast.NoWarn([ value ])

        static member NoWarn(value: string) = Ast.NoWarn(Ast.Constant(value))

        static member Help(value: WidgetBuilder<Constant>) =
            Ast.BaseHashDirective("help", [ value ])

        static member Help(value: string) = Ast.Help(Ast.Constant(value))

        static member HashDirective(ident: string, args: WidgetBuilder<Constant> list) =
            Ast.BaseHashDirective(ident, args)

        static member HashDirective(ident: string, args: string list) =
            Ast.HashDirective(ident, args |> List.map(Ast.Constant))

        static member HashDirective(ident: string, value: WidgetBuilder<Constant>) = Ast.HashDirective(ident, [ value ])

        static member HashDirective(ident: string, value: string) =
            Ast.HashDirective(ident, Ast.Constant(value))

        static member HashDirective(ident: string) = Ast.HashDirective(ident, [])

type HashDirectiveNodeExtensions =

    [<Extension>]
    static member inline Yield
        (_: CollectionBuilder<'parent, ModuleDecl>, x: ParsedHashDirectiveNode)
        : CollectionContent =
        let moduleDecl = ModuleDecl.HashDirectiveList(HashDirectiveListNode([ x ]))
        let widget = Ast.EscapeHatch(moduleDecl).Compile()
        { Widgets = MutStackArray1.One(widget) }

    [<Extension>]
    static member inline Yield
        (this: CollectionBuilder<'parent, ModuleDecl>, x: WidgetBuilder<ParsedHashDirectiveNode>)
        : CollectionContent =
        let node = Gen.mkOak x
        HashDirectiveNodeExtensions.Yield(this, node)
