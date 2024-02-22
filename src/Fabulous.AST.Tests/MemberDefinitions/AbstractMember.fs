namespace Fabulous.AST.Tests.MethodDefinitions

open Fabulous.AST
open Fabulous.AST.Tests
open type Ast
open NUnit.Framework

module AbstractMembers =

    [<Test>]
    let ``Produces a classes with a abstract members`` () =
        AnonymousModule() {
            Interface("Meh") {
                AbstractGetMember("Area", CommonType.Float)
                AbstractGetMember("Area1", "float")

                AbstractSetMember("Area2", CommonType.Float)
                AbstractSetMember("Area3", "float")

                AbstractGetSetMember("Area4", CommonType.Float)
                AbstractGetSetMember("Area5", "float")

                AbstractPropertyMember("Pi", CommonType.Float)
                AbstractPropertyMember("Pi2", "float")

                AbstractTupledMethodMember("Add", [ "int"; "int" ], "int")
                AbstractTupledMethodMember("Add2", [ CommonType.Int32; CommonType.Int32 ], CommonType.Int32)

                AbstractTupledMethodMember(
                    "Add3",
                    [ (Some "a", CommonType.Int32); (Some "b", CommonType.Int32) ],
                    CommonType.Int32
                )

                AbstractTupledMethodMember("Add4", [ (Some "a", "int"); (Some "b", "int") ], "int")

                AbstractCurriedMethodMember("Add5", [ "int"; "int" ], "int")
                AbstractCurriedMethodMember("Add6", [ CommonType.Int32; CommonType.Int32 ], CommonType.Int32)

                AbstractCurriedMethodMember(
                    "Add7",
                    [ (Some "a", CommonType.Int32); (Some "b", CommonType.Int32) ],
                    CommonType.Int32
                )

                AbstractCurriedMethodMember("Add8", [ (Some "a", "int"); (Some "b", "int") ], "int")

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

    [<Test>]
    let ``Produces a genetic interface with multiple abstract members types`` () =
        AnonymousModule() {
            Interface("Meh", [ "'other"; "'another" ]) {
                AbstractGetMember("Area", CommonType.Float)
                AbstractGetMember("Area1", "float")

                AbstractSetMember("Area2", CommonType.Float)
                AbstractSetMember("Area3", "float")

                AbstractGetSetMember("Area4", CommonType.Float)
                AbstractGetSetMember("Area5", "float")

                AbstractPropertyMember("Pi", CommonType.Float)
                AbstractPropertyMember("Pi2", "float")

                AbstractTupledMethodMember("Add", [ "int"; "int" ], "int")
                AbstractTupledMethodMember("Add2", [ CommonType.Int32; CommonType.Int32 ], CommonType.Int32)

                AbstractTupledMethodMember(
                    "Add3",
                    [ (Some "a", CommonType.Int32); (Some "b", CommonType.Int32) ],
                    CommonType.Int32
                )

                AbstractTupledMethodMember("Add4", [ (Some "a", "int"); (Some "b", "int") ], "int")

                AbstractCurriedMethodMember("Add5", [ "int"; "int" ], "int")
                AbstractCurriedMethodMember("Add6", [ CommonType.Int32; CommonType.Int32 ], CommonType.Int32)

                AbstractCurriedMethodMember(
                    "Add7",
                    [ (Some "a", CommonType.Int32); (Some "b", CommonType.Int32) ],
                    CommonType.Int32
                )

                AbstractCurriedMethodMember("Add8", [ (Some "a", "int"); (Some "b", "int") ], "int")

            }
        }
        |> produces
            """
type Meh <'other, 'another> =
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
