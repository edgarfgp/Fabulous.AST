namespace Fabulous.AST.Tests.TypeDefinitions

open Fantomas.FCS.Text
open Fabulous.AST.Tests
open Fantomas.Core.SyntaxOak
open NUnit.Framework

open Fabulous.AST
open type Ast

module Record =
    [<Test>]
    let ``Produces a record`` () =

        AnonymousModule() {
            Record("Colors") {
                Field("Red", CommonType.Int32)
                Field("Green", CommonType.Int32)
                Field("Blue", CommonType.Int32)
            }
        }
        |> produces
            """

type Colors = { Red: int; Green: int; Blue: int }

"""

    [<Test>]
    let ``Produces a record using EscapeHatch`` () =
        let customField =
            FieldNode(
                Some(XmlDocNode([| "/// Super cool doc bro" |], Range.Zero)),
                None,
                None,
                false,
                None,
                Some(SingleTextNode("Green", Range.Zero)),
                CommonType.Int32,
                Range.Zero
            )

        AnonymousModule() {
            Record("Colors") {
                Field("Red", CommonType.Int32)
                EscapeHatch(customField)
                Field("Blue", CommonType.Int32)
            }
        }
        |> produces
            """

type Colors =
    {
        Red: int
        /// Super cool doc bro
        Green: int
        Blue: int
    }

"""

    [<Test>]
    let ``Produces a record using a loop`` () =
        AnonymousModule() {
            Record("Colors") {
                for colour in [ "Red"; "Green"; "Blue" ] do
                    Field(colour, CommonType.Int32)
            }
        }
        |> produces
            """

type Colors = { Red: int; Green: int; Blue: int }

"""

    [<Test>]
    let ``Produces a record with property member`` () =

        let expr = Expr.Constant(Constant.FromText(SingleTextNode("\"\"", Range.Zero)))

        AnonymousModule() {
            (Record("Colors") { Field("X", CommonType.String) })
                .members() {
                PropertyMember("this.A") { EscapeHatch(expr) }
            }
        }
        |> produces
            """

type Colors =
    { X: string }

    member this.A = ""

"""

    [<Test>]
    let ``Produces a record with static property member`` () =

        let expr = Expr.Constant(Constant.FromText(SingleTextNode("\"\"", Range.Zero)))

        AnonymousModule() {
            (Record("Colors") { Field("X", CommonType.String) })
                .members() {
                StaticPropertyMember("A") { EscapeHatch(expr) }
            }
        }
        |> produces
            """

type Colors =
    { X: string }

    static member A = ""

"""

    [<Test>]
    let ``Produces a record with method member`` () =

        let expr = Expr.Constant(Constant.FromText(SingleTextNode("\"\"", Range.Zero)))

        let parameters =
            [ Pattern.CreateSingleParameter(Pattern.CreateNamed("p"), Some(CommonType.String)) ]

        AnonymousModule() {
            (Record("Colors") { Field("X", CommonType.String) })
                .members() {
                MethodMember("this.A", parameters) { EscapeHatch(expr) }
            }
        }
        |> produces
            """

type Colors =
    { X: string }

    member this.A(p: string) = ""

"""

    [<Test>]
    let ``Produces a record with static method member`` () =

        let expr = Expr.Constant(Constant.FromText(SingleTextNode("\"\"", Range.Zero)))

        let parameters =
            [ Pattern.CreateSingleParameter(Pattern.CreateNamed("p"), Some(CommonType.String)) ]

        AnonymousModule() {
            (Record("Colors") { Field("X", CommonType.String) })
                .members() {
                StaticMethodMember("A", parameters) { EscapeHatch(expr) }
            }
        }
        |> produces
            """

type Colors =
    { X: string }

    static member A(p: string) = ""

"""

    [<Test>]
    let ``Produces a record with interface member`` () =

        AnonymousModule() {
            Interface("IMyInterface") {
                let parameters = [ CommonType.Unit ]
                AbstractCurriedMethodMember("GetValue", parameters, CommonType.String)
            }

            (Record("MyRecord") {
                Field("MyField1", CommonType.Int32)
                Field("MyField2", CommonType.String)
            })
                .members() {
                let expr =
                    Expr.Constant(Constant.FromText(SingleTextNode("x.MyField2", Range.Zero)))

                InterfaceMember(CommonType.mkType("IMyInterface")) { MethodMember("x.GetValue") { EscapeHatch(expr) } }
            }
        }
        |> produces
            """

type IMyInterface =
    abstract member GetValue: unit -> string

type MyRecord =
    { MyField1: int
      MyField2: string }

    interface IMyInterface with
        member x.GetValue() = x.MyField2
"""

    [<Test>]
    let ``Produces a record with an attribute`` () =
        AnonymousModule() {
            (Record("Colors") {
                for colour in [ "Red"; "Green"; "Blue" ] do
                    Field(colour, CommonType.Int32)
            })
                .attributes([ "Serializable" ])
        }
        |> produces
            """

[<Serializable>]
type Colors = { Red: int; Green: int; Blue: int }

"""

    [<Test>]
    let ``Produces a record field with an attribute`` () =
        AnonymousModule() {
            Record("Colors") {
                Field("Red", CommonType.Int32).attributes([ "Obsolete" ])
                Field("Green", CommonType.Int32)
                Field("Blue", CommonType.Int32)
            }
        }
        |> produces
            """

type Colors =
    { [<Obsolete>]
      Red: int
      Green: int
      Blue: int }

"""

    [<Test>]
    let ``Produces a record with TypeParams`` () =
        AnonymousModule() {
            GenericRecord("Colors", [ "'other" ]) {
                Field("Green", CommonType.String)
                Field("Blue", CommonType.mkType("'other"))
                Field("Yellow", CommonType.Int32)
            }
        }

        |> produces
            """

type Colors<'other> =
    { Green: string
      Blue: 'other
      Yellow: int }

"""

    [<Test>]
    let ``Produces a record with TypeParams and property member`` () =
        AnonymousModule() {
            (GenericRecord("Colors", [ "'other" ]) {
                Field("Green", CommonType.String)
                Field("Blue", CommonType.mkType("'other"))
                Field("Yellow", CommonType.Int32)
            })
                .members() {
                let expr = Expr.Constant(Constant.FromText(SingleTextNode("\"\"", Range.Zero)))
                PropertyMember("this.A") { EscapeHatch(expr) }
            }
        }

        |> produces
            """

type Colors<'other> =
    { Green: string
      Blue: 'other
      Yellow: int }

    member this.A = ""

"""

    [<Test>]
    let ``Produces a record with TypeParams and static property member`` () =
        AnonymousModule() {
            (GenericRecord("Colors", [ "'other" ]) {
                Field("Green", CommonType.String)
                Field("Blue", CommonType.mkType("'other"))
                Field("Yellow", CommonType.Int32)
            })
                .members() {
                let expr = Expr.Constant(Constant.FromText(SingleTextNode("\"\"", Range.Zero)))
                StaticPropertyMember("A") { EscapeHatch(expr) }
            }
        }

        |> produces
            """

type Colors<'other> =
    { Green: string
      Blue: 'other
      Yellow: int }

    static member A = ""

"""

    [<Test>]
    let ``Produces a record with TypeParams and method member`` () =
        AnonymousModule() {
            (GenericRecord("Colors", [ "'other" ]) {
                Field("Green", CommonType.String)
                Field("Blue", CommonType.mkType("'other"))
                Field("Yellow", CommonType.Int32)
            })
                .members() {
                let expr = Expr.Constant(Constant.FromText(SingleTextNode("\"\"", Range.Zero)))

                let parameters =
                    [ Pattern.CreateSingleParameter(Pattern.CreateNamed("p"), Some(CommonType.String)) ]

                MethodMember("this.A", parameters) { EscapeHatch(expr) }
            }
        }

        |> produces
            """

type Colors<'other> =
    { Green: string
      Blue: 'other
      Yellow: int }

    member this.A(p: string) = ""

"""

    [<Test>]
    let ``Produces a record with TypeParams and static method member`` () =
        AnonymousModule() {
            (GenericRecord("Colors", [ "'other" ]) {
                Field("Green", CommonType.String)
                Field("Blue", CommonType.mkType("'other"))
                Field("Yellow", CommonType.Int32)
            })
                .members() {
                let expr = Expr.Constant(Constant.FromText(SingleTextNode("\"\"", Range.Zero)))

                let parameters =
                    [ Pattern.CreateSingleParameter(Pattern.CreateNamed("p"), Some(CommonType.String)) ]

                StaticMethodMember("A", parameters) { EscapeHatch(expr) }
            }
        }

        |> produces
            """

type Colors<'other> =
    { Green: string
      Blue: 'other
      Yellow: int }

    static member A(p: string) = ""

"""

    [<Test>]
    let ``Produces a record with TypeParams and interface member`` () =
        AnonymousModule() {
            Interface("IMyInterface") {
                let parameters = [ CommonType.Unit ]
                AbstractCurriedMethodMember("GetValue", parameters, CommonType.String)
            }

            (GenericRecord("Colors", [ "'other" ]) {
                Field("Green", CommonType.String)
                Field("Blue", CommonType.mkType("'other"))
                Field("Yellow", CommonType.Int32)
            })
                .members() {
                let expr =
                    Expr.Constant(Constant.FromText(SingleTextNode("x.MyField2", Range.Zero)))

                InterfaceMember(CommonType.mkType("IMyInterface")) { MethodMember("x.GetValue") { EscapeHatch(expr) } }
            }
        }

        |> produces
            """
type IMyInterface =
    abstract member GetValue: unit -> string

type Colors<'other> =
    { Green: string
      Blue: 'other
      Yellow: int }

    interface IMyInterface with
        member x.GetValue() = x.MyField2

"""

    [<Test>]
    let ``Produces a struct record with TypeParams`` () =
        AnonymousModule() {
            (GenericRecord("Colors", [ "'other" ]) {
                Field("Green", CommonType.String)
                Field("Blue", CommonType.mkType("'other"))
                Field("Yellow", CommonType.Int32)
            })
                .isStruct()
        }

        |> produces
            """
[<Struct>]
type Colors<'other> =
    { Green: string
      Blue: 'other
      Yellow: int }

"""
