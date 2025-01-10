namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fabulous.AST
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module Named =
    let Value = Attributes.defineScalar<string> "Value"

    let Accessibility = Attributes.defineScalar<AccessControl> "Accessibility"

    let WidgetKey =
        Widgets.register "Named" (fun widget ->
            let name =
                Widgets.getScalarValue widget Value
                |> Fantomas.FCS.Syntax.PrettyNaming.NormalizeIdentifierBackticks
                |> SingleTextNode.Create

            let accessControl =
                Widgets.tryGetScalarValue widget Accessibility
                |> ValueOption.defaultValue AccessControl.Unknown

            let accessControl =
                match accessControl with
                | Public -> Some(SingleTextNode.``public``)
                | Private -> Some(SingleTextNode.``private``)
                | Internal -> Some(SingleTextNode.``internal``)
                | Unknown -> None

            Pattern.Named(PatNamedNode(accessControl, name, Range.Zero)))

[<AutoOpen>]
module NamedBuilders =
    type Ast with

        static member NamedPat(value: string) =
            WidgetBuilder<Pattern>(Named.WidgetKey, Named.Value.WithValue(value))

type NamedPatModifiers =
    [<Extension>]
    static member inline toPrivate(this: WidgetBuilder<Pattern>) =
        this.AddScalar(Named.Accessibility.WithValue(AccessControl.Private))

    [<Extension>]
    static member inline toPublic(this: WidgetBuilder<Pattern>) =
        this.AddScalar(Named.Accessibility.WithValue(AccessControl.Public))

    [<Extension>]
    static member inline toInternal(this: WidgetBuilder<Pattern>) =
        this.AddScalar(Named.Accessibility.WithValue(AccessControl.Internal))
