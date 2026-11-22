namespace Fabulous.AST

open Fabulous.AST
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Syntax

module TypeVar =
    let Identifier = Attributes.defineScalar<string> "Identifier"

    let WidgetKey =
        Widgets.register "TypeVar" (fun widget ->
            let identifier = Widgets.getScalarValue widget Identifier

            // A type variable ('T, ^T) is written as is; anything else is an identifier.
            let identifier =
                if identifier.StartsWith("'") || identifier.StartsWith("^") then
                    identifier
                else
                    PrettyNaming.NormalizeIdentifierBackticks identifier

            Type.Var(SingleTextNode.Create(identifier)))

[<AutoOpen>]
module TypeVarBuilders =
    type Ast with
        static member Var(value: string) =
            WidgetBuilder<Type>(TypeVar.WidgetKey, TypeVar.Identifier.WithValue(value))
