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
                    Member(ConstantPat(Constant("this.Print")), ConstantExpr(ConstantUnit()))
                    Member("this.Print", UnitPat(), ConstantExpr(ConstantUnit()))
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
    let ``Produces an augmentation with type parameters - measure``() =
        Oak() {
            AnonymousModule() {
                Open("Microsoft.FSharp.Core")

                TypeDefn("A", Constructor(ParameterPat("x", AppPrefix(Int(), "'u")))) {
                    Member("_.X", ConstantExpr("x"))
                }
                |> _.typeParams(PostfixList(TyparDecl("'u").attribute(Attribute "Measure")))

                Module("M") {
                    Augmentation("A") { Member(ConstantPat(Constant("this.Y")), ConstantExpr("this.X")) }
                    |> _.typeParams(PostfixList(TyparDecl("'u").attribute(Attribute "Measure")))
                }

                Function("main", "argv", Constant("0")).attribute(Attribute("EntryPoint"))
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

    [<Fact>]
    let ``Produces an augmentation with accessors``() =
        Oak() { AnonymousModule() { Augmentation("A") { Member("this.Y", "this.X") } |> _.toPrivate() } }
        |> produces
            """
type private A with
    member this.Y = this.X
 """

    [<Fact>]
    let ``Produces an augmentation with xml docs``() =
        Oak() {
            AnonymousModule() {
                Augmentation("A") { Member("this.Y", "this.X") }
                |> _.xmlDocs([ "This is a test" ])
            }
        }
        |> produces
            """
/// This is a test
type A with
    member this.Y = this.X
 """

    [<Fact>]
    let ``Produces an augmentation with attributes``() =
        Oak() {
            AnonymousModule() {
                Augmentation("A") { Member("this.Y", "this.X") }
                |> _.attributes([ Attribute("Test") ])
            }
        }
        |> produces
            """
[<Test>]
type A with
    member this.Y = this.X
 """

    [<Fact>]
    let ``Produces an augmentation with constraints``() =
        Oak() {
            AnonymousModule() {
                Augmentation("A") { Member("this.Y", "this.X") }
                |> _.typeParams(PostfixList(TyparDecl("'T"), ConstraintSingle("'T", "equality")))
            }
        }
        |> produces
            """
type A<'T when 'T: equality> with
    member this.Y = this.X
 """

    [<Fact>]
    let ``Produces an augmentation with type params constraints``() =
        Oak() {
            AnonymousModule() {
                Augmentation("A") { Member("this.Y", "this.X") }
                |> _.typeParams(PostfixList(TyparDecl("'T"), ConstraintSingle("'T", "equality")))
            }
        }
        |> produces
            """
type A<'T when 'T: equality> with
    member this.Y = this.X
 """

    [<Fact>]
    let ``Produces an augmentation with a type that you have not defined yourself``() =
        Oak() {
            Namespace("Extensions") {
                Augmentation("IEnumerable") {
                    Member(
                        "xs.RepeatElements",
                        ParenPat(ParameterPat("n", Int())),
                        SeqExpr(ForEachDoExpr("x", "xs", ForEachArrowExpr("_", "1 .. n", "x")))
                    )
                        .xmlDocs([ "Repeat each element of the sequence n times" ])
                }
                |> _.typeParams(PostfixList(TyparDecl("'T")))
            }
            |> _.toImplicit()
        }
        |> produces
            """
module Extensions

type IEnumerable<'T> with
    /// Repeat each element of the sequence n times
    member xs.RepeatElements(n: int) =
        seq {
            for x in xs do
                for _ in 1 .. n -> x
        }
 """
