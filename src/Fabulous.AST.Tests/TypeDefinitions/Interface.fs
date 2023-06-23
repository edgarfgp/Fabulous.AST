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
                AbstractMethodMember("Add", [ CommonType.Int32; CommonType.Int32 ], CommonType.Int32)
            }

        // TODO int * int -> int
        //Interface("INumericDotNet") { EscapeHatch(abstractNode1) }
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
        let parameters =
            [ (Type.SignatureParameter(TypeSignatureParameterNode.Create("a", CommonType.Int32)),
               SingleTextNode.rightArrow)
              (Type.SignatureParameter(TypeSignatureParameterNode.Create("b", CommonType.Int32)),
               SingleTextNode.rightArrow) ]

        let abstractNode =
            MemberDefnAbstractSlotNode.Method("Add", Type.Funs(TypeFunsNode(parameters, CommonType.Int32, Range.Zero)))

        let parameters1 =
            [ (Type.SignatureParameter(TypeSignatureParameterNode.Create("a", CommonType.Int32)), SingleTextNode.star)
              (Type.SignatureParameter(TypeSignatureParameterNode.Create("b", CommonType.Int32)),
               SingleTextNode.rightArrow) ]

        let abstractNode1 =
            MemberDefnAbstractSlotNode.Method("Add", Type.Funs(TypeFunsNode(parameters1, CommonType.Int32, Range.Zero)))

        AnonymousModule() {
            Interface("INumericFSharp") { EscapeHatch(abstractNode) }

            Interface("INumericDotNet") { EscapeHatch(abstractNode1) }
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
                AbstractMethodMember("Add", [ CommonType.Int32; CommonType.Int32; CommonType.String ], CommonType.Int32)
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
                AbstractMethodMember("Add", [ CommonType.Int32; CommonType.Int32; CommonType.String ], CommonType.Int32)
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
                AbstractMethodMember("Add", [ CommonType.Int32; CommonType.Int32; CommonType.String ], CommonType.Int32)
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
