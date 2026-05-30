namespace Fabulous.AST.Tests.MemberDefinitions

open Fabulous.AST
open Fabulous.AST.Tests
open Xunit

open type Ast

/// Tests for the `.toOverride()` modifier across the four widget types that
/// render the `member` keyword: Method, Property (computed), AutoProperty
/// (`member val`), and PropertyGetSet (`member ... with get/set`).
module Override =

    [<Fact>]
    let ``Method member rendered as override``() =
        Oak() {
            AnonymousModule() {
                TypeDefn("Derived", UnitPat()) {
                    Inherit("Base()")

                    Member("this.Show", UnitPat(), ConstantExpr(String "derived")).toOverride()
                }
            }
        }
        |> produces
            """
type Derived() =
    inherit Base()
    override this.Show() = "derived"
"""

    [<Fact>]
    let ``Computed property member rendered as override``() =
        Oak() {
            AnonymousModule() {
                TypeDefn("Derived", UnitPat()) {
                    Inherit("Base()")

                    Member("this.Name", ConstantExpr(String "x")).toOverride()
                }
            }
        }
        |> produces
            """
type Derived() =
    inherit Base()
    override this.Name = "x"
"""

    [<Fact>]
    let ``Auto property (member val) rendered as override``() =
        Oak() {
            AnonymousModule() {
                TypeDefn("Derived", UnitPat()) {
                    Inherit("Base()")

                    MemberVal("Name", ConstantExpr(String "x"), String(), hasGetter = true).toOverride()
                }
            }
        }
        |> produces
            """
type Derived() =
    inherit Base()
    override val Name: string = "x" with get
"""

    [<Fact>]
    let ``PropertyGetSet member rendered as override``() =
        Oak() {
            AnonymousModule() {
                TypeDefn("Derived", UnitPat()) {
                    Inherit("Base()")

                    Member("this.Position", Getter(ConstantExpr(Int 0))).toOverride()
                }
            }
        }
        |> produces
            """
type Derived() =
    inherit Base()

    override this.Position
        with get () = 0
"""
