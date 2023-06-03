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

//     [<Test>]
//     let ``Produces a class explicit constructor with no params`` () =
//         let memberNode =
//             MemberDefn.Member(
//                 BindingNode(
//                     None,
//                     None,
//                     MultipleTextsNode([ SingleTextNode("member", Range.Zero) ], Range.Zero),
//                     false,
//                     None,
//                     None,
//                     Choice1Of2(
//                         IdentListNode(
//                             [ IdentifierOrDot.Ident(SingleTextNode("this", Range.Zero))
//                               IdentifierOrDot.Ident(SingleTextNode(".", Range.Zero))
//                               IdentifierOrDot.Ident(SingleTextNode("Name", Range.Zero)) ],
//                             Range.Zero
//                         )
//                     ),
//                     None,
//                     List.Empty,
//                     None,
//                     SingleTextNode("=", Range.Zero),
//                     Expr.Constant(Constant.FromText(SingleTextNode("\"\"", Range.Zero))),
//                     Range.Zero
//                 )
//             )
//
//         AnonymousModule() { Class("Person", []) { EscapeHatch(memberNode) } }
//         |> produces
//             """
// type Person () =
//     member this.Name = ""
//
// """
//
//     [<Test>]
//     let ``Produces a class explicit constructor with params`` () =
//         let memberNode =
//             MemberDefn.Member(
//                 BindingNode(
//                     None,
//                     None,
//                     MultipleTextsNode([ SingleTextNode("member", Range.Zero) ], Range.Zero),
//                     false,
//                     None,
//                     None,
//                     Choice1Of2(
//                         IdentListNode(
//                             [ IdentifierOrDot.Ident(SingleTextNode("this", Range.Zero))
//                               IdentifierOrDot.Ident(SingleTextNode(".", Range.Zero))
//                               IdentifierOrDot.Ident(SingleTextNode("Name", Range.Zero)) ],
//                             Range.Zero
//                         )
//                     ),
//                     None,
//                     List.Empty,
//                     None,
//                     SingleTextNode("=", Range.Zero),
//                     Expr.Constant(Constant.FromText(SingleTextNode("name", Range.Zero))),
//                     Range.Zero
//                 )
//             )
//
//         let param =
//             SimplePatNode(None, false, SingleTextNode("name", Range.Zero), None, Range.Zero)
//
//         AnonymousModule() { Class("Person", [ param ]) { EscapeHatch(memberNode) } }
//         |> produces
//             """
// type Person (name) =
//     member this.Name = name
//
// """
//
//     [<Test>]
//     let ``Produces a class explicit constructor with typed params`` () =
//         let memberNode =
//             MemberDefn.Member(
//                 BindingNode(
//                     None,
//                     None,
//                     MultipleTextsNode([ SingleTextNode("member", Range.Zero) ], Range.Zero),
//                     false,
//                     None,
//                     None,
//                     Choice1Of2(
//                         IdentListNode(
//                             [ IdentifierOrDot.Ident(SingleTextNode("this", Range.Zero))
//                               IdentifierOrDot.Ident(SingleTextNode(".", Range.Zero))
//                               IdentifierOrDot.Ident(SingleTextNode("Name", Range.Zero)) ],
//                             Range.Zero
//                         )
//                     ),
//                     None,
//                     List.Empty,
//                     None,
//                     SingleTextNode("=", Range.Zero),
//                     Expr.Constant(Constant.FromText(SingleTextNode("name", Range.Zero))),
//                     Range.Zero
//                 )
//             )
//
//         let param =
//             SimplePatNode(None, false, SingleTextNode("name", Range.Zero), Some(Type.FromString("string")), Range.Zero)
//
//         AnonymousModule() { Class("Person", [ param ]) { EscapeHatch(memberNode) } }
//         |> produces
//             """
// type Person (name: string) =
//     member this.Name = name
//
// """
//
//     [<Test>]
//     let ``Produces a class marked as a Struct explicit constructor with typed params`` () =
//         let memberNode =
//             MemberDefn.Member(
//                 BindingNode(
//                     None,
//                     None,
//                     MultipleTextsNode([ SingleTextNode("member", Range.Zero) ], Range.Zero),
//                     false,
//                     None,
//                     None,
//                     Choice1Of2(
//                         IdentListNode(
//                             [ IdentifierOrDot.Ident(SingleTextNode("this", Range.Zero))
//                               IdentifierOrDot.Ident(SingleTextNode(".", Range.Zero))
//                               IdentifierOrDot.Ident(SingleTextNode("Name", Range.Zero)) ],
//                             Range.Zero
//                         )
//                     ),
//                     None,
//                     List.Empty,
//                     None,
//                     SingleTextNode("=", Range.Zero),
//                     Expr.Constant(Constant.FromText(SingleTextNode("name", Range.Zero))),
//                     Range.Zero
//                 )
//             )
//
//         let param =
//             SimplePatNode(None, false, SingleTextNode("name", Range.Zero), Some(Type.FromString("string")), Range.Zero)
//
//         AnonymousModule() { (Class("Person", [ param ]) { EscapeHatch(memberNode) }).isStruct() }
//         |> produces
//             """
// [<Struct>]
// type Person (name: string) =
//     member this.Name = name
//
// """
//
//     [<Test>]
//     let ``Produces a class marked with multiple attributes`` () =
//         let memberNode =
//             MemberDefn.Member(
//                 BindingNode(
//                     None,
//                     None,
//                     MultipleTextsNode([ SingleTextNode("member", Range.Zero) ], Range.Zero),
//                     false,
//                     None,
//                     None,
//                     Choice1Of2(
//                         IdentListNode(
//                             [ IdentifierOrDot.Ident(SingleTextNode("this", Range.Zero))
//                               IdentifierOrDot.Ident(SingleTextNode(".", Range.Zero))
//                               IdentifierOrDot.Ident(SingleTextNode("Name", Range.Zero)) ],
//                             Range.Zero
//                         )
//                     ),
//                     None,
//                     List.Empty,
//                     None,
//                     SingleTextNode("=", Range.Zero),
//                     Expr.Constant(Constant.FromText(SingleTextNode("\"\"", Range.Zero))),
//                     Range.Zero
//                 )
//             )
//
//         AnonymousModule() {
//             (Class("Person", []) { EscapeHatch(memberNode) })
//                 .attributes([ "Sealed"; "AbstractClass" ])
//         }
//         |> produces
//             """
// [<Sealed; AbstractClass>]
// type Person () =
//     member this.Name = ""
//
// """
