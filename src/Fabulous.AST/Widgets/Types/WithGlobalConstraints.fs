namespace Fabulous.AST

open Fabulous.AST
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module WithGlobalConstraints =
    let GlobalConstraints =
        Attributes.defineScalar<struct (Type * TypeConstraint seq)> "ConstraintValue"

    let WidgetKey =
        Widgets.register "WithGlobalConstraints" (fun widget ->
            let struct (tp, constraints) = Widgets.getScalarValue widget GlobalConstraints
            Type.WithGlobalConstraints(TypeWithGlobalConstraintsNode(tp, List.ofSeq constraints, Range.Zero)))

[<AutoOpen>]
module WithGlobalConstraintsBuilders =
    type Ast with
        static member WithGlobal(tp: WidgetBuilder<Type>, constraints: WidgetBuilder<TypeConstraint> seq) =
            let constraints = constraints |> Seq.map Gen.mkOak

            WidgetBuilder<Type>(
                WithGlobalConstraints.WidgetKey,
                WithGlobalConstraints.GlobalConstraints.WithValue(Gen.mkOak tp, constraints)
            )

        static member WithGlobal(tp: string, constraints: WidgetBuilder<TypeConstraint> seq) =
            Ast.WithGlobal(Ast.LongIdent(tp), constraints)

        static member WithGlobal(tp: string, constraints: WidgetBuilder<TypeConstraint>) =
            Ast.WithGlobal(tp, [ constraints ])
