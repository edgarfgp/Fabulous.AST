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
        Oak() { AnonymousModule() { ClassEnd("MyClass", true) } }
        |> produces
            """
type MyClass () = class end
            """

    [<Fact>]
    let ``Produces a class end with constructor and attributes``() =
        Oak() {
            AnonymousModule() {
                ClassEnd("MyClass", true).attributes() {
                    Attribute("Sealed")
                    Attribute("AbstractClass")
                }

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
                ClassEnd("MyClass", Constructor() { SimplePat("name", String(), false) })
                    .attributes() {
                    Attribute("Sealed")
                    Attribute("AbstractClass")
                }

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
                ClassEnd("MyClass", [ "'a" ], Constructor() { SimplePat("name", String(), false) })
                    .attributes() {
                    Attribute("Sealed")
                    Attribute("AbstractClass")
                }

            }
        }
        |> produces
            """
[<Sealed; AbstractClass>]
type MyClass <'a>(name: string) = class end
            """

    [<Fact>]
    let ``Produces a class end with type params``() =
        Oak() { AnonymousModule() { ClassEnd("MyClass", [ "'a"; "'b" ]) } }
        |> produces
            """
type MyClass <'a, 'b> = class end
            """

    [<Fact>]
    let ``Produces a class end with constructor and  type params``() =
        Oak() { AnonymousModule() { ClassEnd("MyClass", [ "'a"; "'b" ], true) } }
        |> produces
            """
type MyClass <'a, 'b>() = class end
            """
