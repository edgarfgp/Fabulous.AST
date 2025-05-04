namespace Fabulous.AST.Tests.Types

open Xunit
open Fabulous.AST.Tests

open Fabulous.AST

open type Ast

module LongIdent =

    [<Fact>]
    let ``Long identifier with multiple parts``() =
        Oak() {
            AnonymousModule() { Value("x", UnitExpr(), LongIdent([ "System"; "Collections"; "Generic"; "List" ])) }
        }
        |> produces
            """
let x: System.Collections.Generic.List = ()
"""

    [<Fact>]
    let ``Long identifier with single part``() =
        Oak() { AnonymousModule() { Value("x", UnitExpr(), LongIdent("int")) } }
        |> produces
            """
let x: int = ()
"""

    [<Fact>]
    let ``Primitive types``() =
        Oak() {
            AnonymousModule() {
                Value("a", UnitExpr(), Boolean())
                Value("b", UnitExpr(), Byte())
                Value("c", UnitExpr(), SByte())
                Value("d", UnitExpr(), Int16())
                Value("e", UnitExpr(), UInt16())
                Value("f", UnitExpr(), Int())
                Value("g", UnitExpr(), UInt32())
                Value("h", UnitExpr(), Int64())
                Value("i", UnitExpr(), UInt64())
                Value("j", UnitExpr(), IntPtr())
                Value("k", UnitExpr(), UIntPtr())
                Value("l", UnitExpr(), Decimal())
                Value("m", UnitExpr(), Float())
                Value("n", UnitExpr(), Double())
                Value("o", UnitExpr(), Float32())
                Value("p", UnitExpr(), Single())
                Value("q", UnitExpr(), Char())
                Value("r", UnitExpr(), String())
                Value("s", UnitExpr(), Obj())
                Value("t", UnitExpr(), Unit())
                Value("u", UnitExpr(), Option())
                Value("v", UnitExpr(), List())
                Value("w", UnitExpr(), Seq())
                Value("x", UnitExpr(), Null())
            }
        }
        |> produces
            """
let a: bool = ()
let b: byte = ()
let c: sbyte = ()
let d: int16 = ()
let e: uint16 = ()
let f: int = ()
let g: uint32 = ()
let h: int64 = ()
let i: uint64 = ()
let j: nativeint = ()
let k: unativeint = ()
let l: decimal = ()
let m: float = ()
let n: double = ()
let o: float32 = ()
let p: single = ()
let q: char = ()
let r: string = ()
let s: obj = ()
let t: unit = ()
let u: option = ()
let v: list = ()
let w: seq = ()
let x: null = ()
"""
