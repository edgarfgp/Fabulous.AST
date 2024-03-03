namespace Fabulous.AST.Tests.Types

open System
open NUnit.Framework
open Fabulous.AST.Tests

open Fabulous.AST

open type Ast

module Types =
    (*
| Type      | .NET type | Description                                                                           | Example                    |
|-----------|-----------|---------------------------------------------------------------------------------------|----------------------------|
| bool      | Boolean   | Possible values are true and false.                                                  | true/false                 |
| byte      | Byte      | Values from 0 to 255.                                                                 | 1uy                        |
| sbyte     | SByte     | Values from -128 to 127.                                                              | 1y                         |
| int16     | Int16     | Values from -32768 to 32767.                                                          | 1s                         |
| uint16    | UInt16    | Values from 0 to 65535.                                                               | 1us                        |
| int       | Int32     | Values from -2,147,483,648 to 2,147,483,647.                                          | 1                          |
| uint      | UInt32    | Values from 0 to 4,294,967,295.                                                       | 1u                         |
| int64     | Int64     | Values from -9,223,372,036,854,775,808 to 9,223,372,036,854,775,807.                  | 1L                         |
| uint64    | UInt64    | Values from 0 to 18,446,744,073,709,551,615.                                          | 1UL                        |
| nativeint | IntPtr    | A native pointer as a signed integer.                                                 | nativeint 1                |
| unativeint| UIntPtr   | A native pointer as an unsigned integer.                                              | unativeint 1               |
| decimal   | Decimal   | A floating point data type that has at least 28 significant digits.                    | 1.0m                       |
| float, double | Double | A 64-bit floating point type.                                                         | 1.0                        |
| float32, single | Single | A 32-bit floating point type.                                                         | 1.0f                       |
| char      | Char      | Unicode character values.                                                             | 'c'                        |
| string    | String    | Unicode text.                                                                         | "str"                      |
| unit      | NA        | Indicates the absence of an actual value. The type has only one formal value, which is denoted (). The unit value, (), is often used as a placeholder where a value is needed but no real value is available or makes sense. | () |
*)
    [<Test>]
    let ``Value with return primitive types`` () =
        AnonymousModule() {
            Value("a", "false").returnType(Boolean())
            Value("b", "0uy").returnType(Byte())
            Value("c", "1y").returnType(SByte())
            Value("d", "1s").returnType(Int16())
            Value("e", "1us").returnType(UInt16())
            Value("f", "1").returnType(Int32())
            Value("g", "1u").returnType(UInt32())
            Value("h", "1L").returnType(Int64())
            Value("i", "1UL").returnType(UInt64())
            Value("j", "nativeint 1").returnType(IntPtr())
            Value("k", "unativeint 1").returnType(UIntPtr())
            Value("l", "1.0m").returnType(Decimal())
            Value("m", "1.0").returnType(Double())
            Value("n", "1.0f").returnType(Single())
            Value("o", "'c'").returnType(Char())
            Value("p", "\"str\"").returnType(String())
            Value("q", "()").returnType(Unit())
            Value("r", "1.").returnType(Float())
            Value("s", "1.f").returnType(Float32())
            Value("t", "obj").returnType(Obj())
        }
        |> produces
            """
let a: bool = false
let b: byte = 0uy
let c: sbyte = 1y
let d: int16 = 1s
let e: uint16 = 1us
let f: int = 1
let g: uint = 1u
let h: int64 = 1L
let i: uint64 = 1UL
let j: nativeint = nativeint 1
let k: unativeint = unativeint 1
let l: decimal = 1.0m
let m: double = 1.0
let n: single = 1.0f
let o: char = 'c'
let p: string = "str"
let q: unit = ()
let r: float = 1.
let s: float32 = 1.f
let t: obj = obj
"""
