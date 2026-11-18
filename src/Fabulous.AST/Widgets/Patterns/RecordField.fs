namespace Fabulous.AST

open Fabulous.AST
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Syntax
open Fantomas.FCS.Text

module RecordFieldPat =

    let Prefix = Attributes.defineScalar<string> "Prefix"

    let FieldName = Attributes.defineScalar<string> "FieldName"

    let Pat = Attributes.defineWidget "Pat"

    let WidgetKey =
        Widgets.register "RecordFieldPat" (fun widget ->
            let fieldName =
                Widgets.getScalarValue widget FieldName
                |> PrettyNaming.NormalizeIdentifierBackticks

            let pattern = Widgets.getNodeFromWidget widget Pat

            // Fantomas 8 keeps the optional module prefix in the field's identifier
            // (`Prefix.field = pat`) instead of a separate node.
            let identifier =
                match Widgets.tryGetScalarValue widget Prefix with
                | ValueSome prefix ->
                    [ IdentifierOrDot.Ident(SingleTextNode.Create(PrettyNaming.NormalizeIdentifierBackticks prefix))
                      IdentifierOrDot.KnownDot(SingleTextNode.dot)
                      IdentifierOrDot.Ident(SingleTextNode.Create(fieldName)) ]
                | ValueNone -> [ IdentifierOrDot.Ident(SingleTextNode.Create(fieldName)) ]

            NamePatPairNode(IdentListNode(identifier, Range.range0), SingleTextNode.equals, pattern, Range.range0))

[<AutoOpen>]
module RecordFieldPatBuilders =
    type Ast with
        static member RecordFieldPat(name: string, pat: WidgetBuilder<Pattern>) =
            WidgetBuilder<NamePatPairNode>(
                RecordFieldPat.WidgetKey,
                AttributesBundle(
                    StackList.one(RecordFieldPat.FieldName.WithValue(name)),
                    [| RecordFieldPat.Pat.WithValue(pat.Compile()) |],
                    Array.empty
                )
            )

        static member RecordFieldPat(name: string, pat: WidgetBuilder<Constant>) =
            Ast.RecordFieldPat(name, Ast.ConstantPat(pat))

        static member RecordFieldPat(name: string, pat: string) =
            Ast.RecordFieldPat(name, Ast.Constant(pat))

        static member RecordFieldPat(name: string, pat: WidgetBuilder<Pattern>, prefix: string) =
            WidgetBuilder<NamePatPairNode>(
                RecordFieldPat.WidgetKey,
                AttributesBundle(
                    StackList.two(RecordFieldPat.FieldName.WithValue(name), RecordFieldPat.Prefix.WithValue(prefix)),
                    [| RecordFieldPat.Pat.WithValue(pat.Compile()) |],
                    Array.empty
                )
            )

        static member RecordFieldPat(name: string, pat: WidgetBuilder<Constant>, prefix: string) =
            Ast.RecordFieldPat(name, Ast.ConstantPat(pat), prefix)

        static member RecordFieldPat(name: string, pat: string, prefix: string) =
            Ast.RecordFieldPat(name, Ast.Constant(pat), prefix)
