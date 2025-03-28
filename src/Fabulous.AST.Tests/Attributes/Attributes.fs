namespace Fabulous.AST.Tests.Attributes

open Xunit
open Fabulous.AST.Tests

open Fabulous.AST

open type Ast

module AttributesNodes =

    [<Fact>]
    let ``Simple AttributeNode``() =
        Oak() {
            AnonymousModule() {
                Value(ConstantPat(Constant("x")), ConstantExpr(Int(12))).attribute(Attribute "Obsolete")
            }
        }
        |> produces
            """
[<Obsolete>]
let x = 12
"""

    [<Fact>]
    let ``Simple AttributeNode with expr``() =
        Oak() {
            AnonymousModule() {
                Value(ConstantPat(Constant("x")), ConstantExpr(Int(12)))
                    .attribute(Attribute("Obsolete", ParenExpr(ConstantExpr(String("This is obsolete")))))
            }
        }
        |> produces
            """
[<Obsolete("This is obsolete")>]
let x = 12
"""

    [<Fact>]
    let ``Multiple attributes``() =
        Oak() {
            AnonymousModule() {
                Value(ConstantPat(Constant("x")), ConstantExpr(Int(12)))
                    .attributes([ Attribute("Obsolete", ParenExpr(String("This is obsolete"))) ])
            }
        }
        |> produces
            """
[<Obsolete("This is obsolete")>]
let x = 12
"""

    [<Fact>]
    let ``Simple AttributeNode type name and target``() =
        Oak() {
            AnonymousModule() {
                Value(ConstantPat(Constant("x")), ConstantExpr(Int(12))).attribute(AttributeTarget("Struct", "return"))
            }
        }
        |> produces
            """
[<return: Struct>]
let x = 12
"""

    [<Fact>]
    let ``Attributes on member and get, set properties``() =
        Oak() {
            AnonymousModule() {
                TypeDefn("Object3D", UnitPat()) {
                    Member(
                        "this.position",
                        Setter(NamedPat("v"), SetExpr(ConstantExpr("_position"), ConstantExpr("v")))
                            .attribute(Attribute("Y")),
                        Getter("_position").attribute(Attribute("Z"))
                    )
                        .attribute(Attribute("X"))
                }
            }
        }
        |> produces
            """
type Object3D() =
    [<X>]
    member this.position
        with [<Y>] set v = _position <- v
        and [<Z>] get () = _position
"""
