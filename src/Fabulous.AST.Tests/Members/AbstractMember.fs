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
                AbstractSetMember("Area", CommonType.Float)
                AbstractGetSetMember("Area", CommonType.Float)
                AbstractPropertyMember("Pi", CommonType.Float)
                AbstractCurriedMethodMember("Add", [ CommonType.Int32; CommonType.Int32 ], CommonType.Int32)
                AbstractTupledMethodMember("Add", [ CommonType.Int32; CommonType.Int32 ], CommonType.Int32)
                let parameters = [ ("a", CommonType.Int32); ("b", CommonType.Int32) ]
                let parameters1 = [ ("a", CommonType.Int32); ("b", CommonType.Int32) ]
                AbstractCurriedMethodMember("Add", parameters, CommonType.Int32)
                AbstractTupledMethodMember("Add", parameters1, CommonType.Int32)
            }
        }
        |> produces
            """
type Meh =
    abstract member Area: float with get
    abstract member Area: float with set
    abstract member Area: float with get, set
    abstract member Pi: float
    abstract member Add: int -> int -> int
    abstract member Add: int * int -> int
    abstract member Add: a: int -> b: int -> int
    abstract member Add: a: int * b: int -> int

"""

    [<Test>]
    let ``Produces a genetic interface with multiple abstract members types`` () =
        AnonymousModule() {
            GenericInterface("Meh", [ "'other"; "'another" ]) {
                AbstractGetMember("Area", CommonType.Float)
                AbstractSetMember("Area", CommonType.Float)
                AbstractGetSetMember("Area", CommonType.Float)
                AbstractPropertyMember("Pi", CommonType.Float)
                AbstractCurriedMethodMember("Add", [ CommonType.Int32; CommonType.Int32 ], CommonType.Int32)
                AbstractTupledMethodMember("Add", [ CommonType.Int32; CommonType.Int32 ], CommonType.Int32)
                let parameters = [ ("a", CommonType.Int32); ("b", CommonType.Int32) ]
                let parameters1 = [ ("a", CommonType.Int32); ("b", CommonType.Int32) ]
                AbstractCurriedMethodMember("Add", parameters, CommonType.Int32)
                AbstractTupledMethodMember("Add", parameters1, CommonType.Int32)
            }
        }
        |> produces
            """
type Meh <'other, 'another> =
    abstract member Area: float with get
    abstract member Area: float with set
    abstract member Area: float with get, set
    abstract member Pi: float
    abstract member Add: int -> int -> int
    abstract member Add: int * int -> int
    abstract member Add: a: int -> b: int -> int
    abstract member Add: a: int * b: int -> int

 """
