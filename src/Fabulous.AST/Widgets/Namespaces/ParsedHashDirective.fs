namespace Fabulous.AST

open Fantomas.FCS.Text
open Fantomas.Core.SyntaxOak
open type Fabulous.AST.Ast

type HashDirectiveNode(ident: string, args: string list) =
    inherit ParsedHashDirectiveNode(ident, [ for arg in args -> SingleTextNode.Create($"\"{arg}\"") ], Range.Zero)

module HashDirective =
    let Ident = Attributes.defineScalar "Ident"
    let Args = Attributes.defineScalar "Args"

    let WidgetKey =
        Widgets.register "HashDirective" (fun widget ->
            let ident = Helpers.getScalarValue widget Ident
            let args = Helpers.getScalarValue widget Args
            HashDirectiveNode(ident, args))

[<AutoOpen>]
module HashDirectiveBuilders =
    type Ast with

        static member inline NoWarn(arg: string) =
            WidgetBuilder<HashDirectiveNode>(
                HashDirective.WidgetKey,
                HashDirective.Ident.WithValue("nowarn"),
                HashDirective.Args.WithValue([ arg ])
            )
