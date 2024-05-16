namespace Fabulous.AST.Tests.TypeDefinitions

open Fabulous.AST.Tests
open Xunit

open Fabulous.AST
open type Ast

module TypeDefnExplicit =
    [<Fact>]
    let ``Produces a class end``() =
        Oak() { AnonymousModule() { ClassEnd("MyClass") } }
        |> produces
            """
type MyClass = class end
            """

    [<Fact>]
    let ``Produces a class end with constructor``() =
        Oak() { AnonymousModule() { ClassEnd("MyClass", ImplicitConstructor()) } }
        |> produces
            """
type MyClass() = class end
            """

    [<Fact>]
    let ``Produces a class end with constructor and attributes``() =
        Oak() {
            AnonymousModule() {
                ClassEnd("MyClass", ImplicitConstructor())
                    .attributes([ Attribute("Sealed"); Attribute("AbstractClass") ])

            }
        }
        |> produces
            """
[<Sealed; AbstractClass>]
type MyClass() = class end
            """

    [<Fact>]
    let ``Produces a class end with constructor params``() =
        Oak() {
            AnonymousModule() {
                ClassEnd(
                    "MyClass",
                    ImplicitConstructor(ParenPat(ParameterPat(ConstantPat(Constant("name")), String())))
                )
                    .attributes([ Attribute("Sealed"); Attribute("AbstractClass") ])

            }
        }
        |> produces
            """
[<Sealed; AbstractClass>]
type MyClass(name: string) = class end
            """

    [<Fact>]
    let ``Produces a class end with constructor params and type args``() =
        Oak() {
            AnonymousModule() {
                ClassEnd(
                    "MyClass",
                    ImplicitConstructor(ParenPat(ParameterPat(ConstantPat(Constant("name")), String())))
                )
                    .attributes([ Attribute("Sealed"); Attribute("AbstractClass") ])
                    .typeParams(PostfixList([ "'a" ]))
            }
        }
        |> produces
            """
[<Sealed; AbstractClass>]
type MyClass<'a>(name: string) = class end
            """

    [<Fact>]
    let ``Produces a class end with type params``() =
        Oak() { AnonymousModule() { ClassEnd("MyClass") |> _.typeParams(PostfixList([ "'a"; "'b" ])) } }
        |> produces
            """
type MyClass<'a, 'b> = class end
            """

    [<Fact>]
    let ``Produces a class end with constructor and  type params``() =
        Oak() {
            AnonymousModule() {
                ClassEnd("MyClass", ImplicitConstructor())
                    .typeParams(PostfixList([ "'a"; "'b" ]))
            }
        }
        |> produces
            """
type MyClass<'a, 'b>() = class end
            """

module StructEnd =
    [<Fact>]
    let ``Produces a struct end empty constructor``() =
        Oak() { AnonymousModule() { StructEnd("MyClass", ImplicitConstructor()) } }
        |> produces
            """
type MyClass() = struct end
            """

    [<Fact>]
    let ``Produces a struct end non empty constructor``() =
        Oak() { AnonymousModule() { StructEnd("MyClass", ImplicitConstructor(ParenPat(ParameterPat("a", String())))) } }
        |> produces
            """
type MyClass(a: string) = struct end
            """

module InterfaceEnd =
    [<Fact>]
    let ``Produces an interface end``() =
        Oak() { AnonymousModule() { InterfaceEnd("IFoo") } }
        |> produces
            """
type IFoo = interface end
                    """
