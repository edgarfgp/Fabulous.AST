namespace Fabulous.AST

open System
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

[<RequireQualifiedAccess>]
module CommonType =
    let mkLongIdent (typ: string) : Type =
        Type.LongIdent(IdentListNode([ IdentifierOrDot.Ident(SingleTextNode.Create(typ)) ], Range.Zero))

    let Boolean = mkLongIdent "bool"
    let Byte = mkLongIdent "byte"
    let SByte = mkLongIdent "sbyte"
    let Int16 = mkLongIdent "int16"
    let Uint16 = mkLongIdent "uint16"
    let Int32 = mkLongIdent "int"
    let UInt32 = mkLongIdent "uint"
    let Int64 = mkLongIdent "int64"
    let Uint64 = mkLongIdent "uint64"
    let IntPtr = mkLongIdent "nativeint"
    let UIntPtr = mkLongIdent "unativeint"
    let Decimal = mkLongIdent "decimal"
    let Float = mkLongIdent "float"
    let Double = mkLongIdent "double"
    let Float32 = mkLongIdent "float32"
    let Single = mkLongIdent "single"
    let Char = mkLongIdent "char"
    let String = mkLongIdent "string"
    let Obj = mkLongIdent "obj"
    let Unit = mkLongIdent "unit"
