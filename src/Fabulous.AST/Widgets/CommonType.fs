namespace Fabulous.AST

open Fantomas.Core.SyntaxOak

[<AutoOpen>]
module CommonType =
    let mkType (typ: string) : Type =
        [ IdentifierOrDot.CreateIdent(typ) ] |> IdentListNode.Create |> Type.LongIdent

    let Boolean = mkType "bool"
    let Byte = mkType "byte"
    let SByte = mkType "sbyte"
    let Int16 = mkType "int16"
    let Uint16 = mkType "uint16"
    let Int32 = mkType "int"
    let UInt32 = mkType "uint"
    let Int64 = mkType "int64"
    let Uint64 = mkType "uint64"
    let IntPtr = mkType "nativeint"
    let UIntPtr = mkType "unativeint"
    let Decimal = mkType "decimal"
    let Float = mkType "float"
    let Double = mkType "double"
    let Float32 = mkType "float32"
    let Single = mkType "single"
    let Char = mkType "char"
    let String = mkType "string"
    let Obj = mkType "obj"
