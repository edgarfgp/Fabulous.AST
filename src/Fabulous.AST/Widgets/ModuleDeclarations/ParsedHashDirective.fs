namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fabulous.AST.StackAllocatedCollections
open Fantomas.FCS.Syntax
open Fantomas.FCS.Text
open Fantomas.Core.SyntaxOak
open type Fabulous.AST.Ast

module HashDirective =
    let Ident = Attributes.defineScalar<string> "Ident"
    let Args = Attributes.defineScalar<string list> "Args"

    let HasDirective = Attributes.defineScalar<bool> "HasDirective"

    let WidgetKey =
        Widgets.register "HashDirective" (fun widget ->
            let ident = Widgets.getScalarValue widget Ident

            let hasDirective =
                Widgets.tryGetScalarValue widget HasDirective |> ValueOption.defaultValue false

            let args =
                Widgets.getScalarValue widget Args
                |> List.map(fun value -> if hasDirective then value else $"\"{value}\"")
                |> List.map(SingleTextNode.Create)

            ParsedHashDirectiveNode(ident, args, Range.Zero))

[<AutoOpen>]
module HashDirectiveBuilders =
    type Ast with

        static member private BaseHashDirective(ident: string, args: string list, hasDirective: bool) =
            WidgetBuilder<ParsedHashDirectiveNode>(
                HashDirective.WidgetKey,
                HashDirective.Ident.WithValue(ident),
                HashDirective.Args.WithValue(args),
                HashDirective.HasDirective.WithValue(hasDirective)
            )

        static member NoWarn(value: string) =
            Ast.BaseHashDirective("nowarn", [ value ], false)

        static member NoWarn(args: string list) =
            Ast.BaseHashDirective("nowarn", args, false)

        static member HashDirective(ident: string, value: string) =
            Ast.BaseHashDirective(ident, [ value ], true)

        static member HashDirective(ident: string) = Ast.BaseHashDirective(ident, [], true)

        static member HashDirective(ident: string, args: string list) =
            Ast.BaseHashDirective(ident, args, true)

type HashDirectiveNodeExtensions =

    [<Extension>]
    static member inline Yield
        (_: CollectionBuilder<'parent, ModuleDecl>, x: WidgetBuilder<ParsedHashDirectiveNode>)
        : CollectionContent =
        let node = Gen.mkOak x
        let moduleDecl = ModuleDecl.HashDirectiveList(HashDirectiveListNode([ node ]))
        let widget = Ast.EscapeHatch(moduleDecl).Compile()
        { Widgets = MutStackArray1.One(widget) }
