namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fabulous.AST.StackAllocatedCollections
open Fantomas.Core.SyntaxOak

open type Fabulous.AST.Ast

module BindingNode =
    let BodyExpr = Attributes.defineWidget "BindingBodyExpr"
    let IsMutable = Attributes.defineScalar<bool> "IsMutable"
    let XmlDocs = Attributes.defineScalar<string list> "XmlDoc"
    let IsInlined = Attributes.defineScalar<bool> "IsInlined"
    let IsStatic = Attributes.defineScalar<bool> "IsStatic"

    let MultipleAttributes =
        Attributes.defineScalar<AttributeNode list> "MultipleAttributes"

    let Accessibility = Attributes.defineScalar<AccessControl> "Accessibility"
    let Return = Attributes.defineWidget "Return"
    let TypeParams = Attributes.defineWidget "TypeParams"
    let Parameters = Attributes.defineScalar<Pattern list> "Parameters"

type BindingNodeModifiers =
    [<Extension>]
    static member inline xmlDocs(this: WidgetBuilder<BindingNode>, xmlDocs: string list) =
        this.AddScalar(BindingNode.XmlDocs.WithValue(xmlDocs))

    [<Extension>]
    static member inline attributes(this: WidgetBuilder<BindingNode>, attributes: WidgetBuilder<AttributeNode> list) =
        this.AddScalar(
            BindingNode.MultipleAttributes.WithValue(
                [ for attr in attributes do
                      Gen.mkOak attr ]
            )
        )

    [<Extension>]
    static member inline attribute(this: WidgetBuilder<BindingNode>, attribute: WidgetBuilder<AttributeNode>) =
        BindingNodeModifiers.attributes(this, [ attribute ])

    [<Extension>]
    static member inline toPrivate(this: WidgetBuilder<BindingNode>) =
        this.AddScalar(BindingNode.Accessibility.WithValue(AccessControl.Private))

    [<Extension>]
    static member inline toPublic(this: WidgetBuilder<BindingNode>) =
        this.AddScalar(BindingNode.Accessibility.WithValue(AccessControl.Public))

    [<Extension>]
    static member inline toInternal(this: WidgetBuilder<BindingNode>) =
        this.AddScalar(BindingNode.Accessibility.WithValue(AccessControl.Internal))

    [<Extension>]
    static member inline returnType(this: WidgetBuilder<BindingNode>, returnType: WidgetBuilder<Type>) =
        this.AddWidget(BindingNode.Return.WithValue(returnType.Compile()))

    [<Extension>]
    static member inline toMutable(this: WidgetBuilder<BindingNode>) =
        this.AddScalar(BindingNode.IsMutable.WithValue(true))

    [<Extension>]
    static member inline toInlined(this: WidgetBuilder<BindingNode>) =
        this.AddScalar(BindingNode.IsInlined.WithValue(true))

    [<Extension>]
    static member inline toStatic(this: WidgetBuilder<BindingNode>) =
        this.AddScalar(BindingNode.IsStatic.WithValue(true))

    [<Extension>]
    static member inline typeParams(this: WidgetBuilder<BindingNode>, typeParams: WidgetBuilder<TyparDecls>) =
        this.AddWidget(BindingNode.TypeParams.WithValue(typeParams.Compile()))

type ValueYieldExtensions =
    [<Extension>]
    static member inline Yield
        (_: CollectionBuilder<'parent, ModuleDecl>, x: WidgetBuilder<BindingNode>)
        : CollectionContent =
        let node = Gen.mkOak x
        let moduleDecl = ModuleDecl.TopLevelBinding node
        let widget = Ast.EscapeHatch(moduleDecl).Compile()
        { Widgets = MutStackArray1.One(widget) }

    [<Extension>]
    static member inline Yield(_: CollectionBuilder<'parent, ModuleDecl>, node: BindingNode) : CollectionContent =
        let moduleDecl = ModuleDecl.TopLevelBinding node
        let widget = Ast.EscapeHatch(moduleDecl).Compile()
        { Widgets = MutStackArray1.One(widget) }
