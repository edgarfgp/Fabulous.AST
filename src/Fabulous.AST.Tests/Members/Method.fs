namespace Fabulous.AST.Tests.MethodDefinitions

open Fantomas.FCS.Text
open Fabulous.AST
open Fabulous.AST.Tests
open Fantomas.Core.SyntaxOak
open type Ast
open NUnit.Framework

module Methods =
    [<Test>]
    let ``A member property `` () =
        let constExpr = Expr.Constant(Constant.FromText(SingleTextNode("name", Range.Zero)))

        let memberNode = Method("Name") { EscapeHatch(constExpr) }

        AnonymousModule() { Class("Person") { memberNode } }
        |> produces
            """
type Person =
    member this.Name = name

"""

    [<Test>]
    let ``Produces a static member`` () =

        let expr = Expr.Constant(Constant.FromText(SingleTextNode("23", Range.Zero)))

        let pattern = PatternWithCurriedParameters([])

        let memberNode = (Method("Name", pattern) { EscapeHatch(expr) }).isStatic()

        AnonymousModule() { Class("Person", []) { memberNode } }
        |> produces
            """
type Person () =
    static member Name() = 23
"""

    [<Test>]
    let ``Produces a member with a parameter`` () =
        let expr = Expr.Constant(Constant.FromText(SingleTextNode("23", Range.Zero)))

        let parameters =
            PatternWithTupledParameters([ ("params", Some(Type.FromString("string"))) ])


        let memberNode = Method("Name", parameters) { EscapeHatch(expr) }

        AnonymousModule() { Class("Person", []) { memberNode } }
        |> produces
            """
type Person () =
    member this.Name(params: string) = 23
"""

    [<Test>]
    let ``Produces a member with multiple parameter`` () =
        let expr = Expr.Constant(Constant.FromText(SingleTextNode("23", Range.Zero)))

        let parameters =
            PatternWithCurriedParameters(
                [ ("name", Some(Type.FromString("string")))
                  ("age", Some(Type.FromString("int"))) ]
            )

        let memberNode = Method("Name", parameters) { EscapeHatch(expr) }

        AnonymousModule() { Class("Person", []) { memberNode } }
        |> produces
            """
type Person () =
    member this.Name (name: string) (age: int) = 23
"""

    [<Test>]
    let ``Produces a member with tupled parameter`` () =
        let expr = Expr.Constant(Constant.FromText(SingleTextNode("23", Range.Zero)))

        let parameters =
            PatternWithTupledParameters(
                [ ("name", Some(Type.FromString("string")))
                  ("age", Some(Type.FromString("int"))) ]
            )

        let memberNode = Method("Name", parameters) { EscapeHatch(expr) }

        AnonymousModule() { Class("Person", []) { memberNode } }
        |> produces
            """
type Person () =
    member this.Name(name: string, age: int) = 23
"""

    [<Test>]
    let ``A member with attributes`` () =
        let expr = Expr.Constant(Constant.FromText(SingleTextNode("23", Range.Zero)))

        let memberNode =
            (Method("Name") { EscapeHatch(expr) })
                .attributes([ "Extension" ])
                .isStatic()

        AnonymousModule() { Class("Person", []) { memberNode } }
        |> produces
            """
type Person () =
    [<Extension>]
    static member Name = 23
"""

    [<Test>]
    let ``An inline member`` () =
        let expr = Expr.Constant(Constant.FromText(SingleTextNode("23", Range.Zero)))

        let memberNode =
            (Method("Name") { EscapeHatch(expr) })
                .isInlined()
                .isStatic()

        AnonymousModule() { Class("Person", []) { memberNode } }
        |> produces
            """
type Person () =
    static member inline Name = 23
"""

    [<Test>]
    let ``Add return info`` () =
        let expr = Expr.Constant(Constant.FromText(SingleTextNode("23", Range.Zero)))

        let memberNode =
            (Method("Name") { EscapeHatch(expr) })
                .returns(Type.FromString("int"))
                .isStatic()

        AnonymousModule() { Class("Person", []) { memberNode } }
        |> produces
            """
type Person () =
    static member Name: int = 23
"""
