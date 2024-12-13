namespace Fabulous.AST

open Fabulous.AST
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module WithGlobalConstraints =
    let GlobalConstraints =
        Attributes.defineScalar<struct (Type * TypeConstraint list)> "ConstraintValue"

    let WidgetKey =
        Widgets.register "WithGlobalConstraints" (fun widget ->
            let struct (tp, constraints) = Widgets.getScalarValue widget GlobalConstraints
            Type.WithGlobalConstraints(TypeWithGlobalConstraintsNode(tp, constraints, Range.Zero)))

[<AutoOpen>]
module WithGlobalConstraintsBuilders =
    type Ast with
        static member WithGlobal(tp: WidgetBuilder<Type>, constraints: WidgetBuilder<TypeConstraint> list) =
            let constraints = constraints |> List.map Gen.mkOak

            WidgetBuilder<Type>(
                WithGlobalConstraints.WidgetKey,
                WithGlobalConstraints.GlobalConstraints.WithValue(Gen.mkOak tp, constraints)
            )

        static member WithGlobal(tp: string, constraints: WidgetBuilder<TypeConstraint> list) =
            Ast.WithGlobal(Ast.LongIdent(tp), constraints)

        static member WithGlobal(tp: string, constraints: WidgetBuilder<TypeConstraint>) =
            Ast.WithGlobal(tp, [ constraints ])
