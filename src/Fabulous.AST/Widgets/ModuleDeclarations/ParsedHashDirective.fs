namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fabulous.AST.StackAllocatedCollections
open Fantomas.FCS.Text
open Fantomas.Core.SyntaxOak
open type Fabulous.AST.Ast

module HashDirective =
    let Ident = Attributes.defineScalar<string> "Ident"
    let Args = Attributes.defineScalar<string list> "Args"

    let WidgetKey =
        Widgets.register "HashDirective" (fun widget ->
            let ident = Helpers.getScalarValue widget Ident

            let args = Helpers.getScalarValue widget Args |> List.map(SingleTextNode.Create)

            ParsedHashDirectiveNode(ident, args, Range.Zero))

[<AutoOpen>]
module HashDirectiveBuilders =
    type Ast with

        static member private BaseHashDirective(ident: string, args: string list, wrappedInQuotes: bool) =
            let args =
                if wrappedInQuotes then
                    args |> List.map(fun value -> $"\"{value}\"")
                else
                    args

            WidgetBuilder<ParsedHashDirectiveNode>(
                HashDirective.WidgetKey,
                HashDirective.Ident.WithValue(ident),
                HashDirective.Args.WithValue(args)
            )

        static member NoWarn(value: string) =
            Ast.BaseHashDirective("nowarn", [ value ], true)

        static member NoWarn(value: string list) =
            Ast.BaseHashDirective("nowarn", value, true)

        static member HashDirective(ident: string, value: string) =
            Ast.BaseHashDirective(ident, [ value ], false)

        static member HashDirective(ident: string) = Ast.BaseHashDirective(ident, [], false)

        static member HashDirective(ident: string, value: string list) =
            Ast.BaseHashDirective(ident, value, false)


[<Extension>]
type HashDirectiveNodeExtensions =

    [<Extension>]
    static member inline Yield
        (
            _: CollectionBuilder<'parent, ModuleDecl>,
            x: WidgetBuilder<ParsedHashDirectiveNode>
        ) : CollectionContent =
        let node = Gen.mkOak x
        let moduleDecl = ModuleDecl.HashDirectiveList(HashDirectiveListNode([ node ]))
        let widget = Ast.EscapeHatch(moduleDecl).Compile()
        { Widgets = MutStackArray1.One(widget) }
