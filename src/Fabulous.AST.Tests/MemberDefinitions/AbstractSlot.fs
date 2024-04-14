namespace Fabulous.AST.Tests.MethodDefinitions

open Fabulous.AST
open Fabulous.AST.Tests
open Xunit

open type Ast

module AbstractMembers =

    [<Fact>]
    let ``Produces a classes with a abstract members``() =
        Oak() {
            AnonymousModule() {
                Interface("Meh") {
                    AbstractGet("Area", Float())
                    AbstractGet("Area1", "float")

                    AbstractSet("Area2", Float())
                    AbstractSet("Area3", "float")

                    AbstractGetSet("Area4", Float())
                    AbstractGetSet("Area5", "float")

                    AbstractProperty("Pi", Float())
                    AbstractProperty("Pi2", "float")

                    AbstractTupledMethod("Add", [ "int"; "int" ], "int")
                    AbstractTupledMethod("Add2", [ Int32(); Int32() ], Int32())

                    AbstractTupledMethod("Add3", [ (Some "a", Int32()); (Some "b", Int32()) ], Int32())

                    AbstractTupledMethod("Add4", [ (Some "a", "int"); (Some "b", "int") ], "int")

                    AbstractCurriedMethod("Add5", [ "int"; "int" ], "int")
                    AbstractCurriedMethod("Add6", [ Int32(); Int32() ], Int32())

                    AbstractCurriedMethod("Add7", [ (Some "a", Int32()); (Some "b", Int32()) ], Int32())

                    AbstractCurriedMethod("Add8", [ (Some "a", "int"); (Some "b", "int") ], "int")

                }
            }
        }
        |> produces
            """
type Meh =
    abstract member Area: float with get
    abstract member Area1: float with get
    abstract member Area2: float with set
    abstract member Area3: float with set
    abstract member Area4: float with get, set
    abstract member Area5: float with get, set
    abstract member Pi: float
    abstract member Pi2: float
    abstract member Add: int * int -> int
    abstract member Add2: int * int -> int
    abstract member Add3: a: int * b: int -> int
    abstract member Add4: a: int * b: int -> int
    abstract member Add5: int -> int -> int
    abstract member Add6: int -> int -> int
    abstract member Add7: a: int -> b: int -> int
    abstract member Add8: a: int -> b: int -> int

"""

    [<Fact>]
    let ``Produces a genetic interface with multiple abstract members types``() =
        Oak() {
            AnonymousModule() {
                Interface("Meh") {
                    AbstractGet("Area", Float())
                    AbstractGet("Area1", "float")

                    AbstractSet("Area2", Float())
                    AbstractSet("Area3", "float")

                    AbstractGetSet("Area4", Float())
                    AbstractGetSet("Area5", "float")

                    AbstractProperty("Pi", Float())
                    AbstractProperty("Pi2", "float")

                    AbstractTupledMethod("Add", [ "int"; "int" ], "int")
                    AbstractTupledMethod("Add2", [ Int32(); Int32() ], Int32())

                    AbstractTupledMethod("Add3", [ (Some "a", Int32()); (Some "b", Int32()) ], Int32())

                    AbstractTupledMethod("Add4", [ (Some "a", "int"); (Some "b", "int") ], "int")

                    AbstractCurriedMethod("Add5", [ "int"; "int" ], "int")
                    AbstractCurriedMethod("Add6", [ Int32(); Int32() ], Int32())

                    AbstractCurriedMethod("Add7", [ (Some "a", Int32()); (Some "b", Int32()) ], Int32())

                    AbstractCurriedMethod("Add8", [ (Some "a", "int"); (Some "b", "int") ], "int")

                }
                |> _.typeParams([ "'other"; "'another" ])
            }
        }
        |> produces
            """
type Meh<'other, 'another> =
    abstract member Area: float with get
    abstract member Area1: float with get
    abstract member Area2: float with set
    abstract member Area3: float with set
    abstract member Area4: float with get, set
    abstract member Area5: float with get, set
    abstract member Pi: float
    abstract member Pi2: float
    abstract member Add: int * int -> int
    abstract member Add2: int * int -> int
    abstract member Add3: a: int * b: int -> int
    abstract member Add4: a: int * b: int -> int
    abstract member Add5: int -> int -> int
    abstract member Add6: int -> int -> int
    abstract member Add7: a: int -> b: int -> int
    abstract member Add8: a: int -> b: int -> int

"""
