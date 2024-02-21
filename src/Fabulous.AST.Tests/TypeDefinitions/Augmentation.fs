namespace Fabulous.AST.Tests.TypeDefinitions

open Fabulous.AST.Tests
open NUnit.Framework

open Fabulous.AST
open type Ast

module Augmentation =
    [<Test>]
    let ``Produces an Augment`` () =
        AnonymousModule() {
            Augmentation("DateTime") {
                Member("this.Print", ConstantExpr(ConstantUnit()))
                Member("this.Print", UnitPat(), ConstantExpr(ConstantUnit()))
            }
        }
        |> produces
            """
type DateTime with
    member this.Print = ()
    member this.Print() = ()

 """
