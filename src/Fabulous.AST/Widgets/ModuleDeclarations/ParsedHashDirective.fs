namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fabulous.AST.StackAllocatedCollections
open Fantomas.FCS.Text
open Fantomas.Core.SyntaxOak
open type Fabulous.AST.Ast

type HashDirectiveNode(ident: string, args: SingleTextNode list) =
    inherit ParsedHashDirectiveNode(ident, args, Range.Zero)

module HashDirective =
    let Ident = Attributes.defineScalar<string> "Ident"
    let Args = Attributes.defineScalar<string list> "Args"

    let WidgetKey =
        Widgets.register "HashDirective" (fun widget ->
            let ident = Helpers.getScalarValue widget Ident

            let args = Helpers.getScalarValue widget Args |> List.map(SingleTextNode.Create)

            HashDirectiveNode(ident, args))

[<AutoOpen>]
module HashDirectiveBuilders =
    type Ast with

        static member private BaseHashDirective(ident: string, args: string list, wrappedInQuotes: bool) =
            let args =
                if wrappedInQuotes then
                    args |> List.map(fun arg -> $"\"{arg}\"")
                else
                    args

            WidgetBuilder<HashDirectiveNode>(
                HashDirective.WidgetKey,
                HashDirective.Ident.WithValue(ident),
                HashDirective.Args.WithValue(args)
            )

        static member NoWarn(arg: string) =
            Ast.BaseHashDirective("nowarn", [ arg ], true)

        static member NoWarn(args: string list) =
            Ast.BaseHashDirective("nowarn", args, true)

        static member HashDirective(ident: string, args: string) =
            Ast.BaseHashDirective(ident, [ args ], false)

        static member HashDirective(ident: string) = Ast.BaseHashDirective(ident, [], false)

        static member HashDirective(ident: string, args: string list) =
            Ast.BaseHashDirective(ident, args, false)


[<Extension>]
type HashDirectiveNodeExtensions =

    [<Extension>]
    static member inline Yield
        (
            _: CollectionBuilder<'parent, ModuleDecl>,
            x: WidgetBuilder<HashDirectiveNode>
        ) : CollectionContent =
        let node = Tree.compile x
        let moduleDecl = ModuleDecl.HashDirectiveList(HashDirectiveListNode([ node ]))
        let widget = Ast.EscapeHatch(moduleDecl).Compile()
        { Widgets = MutStackArray1.One(widget) }
