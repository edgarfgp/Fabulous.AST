namespace Fabulous.AST.Tests.TypeDefinitions

open Fantomas.FCS.Text
open Fabulous.AST.Tests
open Fantomas.Core.SyntaxOak
open NUnit.Framework

open Fabulous.AST
open type Ast

module Class =
    [<Test>]
    let ``Produces a class implicit constructor`` () =
        let memberNode =
            MemberDefn.Member(
                BindingNode(
                    None,
                    None,
                    MultipleTextsNode([ SingleTextNode("member", Range.Zero) ], Range.Zero),
                    false,
                    None,
                    None,
                    Choice1Of2(
                        IdentListNode(
                            [ IdentifierOrDot.Ident(SingleTextNode("this", Range.Zero))
                              IdentifierOrDot.Ident(SingleTextNode(".", Range.Zero))
                              IdentifierOrDot.Ident(SingleTextNode("Name", Range.Zero)) ],
                            Range.Zero
                        )
                    ),
                    None,
                    List.Empty,
                    None,
                    SingleTextNode("=", Range.Zero),
                    Expr.Constant(Constant.FromText(SingleTextNode("name", Range.Zero))),
                    Range.Zero
                )
            )

        AnonymousModule() { Class("Person") { EscapeHatch(memberNode) } }
        |> produces
            """
type Person =
    member this.Name = name

"""

    [<Test>]
    let ``Produces a class explicit constructor with no params`` () =
        let memberNode =
            MemberDefn.Member(
                BindingNode(
                    None,
                    None,
                    MultipleTextsNode([ SingleTextNode("member", Range.Zero) ], Range.Zero),
                    false,
                    None,
                    None,
                    Choice1Of2(
                        IdentListNode(
                            [ IdentifierOrDot.Ident(SingleTextNode("this", Range.Zero))
                              IdentifierOrDot.Ident(SingleTextNode(".", Range.Zero))
                              IdentifierOrDot.Ident(SingleTextNode("Name", Range.Zero)) ],
                            Range.Zero
                        )
                    ),
                    None,
                    List.Empty,
                    None,
                    SingleTextNode("=", Range.Zero),
                    Expr.Constant(Constant.FromText(SingleTextNode("\"\"", Range.Zero))),
                    Range.Zero
                )
            )

        AnonymousModule() { (Class("Person") { EscapeHatch(memberNode) }).implicitConstructorParameters([]) }
        |> produces
            """
type Person () =
    member this.Name = ""

"""

    [<Test>]
    let ``Produces a class explicit constructor with params`` () =
        let memberNode =
            MemberDefn.Member(
                BindingNode(
                    None,
                    None,
                    MultipleTextsNode([ SingleTextNode("member", Range.Zero) ], Range.Zero),
                    false,
                    None,
                    None,
                    Choice1Of2(
                        IdentListNode(
                            [ IdentifierOrDot.Ident(SingleTextNode("this", Range.Zero))
                              IdentifierOrDot.Ident(SingleTextNode(".", Range.Zero))
                              IdentifierOrDot.Ident(SingleTextNode("Name", Range.Zero)) ],
                            Range.Zero
                        )
                    ),
                    None,
                    List.Empty,
                    None,
                    SingleTextNode.equals,
                    Expr.Constant(Constant.FromText(SingleTextNode("name", Range.Zero))),
                    Range.Zero
                )
            )

        let param =
            [ "name"; "lastName"; "age" ]
            |> List.map(fun n -> SimplePatNode(None, false, SingleTextNode(n, Range.Zero), None, Range.Zero))

        AnonymousModule() {
            (Class("Person") { EscapeHatch(memberNode) })
                .implicitConstructorParameters(param)
        }
        |> produces
            """
type Person (name, lastName, age) =
    member this.Name = name

"""

    [<Test>]
    let ``Produces a class explicit constructor with typed params`` () =
        let memberNode =
            MemberDefn.Member(
                BindingNode(
                    None,
                    None,
                    MultipleTextsNode([ SingleTextNode("member", Range.Zero) ], Range.Zero),
                    false,
                    None,
                    None,
                    Choice1Of2(
                        IdentListNode(
                            [ IdentifierOrDot.Ident(SingleTextNode("this", Range.Zero))
                              IdentifierOrDot.Ident(SingleTextNode(".", Range.Zero))
                              IdentifierOrDot.Ident(SingleTextNode("Name", Range.Zero)) ],
                            Range.Zero
                        )
                    ),
                    None,
                    List.Empty,
                    None,
                    SingleTextNode("=", Range.Zero),
                    Expr.Constant(Constant.FromText(SingleTextNode("name", Range.Zero))),
                    Range.Zero
                )
            )

        let param =
            [ ("name", "string"); ("lastName", "string"); ("age", "int") ]
            |> List.map(fun n ->
                let isOpt = fst n = "age"

                SimplePatNode(
                    None,
                    isOpt,
                    SingleTextNode(fst n, Range.Zero),
                    Some(Type.FromString($"{snd n}")),
                    Range.Zero
                ))

        AnonymousModule() {
            (Class("Person") { EscapeHatch(memberNode) })
                .implicitConstructorParameters(param)
        }
        |> produces
            """
type Person (name: string, lastName: string, ?age: int) =
    member this.Name = name
"""

    [<Test>]
    let ``Produces a class explicit constructor with multiple typed params`` () =
        let expr = Expr.Constant(Constant.FromText(SingleTextNode("name", Range.Zero)))

        let param =
            [ SimplePatNode(
                  None,
                  false,
                  SingleTextNode("name", Range.Zero),
                  Some(Type.FromString("string")),
                  Range.Zero
              )
              SimplePatNode(None, false, SingleTextNode("age", Range.Zero), Some(Type.FromString("int")), Range.Zero) ]

        AnonymousModule() {
            (Class("Person") { PropertyMember("this.Name") { EscapeHatch(expr) } })
                .implicitConstructorParameters(param)
        }
        |> produces
            """
type Person (name: string, age: int) =
    member this.Name = name

"""

    [<Test>]
    let ``Produces a class marked as a Struct explicit constructor with typed params`` () =
        let memberNode =
            MemberDefn.Member(
                BindingNode(
                    None,
                    None,
                    MultipleTextsNode([ SingleTextNode("member", Range.Zero) ], Range.Zero),
                    false,
                    None,
                    None,
                    Choice1Of2(
                        IdentListNode(
                            [ IdentifierOrDot.Ident(SingleTextNode("this", Range.Zero))
                              IdentifierOrDot.Ident(SingleTextNode(".", Range.Zero))
                              IdentifierOrDot.Ident(SingleTextNode("Name", Range.Zero)) ],
                            Range.Zero
                        )
                    ),
                    None,
                    List.Empty,
                    None,
                    SingleTextNode("=", Range.Zero),
                    Expr.Constant(Constant.FromText(SingleTextNode("name", Range.Zero))),
                    Range.Zero
                )
            )

        let param =
            SimplePatNode(None, false, SingleTextNode("name", Range.Zero), Some(Type.FromString("string")), Range.Zero)

        AnonymousModule() {
            (Class("Person") { EscapeHatch(memberNode) })
                .isStruct()
                .implicitConstructorParameters([ param ])
        }
        |> produces
            """
[<Struct>]
type Person (name: string) =
    member this.Name = name

"""

    [<Test>]
    let ``Produces a class marked with multiple attributes`` () =
        let memberNode =
            MemberDefn.Member(
                BindingNode(
                    None,
                    None,
                    MultipleTextsNode([ SingleTextNode("member", Range.Zero) ], Range.Zero),
                    false,
                    None,
                    None,
                    Choice1Of2(
                        IdentListNode(
                            [ IdentifierOrDot.Ident(SingleTextNode("this", Range.Zero))
                              IdentifierOrDot.Ident(SingleTextNode(".", Range.Zero))
                              IdentifierOrDot.Ident(SingleTextNode("Name", Range.Zero)) ],
                            Range.Zero
                        )
                    ),
                    None,
                    List.Empty,
                    None,
                    SingleTextNode("=", Range.Zero),
                    Expr.Constant(Constant.FromText(SingleTextNode("\"\"", Range.Zero))),
                    Range.Zero
                )
            )

        AnonymousModule() {
            (Class("Person") { EscapeHatch(memberNode) })
                .attributes([ "Sealed"; "AbstractClass" ])
                .implicitConstructorParameters([])
        }
        |> produces
            """
[<Sealed; AbstractClass>]
type Person () =
    member this.Name = ""

"""

    [<Test>]
    let ``Produces a generic class`` () =
        let memberNode =
            MemberDefn.Member(
                BindingNode(
                    None,
                    None,
                    MultipleTextsNode([ SingleTextNode("member", Range.Zero) ], Range.Zero),
                    false,
                    None,
                    None,
                    Choice1Of2(
                        IdentListNode(
                            [ IdentifierOrDot.Ident(SingleTextNode("this", Range.Zero))
                              IdentifierOrDot.Ident(SingleTextNode(".", Range.Zero))
                              IdentifierOrDot.Ident(SingleTextNode("Name", Range.Zero)) ],
                            Range.Zero
                        )
                    ),
                    None,
                    List.Empty,
                    None,
                    SingleTextNode("=", Range.Zero),
                    Expr.Constant(Constant.FromText(SingleTextNode("\"\"", Range.Zero))),
                    Range.Zero
                )
            )

        AnonymousModule() {
            GenericClass("Person", [ "'a"; "'b" ]) { EscapeHatch(memberNode) }

        }
        |> produces
            """
type Person <'a, 'b> =
    member this.Name = ""

"""

    [<Test>]
    let ``Produces a generic class with a constructor`` () =
        let memberNode =
            MemberDefn.Member(
                BindingNode(
                    None,
                    None,
                    MultipleTextsNode([ SingleTextNode("member", Range.Zero) ], Range.Zero),
                    false,
                    None,
                    None,
                    Choice1Of2(
                        IdentListNode(
                            [ IdentifierOrDot.Ident(SingleTextNode("this", Range.Zero))
                              IdentifierOrDot.Ident(SingleTextNode(".", Range.Zero))
                              IdentifierOrDot.Ident(SingleTextNode("Name", Range.Zero)) ],
                            Range.Zero
                        )
                    ),
                    None,
                    List.Empty,
                    None,
                    SingleTextNode("=", Range.Zero),
                    Expr.Constant(Constant.FromText(SingleTextNode("\"\"", Range.Zero))),
                    Range.Zero
                )
            )

        AnonymousModule() {
            (GenericClass("Person", [ "'a"; "'b" ]) { EscapeHatch(memberNode) })
                .implicitConstructorParameters([])

        }
        |> produces
            """
type Person <'a, 'b>() =
    member this.Name = ""

"""

    [<Test>]
    let ``Produces a struct generic class with a constructor`` () =
        let memberNode =
            MemberDefn.Member(
                BindingNode(
                    None,
                    None,
                    MultipleTextsNode([ SingleTextNode("member", Range.Zero) ], Range.Zero),
                    false,
                    None,
                    None,
                    Choice1Of2(
                        IdentListNode(
                            [ IdentifierOrDot.Ident(SingleTextNode("this", Range.Zero))
                              IdentifierOrDot.Ident(SingleTextNode(".", Range.Zero))
                              IdentifierOrDot.Ident(SingleTextNode("Name", Range.Zero)) ],
                            Range.Zero
                        )
                    ),
                    None,
                    List.Empty,
                    None,
                    SingleTextNode("=", Range.Zero),
                    Expr.Constant(Constant.FromText(SingleTextNode("\"\"", Range.Zero))),
                    Range.Zero
                )
            )

        AnonymousModule() {
            (GenericClass("Person", [ "'a"; "'b" ]) { EscapeHatch(memberNode) })
                .isStruct()
                .implicitConstructorParameters([])

        }
        |> produces
            """
[<Struct>]
type Person <'a, 'b>() =
    member this.Name = ""

"""

    [<Test>]
    let ``Produces a class end`` () =
        AnonymousModule() { ClassEnd("MyClass") }
        |> produces
            """
type MyClass =
    class
    end
            """

    [<Test>]
    let ``Produces a class end with constructor`` () =
        AnonymousModule() { ClassEnd("MyClass").implicitConstructorParameters([]) }
        |> produces
            """
type MyClass () =
    class
    end
            """

    [<Test>]
    let ``Produces a class end with constructor and attributes`` () =
        AnonymousModule() {
            ClassEnd("MyClass")
                .attributes([ "Sealed"; "AbstractClass" ])
                .implicitConstructorParameters([])
        }
        |> produces
            """
[<Sealed; AbstractClass>]
type MyClass () =
    class
    end
            """

    [<Test>]
    let ``Produces a class end with type params`` () =
        AnonymousModule() { ClassEnd("MyClass", [ "'a"; "'b" ]) }
        |> produces
            """
type MyClass <'a, 'b> =
    class
    end
            """

    [<Test>]
    let ``Produces a class end with constructor and  type params`` () =
        AnonymousModule() { ClassEnd("MyClass", [ "'a"; "'b" ]).implicitConstructorParameters([]) }
        |> produces
            """
type MyClass <'a, 'b>() =
    class
    end
            """
