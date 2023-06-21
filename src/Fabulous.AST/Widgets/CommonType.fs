namespace Fabulous.AST

open Fantomas.Core.SyntaxOak

[<AutoOpen>]
module CommonType =
    let mkType (typ: string) : Type =
        [ IdentifierOrDot.CreateIdent(typ) ] |> IdentListNode.Create |> Type.LongIdent

    let bool = mkType "bool"
    let byte = mkType "byte"
    let sbyte = mkType "sbyte"
    let int16 = mkType "int16"
    let uint16 = mkType "uint16"
    let int = mkType "int"
    let uint = mkType "uint"
    let int64 = mkType "int64"
    let uint64 = mkType "uint64"
    let nativeint = mkType "nativeint"
    let unativeint = mkType "unativeint"
    let decimal = mkType "decimal"
    let float = mkType "float"
    let double = mkType "double"
    let float32 = mkType "float32"
    let single = mkType "single"
    let char = mkType "char"
    let string = mkType "string"
    let unit = mkType "unit"
    let obj = mkType "obj"
