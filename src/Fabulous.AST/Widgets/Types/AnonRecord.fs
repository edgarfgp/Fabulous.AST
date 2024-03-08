namespace Fabulous.AST

open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module TypeAnonRecord =
    let Fields = Attributes.defineScalar<(string * WidgetBuilder<Type>) list> "Fields"

    let IsStructNode = Attributes.defineScalar<bool> "IsStructNode"

    let WidgetKey =
        Widgets.register "TypeStructTuple" (fun widget ->
            let fields = Helpers.getScalarValue widget Fields

            let fields =
                fields
                |> List.map(fun (name, widget) -> (SingleTextNode.Create(name), Gen.mkOak widget))

            let isStructNode = Helpers.getScalarValue widget IsStructNode

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
                    ValueNone,
                    ValueNone
                )
            )

        static member AnonRecord(fields: (string * string) list) =
            let fields =
                fields |> List.map(fun (name, typeName) -> (name, Ast.LongIdent(typeName)))

            WidgetBuilder<Type>(
                TypeAnonRecord.WidgetKey,
                AttributesBundle(
                    StackList.two(TypeAnonRecord.Fields.WithValue(fields), TypeAnonRecord.IsStructNode.WithValue(false)),
                    ValueNone,
                    ValueNone
                )
            )

        static member StructAnonRecord(fields: (string * WidgetBuilder<Type>) list) =
            WidgetBuilder<Type>(
                TypeAnonRecord.WidgetKey,
                AttributesBundle(
                    StackList.two(TypeAnonRecord.Fields.WithValue(fields), TypeAnonRecord.IsStructNode.WithValue(true)),
                    ValueNone,
                    ValueNone
                )
            )

        static member StructAnonRecord(fields: (string * string) list) =
            let fields =
                fields |> List.map(fun (name, typeName) -> (name, Ast.LongIdent(typeName)))

            WidgetBuilder<Type>(
                TypeAnonRecord.WidgetKey,
                AttributesBundle(
                    StackList.two(TypeAnonRecord.Fields.WithValue(fields), TypeAnonRecord.IsStructNode.WithValue(true)),
                    ValueNone,
                    ValueNone
                )
            )
