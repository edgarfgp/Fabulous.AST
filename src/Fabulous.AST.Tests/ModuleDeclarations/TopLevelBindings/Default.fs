namespace Fabulous.AST.Tests.ModuleDeclarations.TopLevelBindings

open Fabulous.AST
open Fabulous.AST.Tests
open Xunit

open type Ast

module Default =

    [<Fact>]
    let ``Produces a default member with Expr``() =
        Oak() {
            AnonymousModule() {
                TypeDefn("Person", UnitPat()) {
                    AbstractMember("GetValue", [ Unit() ], String())
                    Default("this.GetValue", [ UnitPat() ], ConstantExpr(String("Hello")))
                }
            }
        }
        |> producesValid
            """
type Person() =
    abstract GetValue: unit -> string
    default this.GetValue() = "Hello"
"""

    [<Fact>]
    let ``Produces a default member with constant``() =
        Oak() {
            AnonymousModule() {
                TypeDefn("Person", UnitPat()) {
                    AbstractMember("GetValue", [ Unit() ], String())
                    Default("this.GetValue", [ UnitPat() ], String("Hello"))
                }
            }
        }
        |> producesValid
            """
type Person() =
    abstract GetValue: unit -> string
    default this.GetValue() = "Hello"
"""

    [<Fact>]
    let ``Produces a default member with constant string``() =
        Oak() {
            AnonymousModule() {
                TypeDefn("Person", UnitPat()) {
                    AbstractMember("GetValue", [ Unit() ], String())
                    Default("this.GetValue", [ "()" ], String("Hello, World!"))

                }
            }
        }
        |> producesValid
            """
type Person() =
    abstract GetValue: unit -> string
    default this.GetValue () = "Hello, World!"
"""
