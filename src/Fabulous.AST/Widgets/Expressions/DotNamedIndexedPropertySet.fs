namespace Fabulous.AST

open Fabulous.AST
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module DotNamedIndexedPropertySet =
    let Identifier = Attributes.defineWidget "Identifier"

    let Name = Attributes.defineScalar<string> "Name"

    let Property = Attributes.defineWidget "Property"

    let Setter = Attributes.defineWidget "Setter"

    let WidgetKey =
        Widgets.register "DotNamedIndexedPropertySet" (fun widget ->
            let identifier = Widgets.getNodeFromWidget widget Identifier
            let name = Widgets.getScalarValue widget Name
            let property = Widgets.getNodeFromWidget widget Property
            let setter = Widgets.getNodeFromWidget widget Setter

            Expr.DotNamedIndexedPropertySet(
                ExprDotNamedIndexedPropertySetNode(
                    identifier,
                    IdentListNode([ IdentifierOrDot.Ident(SingleTextNode.Create(name)) ], Range.Zero),
                    property,
                    setter,
                    Range.Zero
                )
            ))

[<AutoOpen>]
module DotNamedIndexedPropertySetBuilders =
    type Ast with

        static member DotNamedIndexedPropertySetExpr
            (identifier: WidgetBuilder<Expr>, name: string, property: WidgetBuilder<Expr>, setter: WidgetBuilder<Expr>)
            =
            WidgetBuilder<Expr>(
                DotNamedIndexedPropertySet.WidgetKey,
                AttributesBundle(
                    StackList.one(DotNamedIndexedPropertySet.Name.WithValue(name)),
                    [| DotNamedIndexedPropertySet.Identifier.WithValue(identifier.Compile())
                       DotNamedIndexedPropertySet.Property.WithValue(property.Compile())
                       DotNamedIndexedPropertySet.Setter.WithValue(setter.Compile()) |],
                    Array.empty
                )
            )

        static member DotNamedIndexedPropertySetExpr
            (
                identifier: WidgetBuilder<Constant>,
                name: string,
                property: WidgetBuilder<Expr>,
                setter: WidgetBuilder<Expr>
            ) =
            Ast.DotNamedIndexedPropertySetExpr(Ast.ConstantExpr(identifier), name, property, setter)

        static member DotNamedIndexedPropertySetExpr
            (
                identifier: WidgetBuilder<Expr>,
                name: string,
                property: WidgetBuilder<Constant>,
                setter: WidgetBuilder<Expr>
            ) =
            Ast.DotNamedIndexedPropertySetExpr(identifier, name, Ast.ConstantExpr(property), setter)

        static member DotNamedIndexedPropertySetExpr
            (
                identifier: WidgetBuilder<Expr>,
                name: string,
                property: WidgetBuilder<Expr>,
                setter: WidgetBuilder<Constant>
            ) =
            Ast.DotNamedIndexedPropertySetExpr(identifier, name, property, Ast.ConstantExpr(setter))

        static member DotNamedIndexedPropertySetExpr
            (
                identifier: WidgetBuilder<Constant>,
                name: string,
                property: WidgetBuilder<Constant>,
                setter: WidgetBuilder<Expr>
            ) =
            Ast.DotNamedIndexedPropertySetExpr(Ast.ConstantExpr(identifier), name, Ast.ConstantExpr(property), setter)

        static member DotNamedIndexedPropertySetExpr
            (
                identifier: WidgetBuilder<Constant>,
                name: string,
                property: WidgetBuilder<Expr>,
                setter: WidgetBuilder<Constant>
            ) =
            Ast.DotNamedIndexedPropertySetExpr(Ast.ConstantExpr(identifier), name, property, Ast.ConstantExpr(setter))

        static member DotNamedIndexedPropertySetExpr
            (
                identifier: WidgetBuilder<Expr>,
                name: string,
                property: WidgetBuilder<Constant>,
                setter: WidgetBuilder<Constant>
            ) =
            Ast.DotNamedIndexedPropertySetExpr(identifier, name, Ast.ConstantExpr(property), Ast.ConstantExpr(setter))

        static member DotNamedIndexedPropertySetExpr
            (
                identifier: WidgetBuilder<Constant>,
                name: string,
                property: WidgetBuilder<Constant>,
                setter: WidgetBuilder<Constant>
            ) =
            Ast.DotNamedIndexedPropertySetExpr(
                Ast.ConstantExpr(identifier),
                name,
                Ast.ConstantExpr(property),
                Ast.ConstantExpr(setter)
            )

        static member DotNamedIndexedPropertySetExpr
            (identifier: WidgetBuilder<Expr>, name: string, property: WidgetBuilder<Expr>, setter: string)
            =
            Ast.DotNamedIndexedPropertySetExpr(identifier, name, property, Ast.ConstantExpr setter)

        static member DotNamedIndexedPropertySetExpr
            (identifier: WidgetBuilder<Expr>, name: string, property: string, setter: WidgetBuilder<Expr>)
            =
            Ast.DotNamedIndexedPropertySetExpr(identifier, name, Ast.ConstantExpr property, setter)

        static member DotNamedIndexedPropertySetExpr
            (identifier: WidgetBuilder<Expr>, name: string, property: string, setter: string)
            =
            Ast.DotNamedIndexedPropertySetExpr(identifier, name, Ast.ConstantExpr property, Ast.ConstantExpr setter)

        static member DotNamedIndexedPropertySetExpr
            (identifier: WidgetBuilder<Constant>, name: string, property: WidgetBuilder<Expr>, setter: string)
            =
            Ast.DotNamedIndexedPropertySetExpr(Ast.ConstantExpr identifier, name, property, Ast.ConstantExpr setter)

        static member DotNamedIndexedPropertySetExpr
            (identifier: WidgetBuilder<Constant>, name: string, property: string, setter: WidgetBuilder<Expr>)
            =
            Ast.DotNamedIndexedPropertySetExpr(Ast.ConstantExpr identifier, name, Ast.ConstantExpr property, setter)

        static member DotNamedIndexedPropertySetExpr
            (identifier: WidgetBuilder<Constant>, name: string, property: string, setter: string)
            =
            Ast.DotNamedIndexedPropertySetExpr(
                Ast.ConstantExpr identifier,
                name,
                Ast.ConstantExpr property,
                Ast.ConstantExpr setter
            )

        static member DotNamedIndexedPropertySetExpr
            (identifier: WidgetBuilder<Expr>, name: string, property: WidgetBuilder<Constant>, setter: string)
            =
            Ast.DotNamedIndexedPropertySetExpr(identifier, name, Ast.ConstantExpr property, Ast.ConstantExpr setter)

        static member DotNamedIndexedPropertySetExpr
            (identifier: WidgetBuilder<Expr>, name: string, property: string, setter: WidgetBuilder<Constant>)
            =
            Ast.DotNamedIndexedPropertySetExpr(identifier, name, Ast.ConstantExpr property, Ast.ConstantExpr setter)

        static member DotNamedIndexedPropertySetExpr
            (identifier: WidgetBuilder<Constant>, name: string, property: WidgetBuilder<Constant>, setter: string)
            =
            Ast.DotNamedIndexedPropertySetExpr(
                Ast.ConstantExpr identifier,
                name,
                Ast.ConstantExpr property,
                Ast.ConstantExpr setter
            )

        static member DotNamedIndexedPropertySetExpr
            (identifier: WidgetBuilder<Constant>, name: string, property: string, setter: WidgetBuilder<Constant>)
            =
            Ast.DotNamedIndexedPropertySetExpr(
                Ast.ConstantExpr identifier,
                name,
                Ast.ConstantExpr property,
                Ast.ConstantExpr setter
            )

        static member DotNamedIndexedPropertySetExpr
            (identifier: string, name: string, property: WidgetBuilder<Expr>, setter: WidgetBuilder<Expr>)
            =
            Ast.DotNamedIndexedPropertySetExpr(Ast.ConstantExpr identifier, name, property, setter)

        static member DotNamedIndexedPropertySetExpr
            (identifier: string, name: string, property: WidgetBuilder<Constant>, setter: WidgetBuilder<Expr>)
            =
            Ast.DotNamedIndexedPropertySetExpr(Ast.ConstantExpr identifier, name, Ast.ConstantExpr property, setter)

        static member DotNamedIndexedPropertySetExpr
            (identifier: string, name: string, property: WidgetBuilder<Expr>, setter: WidgetBuilder<Constant>)
            =
            Ast.DotNamedIndexedPropertySetExpr(Ast.ConstantExpr identifier, name, property, Ast.ConstantExpr setter)

        static member DotNamedIndexedPropertySetExpr
            (identifier: string, name: string, property: WidgetBuilder<Constant>, setter: WidgetBuilder<Constant>)
            =
            Ast.DotNamedIndexedPropertySetExpr(
                Ast.ConstantExpr identifier,
                name,
                Ast.ConstantExpr property,
                Ast.ConstantExpr setter
            )

        static member DotNamedIndexedPropertySetExpr
            (identifier: string, name: string, property: WidgetBuilder<Expr>, setter: string)
            =
            Ast.DotNamedIndexedPropertySetExpr(Ast.Constant identifier, name, property, Ast.Constant setter)

        static member DotNamedIndexedPropertySetExpr
            (identifier: string, name: string, property: string, setter: WidgetBuilder<Expr>)
            =
            Ast.DotNamedIndexedPropertySetExpr(Ast.Constant identifier, name, Ast.Constant property, setter)

        static member DotNamedIndexedPropertySetExpr
            (identifier: string, name: string, property: string, setter: string)
            =
            Ast.DotNamedIndexedPropertySetExpr(
                Ast.Constant identifier,
                name,
                Ast.Constant property,
                Ast.Constant setter
            )
