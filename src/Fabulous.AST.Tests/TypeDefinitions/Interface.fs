namespace Fabulous.AST.Tests.TypeDefinitions

open Fabulous.AST.Tests
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text
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
