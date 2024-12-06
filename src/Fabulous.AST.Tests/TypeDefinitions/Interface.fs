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
                TypeDefn("INumericFSharp") { AbstractMember("Add", [ Int(); Int() ], Int()) }

                TypeDefn("INumericDotNet") { AbstractMember("Add", [ Int(); Int() ], Int(), true) }
            }
        }
        |> produces
            """
type INumericFSharp =
    abstract Add: int -> int -> int

type INumericDotNet =
    abstract Add: int * int -> int

 """

module GenericInterface =

    [<Fact>]
    let ``Produces a generic interface with TypeParams``() =
        Oak() {
            AnonymousModule() {
                TypeDefn("MyInterface") {
                    let parameters = [ Int(); Int(); String() ]
                    AbstractMember("Add", parameters, Int())
                    AbstractMember("Pi", Float())
                    AbstractMember("Area", Float(), true, true)
                }
                |> _.typeParams(PostfixList([ "'other"; "'another" ]))

            }
        }
        |> produces
            """
type MyInterface<'other, 'another> =
    abstract Add: int -> int -> string -> int
    abstract Pi: float
    abstract Area: float with get, set

 """
