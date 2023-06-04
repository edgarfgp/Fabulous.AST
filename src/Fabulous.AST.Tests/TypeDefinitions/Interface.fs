namespace Fabulous.AST.Tests.TypeDefinitions

open FSharp.Compiler.Text
open Fabulous.AST.Tests
open Fantomas.Core.SyntaxOak
open NUnit.Framework

open Fabulous.AST
open type Ast

module Interfaces =
    [<Test>]
    let ``Produces an interface abstract method`` () =
        let abstractNode =
            MemberDefnAbstractSlotNode.Method(
                "Print",
                [ (Type.FromString("string"), SingleTextNode("->", Range.Zero)) ],
                Type.FromString("unit")
            )

        AnonymousModule() { Interface("IPrintable") { EscapeHatch(abstractNode) } }
        |> produces
            """
type IPrintable =
    abstract member Print: string -> unit

"""

    [<Test>]
    let ``Produces an interface abstract property`` () =
        let abstractProperty =
            MemberDefnAbstractSlotNode.Property("Pi", Type.FromString("float"))

        AnonymousModule() { Interface("MyInterface") { EscapeHatch(abstractProperty) } }
        |> produces
            """
type MyInterface =
    abstract member Pi: float

"""

    [<Test>]
    let ``Produces an interface abstract with get set`` () =
        let abstractGetSetProperty =
            MemberDefnAbstractSlotNode.GetSet("Area", Type.FromString("float"))

        AnonymousModule() { Interface("MyInterface") { EscapeHatch(abstractGetSetProperty) } }
        |> produces
            """
type MyInterface =
    abstract member Area: float with get, set

"""

    [<Test>]
    let ``Produces an interface abstract methods, properties and get set`` () =
        let method =
            MemberDefnAbstractSlotNode.Method(
                "Add",
                [ (Type.FromString("int"), SingleTextNode("->", Range.Zero))
                  (Type.FromString("int"), SingleTextNode("->", Range.Zero)) ],
                Type.FromString("int")
            )

        let property = MemberDefnAbstractSlotNode.Property("Pi", Type.FromString("float"))

        let getSetProperty =
            MemberDefnAbstractSlotNode.GetSet("Area", Type.FromString("float"))

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
    abstract member Add: int -> int -> int
    abstract member Pi: float
    abstract member Area: float with get, set

"""

    [<Test>]
    let ``Produces a class that implements an interface`` () =
        let method =
            MemberDefnAbstractSlotNode.Method(
                "Add",
                [ (Type.FromString("int"), SingleTextNode("->", Range.Zero))
                  (Type.FromString("int"), SingleTextNode("->", Range.Zero)) ],
                Type.FromString("int")
            )

        let property = MemberDefnAbstractSlotNode.Property("Pi", Type.FromString("float"))

        let getSetProperty =
            MemberDefnAbstractSlotNode.GetSet("Area", Type.FromString("float"))

        let interfaceWidget () =
            Interface("MyInterface") {
                EscapeHatch(method)
                EscapeHatch(property)
                EscapeHatch(getSetProperty)
            }

        AnonymousModule() {
            Class'("MyClass").implements(interfaceWidget())

        }
        |> produces
            """
type MyClass =
    interface MyInterface with
        abstract member Add: int -> int -> int
        abstract member Pi: float
        abstract member Area: float with get, set

"""
