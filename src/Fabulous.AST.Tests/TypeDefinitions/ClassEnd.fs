namespace Fabulous.AST.Tests.TypeDefinitions

open Fabulous.AST.Tests
open Xunit

open Fabulous.AST
open type Ast

module ClassEnd =
    [<Fact>]
    let ``Produces a class end``() =
        Oak() { AnonymousModule() { ClassEnd("MyClass") } }
        |> produces
            """
type MyClass = class end
            """

    [<Fact>]
    let ``Produces a class end with constructor``() =
        Oak() { AnonymousModule() { ClassEnd("MyClass", Constructor()) } }
        |> produces
            """
type MyClass () = class end
            """

    [<Fact>]
    let ``Produces a class end with constructor and attributes``() =
        Oak() {
            AnonymousModule() {
                ClassEnd("MyClass", Constructor())
                    .attributes([ Attribute("Sealed"); Attribute("AbstractClass") ])

            }
        }
        |> produces
            """
[<Sealed; AbstractClass>]
type MyClass () = class end
            """

    [<Fact>]
    let ``Produces a class end with constructor params``() =
        Oak() {
            AnonymousModule() {
                ClassEnd("MyClass", Constructor(ParenPat(ParameterPat("name", String()))))
                    .attributes([ Attribute("Sealed"); Attribute("AbstractClass") ])

            }
        }
        |> produces
            """
[<Sealed; AbstractClass>]
type MyClass (name: string) = class end
            """

    [<Fact>]
    let ``Produces a class end with constructor params and type args``() =
        Oak() {
            AnonymousModule() {
                ClassEnd("MyClass", Constructor(ParenPat(ParameterPat("name", String()))))
                    .attributes([ Attribute("Sealed"); Attribute("AbstractClass") ])
                    .typeParams([ "'a" ])
            }
        }
        |> produces
            """
[<Sealed; AbstractClass>]
type MyClass <'a>(name: string) = class end
            """

    [<Fact>]
    let ``Produces a class end with type params``() =
        Oak() { AnonymousModule() { ClassEnd("MyClass") |> _.typeParams([ "'a"; "'b" ]) } }
        |> produces
            """
type MyClass <'a, 'b> = class end
            """

    [<Fact>]
    let ``Produces a class end with constructor and  type params``() =
        Oak() { AnonymousModule() { ClassEnd("MyClass", Constructor()).typeParams([ "'a"; "'b" ]) } }
        |> produces
            """
type MyClass <'a, 'b>() = class end
            """
