namespace Fabulous.AST

open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak

type IFabIdentifierOrDot = interface end

module IdentifierOrDot =
    let Text = Attributes.defineWidget "Text"
    
    let IdentWidgetKey = Widgets.register "IdentifierOrDot.Ident" (fun widget ->
        let text = Helpers.getNodeFromWidget<SingleTextNode> widget Text
        IdentifierOrDot.Ident text
    )
    
    let KnownDotWidgetKey = Widgets.register "IdentifierOrDot.KnownDot" (fun widget ->
        let text = Helpers.getNodeFromWidget<SingleTextNode> widget Text
        IdentifierOrDot.KnownDot text
    )
    
    let UnknownDotWidgetKey = Widgets.register "IdentifierOrDot.UnknownDot" (fun _ ->
        IdentifierOrDot.UnknownDot
    )

[<AutoOpen>]
module IdentifierOrDotBuilders =
    type Fabulous.AST.Ast with
        static member inline Ident(text: WidgetBuilder<#IFabSingleText>) =
            WidgetBuilder<IFabIdentifierOrDot>(
                IdentifierOrDot.IdentWidgetKey,
                AttributesBundle(
                    StackList.empty(),
                    ValueSome [| IdentifierOrDot.Text.WithValue(text.Compile()) |],
                    ValueNone
                )
            )
            
        static member inline KnownDot(text: WidgetBuilder<#IFabSingleText>) =
            WidgetBuilder<IFabIdentifierOrDot>(
                IdentifierOrDot.KnownDotWidgetKey,
                AttributesBundle(
                    StackList.empty(),
                    ValueSome [| IdentifierOrDot.Text.WithValue(text.Compile()) |],
                    ValueNone
                )
            )
            
        static member inline UnknownDot() =
            WidgetBuilder<IFabIdentifierOrDot>(
                IdentifierOrDot.UnknownDotWidgetKey,
                AttributesBundle(
                    StackList.empty(),
                    ValueNone,
                    ValueNone
                )
            )