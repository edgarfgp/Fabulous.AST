namespace Fabulous.AST.Tests.TypeDefinitions

open Fabulous.AST.Tests
open Xunit

open Fabulous.AST
open type Ast

module Delegate =
    [<Fact>]
    let ``Produces a delegate``() =

        Oak() {
            AnonymousModule() {
                Delegate("Delegate1", [ Paren(Tuple([ Int(); Int() ])); Paren(Tuple([ Int(); Int() ])) ], Int())
                Delegate("Delegate2", Paren(Tuple([ Int(); Int() ])), Int())
                Delegate("Delegate3", SignatureParameter("a", Paren(Tuple([ Int(); Int() ]))), Int())

                Delegate("Delegate4", Tuple([ SignatureParameter("a", Int()); SignatureParameter("b", Int()) ]), Int())

                Delegate("Delegate5", [ "int"; "int" ], Int())
                Delegate("Delegate6", [ "int"; "int" ], "int")
                Delegate("Delegate7", Int(), Int())
            }
        }
        |> produces
            """
type Delegate1 = delegate of (int * int) * (int * int) -> int
type Delegate2 = delegate of (int * int) -> int
type Delegate3 = delegate of a: (int * int) -> int
type Delegate4 = delegate of a: int * b: int -> int
type Delegate5 = delegate of int * int -> int
type Delegate6 = delegate of int * int -> int
type Delegate7 = delegate of int -> int
"""

    [<Fact>]
    let ``yield! multiple delegates``() =

        Oak() {
            AnonymousModule() {
                yield!
                    [ Delegate("Delegate1", Paren(Tuple([ Int(); Int() ])), Int())
                      Delegate("Delegate2", SignatureParameter("a", Paren(Tuple([ Int(); Int() ]))), Int())
                      Delegate(
                          "Delegate3",
                          Tuple([ SignatureParameter("a", Int()); SignatureParameter("b", Int()) ]),
                          Int()
                      ) ]
            }
        }
        |> produces
            """
type Delegate1 = delegate of (int * int) -> int
type Delegate2 = delegate of a: (int * int) -> int
type Delegate3 = delegate of a: int * b: int -> int
"""

    [<Fact>]
    let ``Produces a delegate with attributes``() =
        Oak() { AnonymousModule() { Delegate("Delegate", "int", "int").attributes([ Attribute("Obsolete") ]) } }
        |> produces
            """
[<Obsolete>]
type Delegate = delegate of int -> int
"""

    [<Fact>]
    let ``Produces a delegate with attribute``() =
        Oak() { AnonymousModule() { Delegate("Delegate", "int", "int").attribute(Attribute("Obsolete")) } }
        |> produces
            """
[<Obsolete>]
type Delegate = delegate of int -> int
"""

    [<Fact>]
    let ``Produces a delegate with access modifiers``() =
        Oak() {
            AnonymousModule() {
                Delegate("Delegate1", "int", "int").toPublic()
                Delegate("Delegate2", "int", "int").toPrivate()
                Delegate("Delegate3", "int", "int").toInternal()
            }
        }
        |> produces
            """
type public Delegate1 = delegate of int -> int
type private Delegate2 = delegate of int -> int
type internal Delegate3 = delegate of int -> int
"""

    [<Fact>]
    let ``Produces a delegate with documentation``() =
        Oak() { AnonymousModule() { Delegate("Delegate", "int", "int").xmlDocs([ "This is a delegate" ]) } }
        |> produces
            """
/// This is a delegate
type Delegate = delegate of int -> int
"""
