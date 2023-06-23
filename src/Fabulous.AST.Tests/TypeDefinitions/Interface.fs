namespace Fabulous.AST.Tests.TypeDefinitions

open Fabulous.AST.Tests
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text
open NUnit.Framework

open Fabulous.AST
open type Ast

module Interfaces =
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

    [<Test>]
    let ``Produces interfaces abstract methods with named parameters`` () =
        let parameters = [ ("a", CommonType.Int32); ("b", CommonType.Int32) ]

        let parameters1 = [ ("a", CommonType.Int32); ("b", CommonType.Int32) ]

        AnonymousModule() {
            Interface("INumericFSharp") { AbstractCurriedMethodMember("Add", parameters, CommonType.Int32) }

            Interface("INumericDotNet") { AbstractTupledMethodMember("Add", parameters1, CommonType.Int32) }
        }
        |> produces
            """
type INumericFSharp =
    abstract member Add: a: int -> b: int -> int

type INumericDotNet =
    abstract member Add: a: int * b: int -> int

 """

    [<Test>]
    let ``Produces an interface abstract property`` () =
        AnonymousModule() { Interface("MyInterface") { AbstractPropertyMember("Pi", CommonType.Float) } }
        |> produces
            """
type MyInterface =
    abstract member Pi: float

 """

    [<Test>]
    let ``Produces an interface with attributes and abstract property`` () =

        AnonymousModule() {
            (Interface("MyInterface") { AbstractPropertyMember("Pi", CommonType.Float) })
                .attributes([ "Obsolete" ])
        }
        |> produces
            """
[<Obsolete>]
type MyInterface =
    abstract member Pi: float

 """

    [<Test>]
    let ``Produces an interface abstract with get`` () =
        AnonymousModule() { Interface("MyInterface") { AbstractGetMember("Area", CommonType.Float) } }
        |> produces
            """
type MyInterface =
    abstract member Area: float with get

 """

    [<Test>]
    let ``Produces an interface abstract with set`` () =
        AnonymousModule() { Interface("MyInterface") { AbstractSetMember("Area", CommonType.Float) } }
        |> produces
            """
type MyInterface =
    abstract member Area: float with set

 """

    [<Test>]
    let ``Produces an interface abstract with get set`` () =
        AnonymousModule() { Interface("MyInterface") { AbstractGetSetMember("Area", CommonType.Float) } }
        |> produces
            """
type MyInterface =
    abstract member Area: float with get, set

 """

    [<Test>]
    let ``Produces an interface abstract methods, properties and get set`` () =
        AnonymousModule() {
            Interface("MyInterface") {
                let parameters = [ CommonType.Int32; CommonType.Int32; CommonType.String ]
                AbstractCurriedMethodMember("Add", parameters, CommonType.Int32)
                AbstractPropertyMember("Pi", CommonType.Float)
                AbstractGetSetMember("Area", CommonType.Float)
            }
        }
        |> produces
            """
type MyInterface =
    abstract member Add: int -> int -> string -> int
    abstract member Pi: float
    abstract member Area: float with get, set

 """

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

    [<Test>]
    let ``Produces a genetic interface with attributes and TypeParams`` () =
        AnonymousModule() {
            (GenericInterface("MyInterface", [ "'other"; "'another" ]) {
                let parameters = [ CommonType.Int32; CommonType.Int32; CommonType.String ]
                AbstractCurriedMethodMember("Add", parameters, CommonType.Int32)
                AbstractPropertyMember("Pi", CommonType.Float)
                AbstractGetSetMember("Area", CommonType.Float)
            })
                .attributes([ "Obsolete" ])

        }
        |> produces
            """
[<Obsolete>]
type MyInterface <'other, 'another> =
    abstract member Add: int -> int -> string -> int
    abstract member Pi: float
    abstract member Area: float with get, set

 """
