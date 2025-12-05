namespace Fabulous.AST.Tests.MethodDefinitions

open Fabulous.AST
open Fabulous.AST.Tests
open Microsoft.FSharp.Core
open Xunit

open type Ast

module MemberDefn =
    [<Fact>]
    let ``yield! multiple MemberDefns``() =
        Oak() {
            AnonymousModule() {
                TypeDefn("MyClass", UnitPat()) {
                    Member("this.Name", ConstantExpr(String "John"))
                    Member("this.Age", ConstantExpr(Int 30))
                    Member("this.City", ConstantExpr(String "NYC"))
                }
            }
        }
        |> produces
            """
type MyClass() =
    member this.Name = "John"
    member this.Age = 30
    member this.City = "NYC"
"""

    [<Fact>]
    let ``yield! multiple abstract members``() =
        Oak() {
            AnonymousModule() {
                TypeDefn("IMyInterface") {
                    yield!
                        [ AbstractMember("GetName", [ Unit() ], String())
                          AbstractMember("GetAge", [ Unit() ], Int())
                          AbstractMember("GetCity", [ Unit() ], String()) ]
                }
            }
        }
        |> produces
            """
type IMyInterface =
    abstract GetName: unit -> string
    abstract GetAge: unit -> int
    abstract GetCity: unit -> string
"""

    [<Fact>]
    let ``yield! multiple auto properties``() =
        Oak() {
            AnonymousModule() {
                TypeDefn("MyClass", UnitPat()) {
                    yield!
                        [ MemberVal("Name", ConstantExpr(String "John"), true)
                          MemberVal("Age", ConstantExpr(Int 30), true)
                          MemberVal("City", ConstantExpr(String "NYC"), true) ]
                }
            }
        }
        |> produces
            """
type MyClass() =
    member val Name = "John" with get
    member val Age = 30 with get
    member val City = "NYC" with get
"""
