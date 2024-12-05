namespace Fabulous.AST

open Fabulous.AST
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Syntax
open Fantomas.FCS.Text

module TypeAnonRecord =
    let Fields = Attributes.defineScalar<(string * WidgetBuilder<Type>) list> "Fields"

    let IsStructNode = Attributes.defineScalar<bool> "IsStructNode"

    let WidgetKey =
        Widgets.register "TypeStructTuple" (fun widget ->
            let fields = Widgets.getScalarValue widget Fields

            let fields =
                fields
                |> List.map(fun (name, widget) ->
                    let name = PrettyNaming.NormalizeIdentifierBackticks name
                    let name = SingleTextNode.Create(name)
                    let tp = Gen.mkOak widget
                    (name, tp))

            let isStructNode = Widgets.getScalarValue widget IsStructNode

            let structNode =
                if isStructNode then
                    Some SingleTextNode.``struct``
                else
                    None

            let openingToken =
                if isStructNode then
                    Some SingleTextNode.leftBracket
                else
                    None

            Type.AnonRecord(
                TypeAnonRecordNode(structNode, openingToken, fields, SingleTextNode.rightCurlyBraceWithBar, Range.Zero)
            ))

[<AutoOpen>]
module TypeAnonRecordBuilders =
    type Ast with
        static member AnonRecord(fields: (string * WidgetBuilder<Type>) list) =
            WidgetBuilder<Type>(
                TypeAnonRecord.WidgetKey,
                AttributesBundle(
                    StackList.two(TypeAnonRecord.Fields.WithValue(fields), TypeAnonRecord.IsStructNode.WithValue(false)),
                    Array.empty,
                    Array.empty
                )
            )

        static member AnonRecord(fields: (string * string) list) =
            let fields = fields |> List.map(fun (name, value) -> (name, Ast.LongIdent value))
            Ast.AnonRecord(fields)

        static member StructAnonRecord(fields: (string * WidgetBuilder<Type>) list) =
            WidgetBuilder<Type>(
                TypeAnonRecord.WidgetKey,
                AttributesBundle(
                    StackList.two(TypeAnonRecord.Fields.WithValue(fields), TypeAnonRecord.IsStructNode.WithValue(true)),
                    Array.empty,
                    Array.empty
                )
            )

        static member StructAnonRecord(fields: (string * string) list) =
            let fields = fields |> List.map(fun (name, value) -> (name, Ast.LongIdent value))
            Ast.StructAnonRecord(fields)
