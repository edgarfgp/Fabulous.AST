namespace Fabulous.AST.Tests.TypeDefinitions

open Fabulous.AST.Tests
open Xunit

open Fabulous.AST
open type Ast

module Augmentation =
    [<Fact>]
    let ``Produces an Augment``() =
        AnonymousModule() {
            Augmentation("DateTime") {
                Property("this.Print", ConstantExpr(ConstantUnit()))
                Method("this.Print", UnitPat(), ConstantExpr(ConstantUnit()))
            }
        }
        |> produces
            """
type DateTime with
    member this.Print = ()
    member this.Print() = ()

 """
