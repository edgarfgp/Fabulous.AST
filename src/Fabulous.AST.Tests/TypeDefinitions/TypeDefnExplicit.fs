namespace Fabulous.AST.Tests.TypeDefinitions

open Fabulous.AST.Tests
open Xunit

open Fabulous.AST
open type Ast

module TypeDefnExplicit =
    [<Fact>]
    let ``Produces a class end``() =
        Oak() {
            AnonymousModule() {
                ClassEnd("MyClass") { () }

                ClassEnd("MyClass2") { Member("this.Name", UnitPat(), UnitExpr()) }

                ClassEnd("MyClass3", Constructor(ParameterPat("name", String()))) {
                    Member("this.Name", UnitPat(), String("name"))
                }
            }
        }
        |> producesValid
            """
type MyClass = class end

type MyClass2 =
    class
        member this.Name() = ()
    end

type MyClass3(name: string) =
    class
        member this.Name() = "name"
    end
            """

    [<Fact>]
    let ``Produces a class end with constructor``() =
        Oak() { AnonymousModule() { ClassEnd("MyClass", UnitPat()) { () } } }
        |> producesValid
            """
type MyClass() = class end
            """

    [<Fact>]
    let ``Produces a class end with constructor and attributes``() =
        Oak() {
            AnonymousModule() {
                (ClassEnd("MyClass") { () }).attributes([ Attribute("Sealed"); Attribute("AbstractClass") ])

            }
        }
        |> producesValid
            """
[<Sealed; AbstractClass>]
type MyClass = class end
            """

    [<Fact>]
    let ``Produces a class end with constructor params``() =
        Oak() {
            AnonymousModule() {
                (ClassEnd("MyClass", Constructor(ParameterPat(ConstantPat(Constant("name")), String()))) { () })
                    .attributes([ Attribute("Sealed"); Attribute("AbstractClass") ])

            }
        }
        |> producesValid
            """
[<Sealed; AbstractClass>]
type MyClass(name: string) = class end
            """

    [<Fact>]
    let ``Produces a class end with constructor params and type args``() =
        Oak() {
            AnonymousModule() {
                (ClassEnd("MyClass", Constructor(ParameterPat(ConstantPat(Constant("name")), String()))) { () })
                    .attributes([ Attribute("Sealed"); Attribute("AbstractClass") ])
                    .typeParams(PostfixList([ "'a" ]))
            }
        }
        |> producesValid
            """
[<Sealed; AbstractClass>]
type MyClass<'a>(name: string) = class end
            """

    [<Fact>]
    let ``Produces a class end with type params``() =
        Oak() { AnonymousModule() { ClassEnd("MyClass") { () } |> _.typeParams(PostfixList([ "'a"; "'b" ])) } }
        |> producesValid
            """
type MyClass<'a, 'b> = class end
            """

    [<Fact>]
    let ``Produces a class end with constructor and  type params``() =
        Oak() { AnonymousModule() { (ClassEnd("MyClass", UnitPat()) { () }).typeParams(PostfixList([ "'a"; "'b" ])) } }
        |> producesValid
            """
type MyClass<'a, 'b>() = class end
            """

module StructEnd =
    [<Fact>]
    let ``Produces a struct end empty constructor``() =
        Oak() { AnonymousModule() { StructEnd("MyClass", UnitPat()) { () } } }
        |> producesValid
            """
type MyClass() = struct end
            """

    [<Fact>]
    let ``Produces a struct end non empty constructor``() =
        Oak() {
            AnonymousModule() {
                StructEnd("MyClass", ParameterPat(ConstantPat(Constant("a")), String())) { () }

                StructEnd("Y", Constructor(ParameterPat("a", Int()))) {
                    ValField("x", Int()).toMutable()
                    Constructor("x", RecordExpr([ RecordFieldExpr("x", "x") ]))
                }
            }
        }
        |> producesValid
            """
type MyClass(a: string) = struct end

type Y(a: int) =
    struct
        val mutable x: int
        new(x) = { x = x }
    end
            """

    [<Fact>]
    let ``Produces a struct end with private ValField``() =
        Oak() {
            AnonymousModule() {
                StructEnd("Point", Constructor(ParameterPat("x", Int()))) {
                    ValField("x", Int()).toMutable().toPrivate()
                }
            }
        }
        |> producesValid
            """
type Point(x: int) =
    struct
        val mutable private x: int
    end
            """

module InterfaceEnd =
    [<Fact>]
    let ``Produces an interface end``() =
        Oak() { AnonymousModule() { InterfaceEnd("IFoo") { () } } }
        |> producesValid
            """
type IFoo = interface end
                    """

    [<Fact>]
    let ``Produces an interface with members``() =
        Oak() { AnonymousModule() { InterfaceEnd("IMarker") { AbstractMember("Name", String()) } } }
        |> producesValid
            """
type IMarker =
    interface
        abstract Name: string
    end
                    """
