namespace Fabulous.AST.Tests.TypeDefinitions

open Fabulous.AST.Tests
open NUnit.Framework

open Fabulous.AST
open type Ast

module GenericInterface =


    [<Test>]
    let ``Produces a generic interface with TypeParams`` () =
        AnonymousModule() {
            GenericInterface("MyInterface", [ "'other"; "'another" ]) {
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
