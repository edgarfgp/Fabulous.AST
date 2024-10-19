namespace Fabulous.AST.Tests.TypeDefinitions

open Fabulous.AST.Tests
open Xunit

open Fabulous.AST
open type Ast

module Augmentation =
    [<Fact>]
    let ``Produces an Augment``() =
        Oak() {
            AnonymousModule() {
                Augmentation("DateTime") {
                    Property(ConstantPat(Constant("this.Print")), ConstantExpr(ConstantUnit()))
                    Method("this.Print", UnitPat(), ConstantExpr(ConstantUnit()))
                }
            }
        }
        |> produces
            """
type DateTime with
    member this.Print = ()
    member this.Print() = ()

 """
    [<Fact>]
    let ``Produces an Augmentation with attributes``() =
        Oak() {
            AnonymousModule() {
                Open("Microsoft.FSharp.Core")

                Class("A",
                      ImplicitConstructor(
                          ParenPat(
                              ParameterPat(
                                  "x",
                                  AppPrefix(
                                      Int(),
                                      "'u")
                                  )
                              )
                          )
                      ) {
                    Property("_.X", ConstantExpr("x"))
                }
                |> _.typeParams(PostfixList(TyparDecl("'u").attribute(Attribute "Measure")))

                NestedModule("M") {
                    Augmentation("A") {
                        Property(ConstantPat(Constant("this.Y")), ConstantExpr("this.X"))
                    }
                    |> _.typeParams(PostfixList(TyparDecl("'u").attribute(Attribute "Measure")))
                }

                Function("main", "argv", Constant("0"))
                    .attribute(Attribute("EntryPoint"))
            }
        }
        |> produces
            """
open Microsoft.FSharp.Core

type A<[<Measure>] 'u>(x: int<'u>) =
    member _.X = x

module M =
    type A<[<Measure>] 'u> with
        member this.Y = this.X

[<EntryPoint>]
let main argv = 0
 """
