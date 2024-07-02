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
                Delegate("Delegate1", Paren(Tuple([ Int(); Int() ])), Int())
                Delegate("Delegate2", SignatureParameter("a", Paren(Tuple([ Int(); Int() ]))), Int())
                Delegate("Delegate3", Tuple([ SignatureParameter("a", Int()); SignatureParameter("b", Int()) ]), Int())
            }
        }
        |> produces
            """
type Delegate1 = delegate of (int * int) -> int
type Delegate2 = delegate of a: (int * int) -> int
type Delegate3 = delegate of a: int * b: int -> int
"""

    [<Fact>]
    let ``yield! multiple delegates``() =

        Oak() {
            AnonymousModule() {
                yield!
                    [ AnyModuleDecl(Delegate("Delegate1", Paren(Tuple([ Int(); Int() ])), Int()))
                      AnyModuleDecl(
                          Delegate("Delegate2", SignatureParameter("a", Paren(Tuple([ Int(); Int() ]))), Int())
                      )
                      AnyModuleDecl(
                          Delegate(
                              "Delegate3",
                              Tuple([ SignatureParameter("a", Int()); SignatureParameter("b", Int()) ]),
                              Int()
                          )
                      ) ]
            }
        }
        |> produces
            """
type Delegate1 = delegate of (int * int) -> int
type Delegate2 = delegate of a: (int * int) -> int
type Delegate3 = delegate of a: int * b: int -> int
"""
