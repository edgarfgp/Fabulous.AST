namespace Fabulous.AST.Tests.Patterns

open Xunit
open Fabulous.AST.Tests

open Fabulous.AST

open type Ast

type Person public (name: string) =
    member this.Name = name

module ImplicitConstructor =

    [<Fact>]
    let ``Constructor with private accessibility``() =
        Oak() {
            AnonymousModule() {
                TypeDefn("Person", Constructor(ParameterPat("name", String())).toPrivate()) {
                    Member("this.Name", ConstantExpr("name"))
                }
            }
        }
        |> produces
            """
type Person private (name: string) =
    member this.Name = name
"""

    [<Fact>]
    let ``Constructor with internal accessibility``() =
        Oak() {
            AnonymousModule() {
                TypeDefn("Person", Constructor(ParameterPat("name", String())).toInternal()) {
                    Member("this.Name", ConstantExpr("name"))
                }
            }
        }
        |> produces
            """
type Person internal (name: string) =
    member this.Name = name
"""

    [<Fact>]
    let ``Constructor with public accessibility``() =
        Oak() {
            AnonymousModule() {
                TypeDefn("Person", Constructor(ParameterPat("name", String())).toPublic()) {
                    Member("this.Name", ConstantExpr("name"))
                }
            }
        }
        |> produces
            """
type Person public (name: string) =
    member this.Name = name
"""

    [<Fact>]
    let ``Constructor with alias``() =
        Oak() {
            AnonymousModule() {
                TypeDefn("Person", Constructor(ParameterPat("name", String())).alias("self")) {
                    Member("self.Name", ConstantExpr("name"))
                }
            }
        }
        |> produces
            """
type Person(name: string) as self =
    member self.Name = name
"""

    [<Fact>]
    let ``Constructor with attribute``() =
        Oak() {
            AnonymousModule() {
                TypeDefn("Person", Constructor(ParameterPat("name", String())).attribute(Attribute("Obsolete"))) {
                    Member("this.Name", ConstantExpr("name"))
                }
            }
        }
        |> produces
            """
type Person [<Obsolete>] (name: string) =
    member this.Name = name
"""

    [<Fact>]
    let ``Constructor with xmlDocs``() =
        Oak() {
            AnonymousModule() {
                TypeDefn("Person", Constructor(ParameterPat("name", String())).xmlDocs([ "Creates a new Person" ])) {
                    Member("this.Name", ConstantExpr("name"))
                }
            }
        }
        |> produces
            """
type Person
    /// Creates a new Person
    (name: string) =
    member this.Name = name
"""
