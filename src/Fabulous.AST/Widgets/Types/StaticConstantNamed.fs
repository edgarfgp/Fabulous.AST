namespace Fabulous.AST

open Fabulous.AST
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module TypeStaticConstantNamed =
    let Identifier = Attributes.defineWidget "Identifier"
    let Value = Attributes.defineWidget "Value"

    let WidgetKey =
        Widgets.register "TypeStaticConstantNamed" (fun widget ->
            let identifier = Widgets.getNodeFromWidget<Type> widget Identifier
            let value = Widgets.getNodeFromWidget<Type> widget Value
            Type.StaticConstantNamed(TypeStaticConstantNamedNode(identifier, value, Range.Zero)))

[<AutoOpen>]
module TypeStaticConstantNamedBuilders =
    type Ast with
        static member StaticConstantNamed(identifier: WidgetBuilder<Type>, value: WidgetBuilder<Type>) =
            WidgetBuilder<Type>(
                TypeStaticConstantNamed.WidgetKey,
                AttributesBundle(
                    StackList.empty(),
                    [| TypeStaticConstantNamed.Identifier.WithValue(identifier.Compile())
                       TypeStaticConstantNamed.Value.WithValue(value.Compile()) |],
                    Array.empty
                )
            )

        static member StaticConstantNamed(identifier: string, value: WidgetBuilder<Type>) =
            Ast.StaticConstantNamed(Ast.LongIdent identifier, value)

        static member StaticConstantNamed(identifier: WidgetBuilder<Type>, value: string) =
            Ast.StaticConstantNamed(identifier, Ast.LongIdent value)

        static member StaticConstantNamed(identifier: string, value: string) =
            Ast.StaticConstantNamed(Ast.LongIdent identifier, Ast.LongIdent value)
