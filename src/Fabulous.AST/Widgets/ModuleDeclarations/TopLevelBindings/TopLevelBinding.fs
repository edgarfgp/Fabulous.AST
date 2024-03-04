namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fabulous.AST.StackAllocatedCollections
open Fantomas.Core.SyntaxOak

open type Fabulous.AST.Ast

module BindingNode =
    let NameWidget = Attributes.defineWidget "Name"
    let NameString = Attributes.defineScalar<string> "FunctionName"
    let BodyExpr = Attributes.defineWidget "Value"
    let IsMutable = Attributes.defineScalar<bool> "IsMutable"
    let XmlDocs = Attributes.defineScalar<string list> "XmlDoc"
    let IsInlined = Attributes.defineScalar<bool> "IsInlined"
    let IsStatic = Attributes.defineScalar<bool> "IsStatic"
    let MultipleAttributes = Attributes.defineWidgetCollection "MultipleAttributes"
    let Accessibility = Attributes.defineScalar<AccessControl> "Accessibility"
    let Return = Attributes.defineWidget "Return"
    let TypeParams = Attributes.defineScalar<string list> "TypeParams"
    let Parameters = Attributes.defineWidget "Parameters"

[<Extension>]
type ValueModifiers =
    [<Extension>]
    static member inline xmlDocs(this: WidgetBuilder<BindingNode>, xmlDocs: string list) =
        this.AddScalar(BindingNode.XmlDocs.WithValue(xmlDocs))

    [<Extension>]
    static member inline attributes(this: WidgetBuilder<BindingNode>) =
        AttributeCollectionBuilder<BindingNode, AttributeNode>(this, BindingNode.MultipleAttributes)

    [<Extension>]
    static member inline attributes(this: WidgetBuilder<BindingNode>, attributes: string list) =
        AttributeCollectionBuilder<BindingNode, AttributeNode>(this, BindingNode.MultipleAttributes) {
            for attribute in attributes do
                Ast.Attribute(attribute)
        }

    [<Extension>]
    static member inline attribute(this: WidgetBuilder<BindingNode>, attribute: WidgetBuilder<AttributeNode>) =
        AttributeCollectionBuilder<BindingNode, AttributeNode>(this, BindingNode.MultipleAttributes) { attribute }

    [<Extension>]
    static member inline attribute(this: WidgetBuilder<BindingNode>, attribute: string) =
        AttributeCollectionBuilder<BindingNode, AttributeNode>(this, BindingNode.MultipleAttributes) {
            Ast.Attribute(attribute)
        }

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
    static member inline returnType(this: WidgetBuilder<BindingNode>, returnType: string) =
        ValueModifiers.returnType(this, Ast.TypeLongIdent(returnType))

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
    static member inline typeParameters(this: WidgetBuilder<BindingNode>, typeParams: string list) =
        this.AddScalar(BindingNode.TypeParams.WithValue(typeParams))


[<Extension>]
type ValueYieldExtensions =
    [<Extension>]
    static member inline Yield
        (
            _: CollectionBuilder<'parent, ModuleDecl>,
            x: WidgetBuilder<BindingNode>
        ) : CollectionContent =
        let node = Tree.compile x
        let moduleDecl = ModuleDecl.TopLevelBinding node
        let widget = Ast.EscapeHatch(moduleDecl).Compile()
        { Widgets = MutStackArray1.One(widget) }
