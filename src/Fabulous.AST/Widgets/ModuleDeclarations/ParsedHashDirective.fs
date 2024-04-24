namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fabulous.AST.StackAllocatedCollections
open Fantomas.FCS.Text
open Fantomas.Core.SyntaxOak
open type Fabulous.AST.Ast

module HashDirective =
    let Ident = Attributes.defineScalar<string> "Ident"
    let Args = Attributes.defineScalar<StringVariant list> "Args"

    let WidgetKey =
        Widgets.register "HashDirective" (fun widget ->
            let ident = Widgets.getScalarValue widget Ident

            let args =
                Widgets.getScalarValue widget Args
                |> List.map(StringParsing.normalizeIdentifierQuotes)
                |> List.map(SingleTextNode.Create)

            ParsedHashDirectiveNode(ident, args, Range.Zero))

[<AutoOpen>]
module HashDirectiveBuilders =
    type Ast with

        static member private BaseHashDirective(ident: string, args: StringVariant list) =
            WidgetBuilder<ParsedHashDirectiveNode>(
                HashDirective.WidgetKey,
                HashDirective.Ident.WithValue(ident),
                HashDirective.Args.WithValue(args)
            )

        static member NoWarn(value: string) =
            Ast.BaseHashDirective("nowarn", [ DoubleQuoted value ])

        static member NoWarn(args: string list) =
            let args = args |> List.map(DoubleQuoted)
            Ast.BaseHashDirective("nowarn", args)

        static member HashDirective(ident: string, value: string) =
            Ast.BaseHashDirective(ident, [ Unquoted value ])

        static member HashDirective(ident: string) = Ast.BaseHashDirective(ident, [])

        static member HashDirective(ident: string, value: string list) =
            let args = value |> List.map(Unquoted)
            Ast.BaseHashDirective(ident, args)

type HashDirectiveNodeExtensions =

    [<Extension>]
    static member inline Yield
        (_: CollectionBuilder<'parent, ModuleDecl>, x: WidgetBuilder<ParsedHashDirectiveNode>)
        : CollectionContent =
        let node = Gen.mkOak x
        let moduleDecl = ModuleDecl.HashDirectiveList(HashDirectiveListNode([ node ]))
        let widget = Ast.EscapeHatch(moduleDecl).Compile()
        { Widgets = MutStackArray1.One(widget) }
