namespace Fabulous.AST.Tests.TypeDefinitions

open Fabulous.AST.Tests
open Xunit

open Fabulous.AST
open type Ast

module Interface =
    [<Fact>]
    let ``Produces an interface abstract method``() =
        Oak() {
            AnonymousModule() {
                Interface("INumericFSharp") { AbstractCurriedMethod("Add", [ Int(); Int() ], Int()) }

                Interface("INumericDotNet") { AbstractTupledMethod("Add", [ Int(); Int() ], Int()) }
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

    [<Fact>]
    let ``Produces a generic interface with TypeParams``() =
        Oak() {
            AnonymousModule() {
                Interface("MyInterface") {
                    let parameters = [ Int(); Int(); String() ]
                    AbstractCurriedMethod("Add", parameters, Int())
                    AbstractProperty("Pi", Float())
                    AbstractGetSet("Area", Float())
                }
                |> _.typeParams(PostfixList([ "'other"; "'another" ]))

            }
        }
        |> produces
            """
type MyInterface<'other, 'another> =
    abstract member Add: int -> int -> string -> int
    abstract member Pi: float
    abstract member Area: float with get, set

 """
