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
        let parameters =
            [ (CommonType.int, SingleTextNode.rightArrow)
              (CommonType.int, SingleTextNode.rightArrow) ]

        let abstractNode =
            MemberDefnAbstractSlotNode.Method("Add", Type.Funs(TypeFunsNode(parameters, CommonType.int, Range.Zero)))

        let parameters =
            [ (CommonType.int, SingleTextNode.star)
              (CommonType.int, SingleTextNode.rightArrow) ]

        let abstractNode1 =
            MemberDefnAbstractSlotNode.Method("Add", Type.Funs(TypeFunsNode(parameters, CommonType.int, Range.Zero)))

        AnonymousModule() {
            Interface("INumericFSharp") { EscapeHatch(abstractNode) }

            Interface("INumericDotNet") { EscapeHatch(abstractNode1) }
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
            [ (Type.SignatureParameter(TypeSignatureParameterNode.Create("a", CommonType.int)),
               SingleTextNode.rightArrow)
              (Type.SignatureParameter(TypeSignatureParameterNode.Create("b", CommonType.int)),
               SingleTextNode.rightArrow) ]

        let abstractNode =
            MemberDefnAbstractSlotNode.Method("Add", Type.Funs(TypeFunsNode(parameters, CommonType.int, Range.Zero)))

        let parameters1 =
            [ (Type.SignatureParameter(TypeSignatureParameterNode.Create("a", CommonType.int)), SingleTextNode.star)
              (Type.SignatureParameter(TypeSignatureParameterNode.Create("b", CommonType.int)),
               SingleTextNode.rightArrow) ]

        let abstractNode1 =
            MemberDefnAbstractSlotNode.Method("Add", Type.Funs(TypeFunsNode(parameters1, CommonType.int, Range.Zero)))

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
        let abstractProperty = MemberDefnAbstractSlotNode.Property("Pi", CommonType.float)

        AnonymousModule() { Interface("MyInterface") { EscapeHatch(abstractProperty) } }
        |> produces
            """
type MyInterface =
    abstract member Pi: float

 """

    [<Test>]
    let ``Produces an interface with attributes and abstract property`` () =
        let abstractProperty = MemberDefnAbstractSlotNode.Property("Pi", CommonType.float)

        AnonymousModule() {
            (Interface("MyInterface") { EscapeHatch(abstractProperty) })
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
        let abstractGetSetProperty =
            MemberDefnAbstractSlotNode.Get("Area", CommonType.float)

        AnonymousModule() { Interface("MyInterface") { EscapeHatch(abstractGetSetProperty) } }
        |> produces
            """
type MyInterface =
    abstract member Area: float with get

 """

    [<Test>]
    let ``Produces an interface abstract with set`` () =
        let abstractGetSetProperty =
            MemberDefnAbstractSlotNode.Set("Area", CommonType.float)

        AnonymousModule() { Interface("MyInterface") { EscapeHatch(abstractGetSetProperty) } }
        |> produces
            """
type MyInterface =
    abstract member Area: float with set

 """

    [<Test>]
    let ``Produces an interface abstract with get set`` () =
        let abstractGetSetProperty =
            MemberDefnAbstractSlotNode.GetSet("Area", CommonType.float)

        AnonymousModule() { Interface("MyInterface") { EscapeHatch(abstractGetSetProperty) } }
        |> produces
            """
type MyInterface =
    abstract member Area: float with get, set

 """

    [<Test>]
    let ``Produces an interface abstract methods, properties and get set`` () =
        let parameters =
            [ (CommonType.int, SingleTextNode.rightArrow)
              (CommonType.int, SingleTextNode.rightArrow)
              (CommonType.string, SingleTextNode.rightArrow) ]

        let method =
            MemberDefnAbstractSlotNode.Method("Add", Type.Funs(TypeFunsNode(parameters, CommonType.int, Range.Zero)))

        let property = MemberDefnAbstractSlotNode.Property("Pi", CommonType.float)

        let getSetProperty = MemberDefnAbstractSlotNode.GetSet("Area", CommonType.float)

        AnonymousModule() {
            Interface("MyInterface") {
                EscapeHatch(method)
                EscapeHatch(property)
                EscapeHatch(getSetProperty)
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
        let parameters =
            [ (CommonType.int, SingleTextNode.rightArrow)
              (CommonType.int, SingleTextNode.rightArrow)
              (CommonType.string, SingleTextNode.rightArrow) ]

        let method =
            MemberDefnAbstractSlotNode.Method("Add", Type.Funs(TypeFunsNode(parameters, CommonType.int, Range.Zero)))

        let property = MemberDefnAbstractSlotNode.Property("Pi", CommonType.float)

        let getSetProperty = MemberDefnAbstractSlotNode.GetSet("Area", CommonType.float)

        AnonymousModule() {
            GenericInterface("MyInterface", [ "'other"; "'another" ]) {
                EscapeHatch(method)
                EscapeHatch(property)
                EscapeHatch(getSetProperty)
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
        let parameters =
            [ (CommonType.int, SingleTextNode.rightArrow)
              (CommonType.int, SingleTextNode.rightArrow)
              (CommonType.string, SingleTextNode.rightArrow) ]

        let method =
            MemberDefnAbstractSlotNode.Method("Add", Type.Funs(TypeFunsNode(parameters, CommonType.int, Range.Zero)))

        let property = MemberDefnAbstractSlotNode.Property("Pi", CommonType.float)

        let getSetProperty = MemberDefnAbstractSlotNode.GetSet("Area", CommonType.float)

        AnonymousModule() {
            (GenericInterface("MyInterface", [ "'other"; "'another" ]) {
                EscapeHatch(method)
                EscapeHatch(property)
                EscapeHatch(getSetProperty)
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
