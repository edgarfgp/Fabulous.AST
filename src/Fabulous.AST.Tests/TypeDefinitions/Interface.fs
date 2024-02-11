namespace Fabulous.AST.Tests.TypeDefinitions

open Fabulous.AST.Tests
open NUnit.Framework

open Fabulous.AST
open type Ast

module Interface =
    [<Test>]
    let ``Produces an interface abstract method`` () =
        AnonymousModule() {
            Interface("INumericFSharp") {
                AbstractCurriedMethodMember("Add", [ CommonType.Int32; CommonType.Int32 ], CommonType.Int32)
            }

            Interface("INumericDotNet") {
                AbstractTupledMethodMember("Add", [ CommonType.Int32; CommonType.Int32 ], CommonType.Int32)
            }
        }
        |> produces
            """
type INumericFSharp =
    abstract member Add: int -> int -> int

type INumericDotNet =
    abstract member Add: int * int -> int

 """

module GenericInterface =

    [<Test>]
    let ``Produces a generic interface with TypeParams`` () =
        AnonymousModule() {
            Interface("MyInterface", [ "'other"; "'another" ]) {
                let parameters = [ CommonType.Int32; CommonType.Int32; CommonType.String ]
                AbstractCurriedMethodMember("Add", parameters, CommonType.Int32)
                AbstractPropertyMember("Pi", CommonType.Float)
                AbstractGetSetMember("Area", CommonType.Float)
            }

        }
        |> produces
            """
type MyInterface <'other, 'another> =
    abstract member Add: int -> int -> string -> int
    abstract member Pi: float
    abstract member Area: float with get, set

 """
