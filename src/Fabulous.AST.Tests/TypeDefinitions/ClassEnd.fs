namespace Fabulous.AST.Tests.TypeDefinitions

open Fabulous.AST.Tests
open NUnit.Framework

open Fabulous.AST
open type Ast

module ClassEnd =
    [<Test>]
    let ``Produces a class end`` () =
        AnonymousModule() { ClassEnd("MyClass") }
        |> produces
            """
type MyClass = class end
            """

    [<Test>]
    let ``Produces a class end with constructor`` () =
        AnonymousModule() { ClassEnd("MyClass").implicitConstructorParameters([]) }
        |> produces
            """
type MyClass () = class end
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
type MyClass () = class end
            """

    [<Test>]
    let ``Produces a class end with type params`` () =
        AnonymousModule() { ClassEnd("MyClass", [ "'a"; "'b" ]) }
        |> produces
            """
type MyClass <'a, 'b> = class end
            """

    [<Test>]
    let ``Produces a class end with constructor and  type params`` () =
        AnonymousModule() {
            ClassEnd("MyClass", [ "'a"; "'b" ])
                .implicitConstructorParameters([])
        }
        |> produces
            """
type MyClass <'a, 'b>() = class end
            """
