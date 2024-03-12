namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fabulous.AST.StackAllocatedCollections
open Fantomas.FCS.Text
open Fantomas.Core.SyntaxOak
open type Fabulous.AST.Ast

module Oak =
    let Decls = Attributes.defineWidgetCollection "Decls"
    let ParsedHashDirectives = Attributes.defineWidgetCollection "ParsedHashDirectives"
    let IsRecursive = Attributes.defineScalar<bool> "IsRecursive"
    let Accessibility = Attributes.defineScalar<AccessControl> "Accessibility"
    let IsNameSpace = Attributes.defineScalar<bool> "IsNameSpace"

[<Extension>]
type OakModifiers =
    [<Extension>]
    static member inline hashDirectives(this: WidgetBuilder<Oak>) =
        AttributeCollectionBuilder<Oak, ParsedHashDirectiveNode>(this, Oak.ParsedHashDirectives)

    [<Extension>]
    static member inline toRecursive(this: WidgetBuilder<Oak>) =
        this.AddScalar(Oak.IsRecursive.WithValue(true))

    [<Extension>]
    static member inline toPrivate(this: WidgetBuilder<Oak>) =
        this.AddScalar(Oak.Accessibility.WithValue(AccessControl.Private))

    [<Extension>]
    static member inline toPublic(this: WidgetBuilder<Oak>) =
        this.AddScalar(Oak.Accessibility.WithValue(AccessControl.Public))

    [<Extension>]
    static member inline toInternal(this: WidgetBuilder<Oak>) =
        this.AddScalar(Oak.Accessibility.WithValue(AccessControl.Internal))
