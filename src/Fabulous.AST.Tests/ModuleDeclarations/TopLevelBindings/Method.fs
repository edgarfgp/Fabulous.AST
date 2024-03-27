namespace Fabulous.AST.Tests.ModuleDeclarations.TopLevelBindings

open Fabulous.AST
open Fabulous.AST.Tests
open Xunit

open type Ast

module MethodMembers =

    [<Fact>]
    let ``Produces MethodMembers``() =

        Oak() {
            AnonymousModule() {
                (Record("Colors") { Field("X", LongIdent("string")) })
                    .members(
                        [ Method("this.A", ParenPat(ParameterPat("p", String())), ConstantExpr(Quoted ""))

                          Method("this.C", ParenPat(ParameterPat("p", String())), ConstantExpr(Quoted ""))
                              .toInlined()

                          Method("B", ParenPat(ParameterPat("p", String())), ConstantExpr(Quoted ""))
                              .toStatic()

                          Method("D", ParenPat(ParameterPat("p", String())), ConstantExpr(Quoted ""))
                              .toInlined()
                              .toStatic()

                          Method("this.E", ParenPat(ParameterPat("p", String())), ConstantExpr(Quoted ""))
                          |> _.returnType(String())

                          Method("this.F", ParenPat(ParameterPat("p", String())), ConstantExpr(Quoted ""))
                              .toInlined()
                          |> _.returnType(String())

                          Method("G", ParenPat(ParameterPat("p", String())), ConstantExpr(Quoted ""))
                              .toStatic()
                          |> _.returnType(String())

                          Method("H", ParenPat(ParameterPat("p", String())), ConstantExpr(Quoted ""))
                              .toStatic()
                              .toInlined()
                          |> _.returnType(String())

                          Method("this.I", ParenPat(ParameterPat("p", String())), ConstantExpr(Quoted ""))

                          Method(
                              "this.J",
                              ParenPat(TuplePat([ ParameterPat("p", String()); ParameterPat("p2", String()) ])),
                              ConstantExpr(Quoted "")
                          )

                          Method(
                              "this.K",
                              [ ParenPat(ParameterPat("p", String()))
                                ParenPat(ParameterPat("p2", String())) ],
                              ConstantExpr(Quoted "")
                          )

                          Method(
                              "__.DoSomething",
                              UnitPat(),
                              IfThenElseExpr(
                                  InfixAppExpr(
                                      ConstantExpr(Constant(Unquoted "x")),
                                      "=",
                                      ConstantExpr(Constant(Unquoted "12"))
                                  ),
                                  ConstantExpr(ConstantUnit()),
                                  ConstantExpr(ConstantUnit())
                              )

                          ) ]
                    )
            }
        }
        |> produces
            """

type Colors =
    { X: string }

    member this.A(p: string) = ""
    member inline this.C(p: string) = ""
    static member B(p: string) = ""
    static member inline D(p: string) = ""
    member this.E(p: string) : string = ""
    member inline this.F(p: string) : string = ""
    static member G(p: string) : string = ""
    static member inline H(p: string) : string = ""
    member this.I(p: string) = ""
    member this.J(p: string, p2: string) = ""
    member this.K (p: string) (p2: string) = ""
    member __.DoSomething() = if x = 12 then () else ()

"""

    [<Fact>]
    let ``Produces a record with TypeParams and method member``() =
        Oak() {
            AnonymousModule() {
                (Record("Colors") {
                    Field("Green", LongIdent("string"))
                    Field("Blue", LongIdent("'other"))
                    Field("Yellow", LongIdent("int"))
                })
                    .typeParams([ "'other" ])
                    .members([ Method("this.A", ParenPat(ParameterPat("p", String())), ConstantExpr(Quoted "")) ])
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

    [<Fact>]
    let ``Produces a record with TypeParams and static method member``() =
        Oak() {
            AnonymousModule() {
                (Record("Colors") {
                    Field("Green", LongIdent("string"))
                    Field("Blue", LongIdent("'other"))
                    Field("Yellow", LongIdent("int"))
                })
                    .typeParams([ "'other" ])
                    .members(
                        [ Method("A", ParenPat(ParameterPat("p", String())), ConstantExpr(Quoted ""))
                              .toStatic() ]
                    )
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

    [<Fact>]
    let ``Produces a record with method member``() =
        Oak() {
            AnonymousModule() {
                (Record("Colors") { Field("X", LongIdent("string")) })
                    .members([ Method("this.A", ParenPat(ParameterPat("p", String())), ConstantExpr(Quoted "")) ])
            }
        }
        |> produces
            """

type Colors =
    { X: string }

    member this.A(p: string) = ""

"""

    [<Fact>]
    let ``Produces a record with static method member``() =
        Oak() {
            AnonymousModule() {
                (Record("Colors") { Field("X", LongIdent("string")) })
                    .members(
                        [ Method("A", ParenPat(ParameterPat("p", String())), ConstantExpr(Quoted ""))
                              .toStatic() ]
                    )
            }
        }
        |> produces
            """

type Colors =
    { X: string }

    static member A(p: string) = ""

"""

    [<Fact>]
    let ``Produces a classes with a method member``() =
        Oak() {
            AnonymousModule() {
                Class("Person", Constructor()) { Method("this.Name", UnitPat(), ConstantExpr(Unquoted "23")) }
            }
        }
        |> produces
            """
type Person () =
    member this.Name() = 23
"""

    [<Fact>]
    let ``Produces a classes with a method member and parameter``() =
        Oak() {
            AnonymousModule() {
                Class("Person") {
                    Method("this.Name", ParenPat(ParameterPat("p", String())), ConstantExpr(Unquoted "23"))
                }
            }
        }
        |> produces
            """
type Person () =
    member this.Name(p: string) = 23
"""

    [<Fact>]
    let ``Produces a method member with tupled parameter``() =
        Oak() {
            AnonymousModule() {
                Class("Person") {
                    Method(
                        "this.Name",
                        ParenPat(TuplePat([ ParameterPat("name", String()); ParameterPat("age", Int32()) ])),
                        ConstantExpr(Unquoted "23")
                    )
                }
            }
        }
        |> produces
            """
type Person () =
    member this.Name(name: string, age: int) = 23
"""

    [<Fact>]
    let ``Produces a method member with multiple parameter``() =
        Oak() {
            AnonymousModule() {
                Class("Person") {
                    Method(
                        "this.Name",
                        LongIdentPat(
                            [ ParenPat(ParameterPat("name", String()))
                              ParenPat(ParameterPat("age", Int32())) ]
                        ),
                        ConstantExpr(Unquoted "23")
                    )
                }
            }
        }
        |> produces
            """
type Person () =
    member this.Name (name: string) (age: int) = 23
"""

    [<Fact>]
    let ``Need to add multiple bindings to method``() =
        Oak() {
            AnonymousModule() {
                Class("Person") {
                    Method(
                        "GetPrimitiveReader",
                        ParenPat(
                            TuplePat(
                                [ ParameterPat("t", LongIdent("System.Type"))
                                  ParameterPat("reader", LongIdent("Microsoft.Data.SqlClient.SqlDataReader"))
                                  ParameterPat("isOpt", Boolean())
                                  ParameterPat("isNullable", Boolean()) ]
                            )
                        ),
                        CompExprBodyExpr(
                            [ LetOrUseExpr(
                                  Function(
                                      "wrapValue",
                                      [ ParameterPat("get"); ParenPat(ParameterPat("ord", Int32())) ],
                                      IfThenElifExpr(
                                          [ IfThenExpr(
                                                ConstantExpr(Unquoted "isOpt"),
                                                InfixAppExpr(
                                                    ParenExpr(
                                                        IfThenElseExpr(
                                                            AppExpr(
                                                                OptVarExpr("reader.IsDBNull"),
                                                                ConstantExpr(Unquoted("ord"))
                                                            ),
                                                            ConstantExpr(Unquoted "None"),
                                                            ConstantExpr(Unquoted("get ord |> box"))
                                                        )
                                                    ),
                                                    "|>",
                                                    Unquoted("box")
                                                )
                                            )

                                            ElIfThenExpr(
                                                ConstantExpr(Unquoted "isNullable"),
                                                ParenExpr(
                                                    IfThenElseExpr(
                                                        AppExpr(
                                                            OptVarExpr("reader.IsDBNull"),
                                                            ConstantExpr(Unquoted("ord"))
                                                        ),
                                                        AppLongIdentAndSingleParenArgExpr(
                                                            [ "System"; "Nullable" ],
                                                            "()"
                                                        ),
                                                        InfixAppExpr(
                                                            AppExpr(
                                                                ParenExpr(ConstantExpr(Unquoted("get ord"))),
                                                                OptVarExpr("System.Nullable")
                                                            ),
                                                            "|>",
                                                            OptVarExpr("box")
                                                        )
                                                    )
                                                )
                                            ) ],
                                          ConstantExpr(Unquoted("get ord |> box"))
                                      )
                                  )
                              )

                              LetOrUseExpr(
                                  Function(
                                      "wrapRef",
                                      [ ParameterPat("get"); ParenPat(ParameterPat("ord", Int32())) ],
                                      IfThenElifExpr(
                                          [ IfThenExpr(
                                                ConstantExpr(Unquoted "isOpt"),
                                                InfixAppExpr(
                                                    ParenExpr(
                                                        IfThenElseExpr(
                                                            AppExpr(
                                                                OptVarExpr("reader.IsDBNull"),
                                                                ConstantExpr(Unquoted("ord"))
                                                            ),
                                                            ConstantExpr(Unquoted "None"),
                                                            ConstantExpr(Unquoted("get ord |> Some"))
                                                        )
                                                    ),
                                                    "|>",
                                                    Unquoted("box")
                                                )
                                            ) ],
                                          ConstantExpr(Unquoted("get ord |> box"))
                                      )
                                  )
                              )

                              LetOrUseExpr(Use("wrapValue", OptVarExpr("reader.GetGuid")))

                              OtherExpr(
                                  IfThenElifExpr(
                                      [ IfThenExpr(
                                            TypeAppExpr("typedefof", LongIdent("System.Guid")),
                                            AppLongIdentAndSingleParenArgExpr(
                                                "Some",
                                                ParenExpr(AppExpr("wrapValue", OptVarExpr("reader.GetGuid")))
                                            )
                                        )

                                        ElIfThenExpr(
                                            TypeAppExpr("typedefof", Boolean()),
                                            AppLongIdentAndSingleParenArgExpr(
                                                "Some",
                                                ParenExpr(AppExpr("wrapValue", OptVarExpr("reader.GetBoolean")))
                                            )
                                        )

                                        ElIfThenExpr(
                                            TypeAppExpr("typedefof", Int32()),
                                            AppLongIdentAndSingleParenArgExpr(
                                                "Some",
                                                ParenExpr(AppExpr("wrapValue", OptVarExpr("reader.GetInt32")))
                                            )
                                        )

                                        ElIfThenExpr(
                                            TypeAppExpr("typedefof", Int64()),
                                            AppLongIdentAndSingleParenArgExpr(
                                                "Some",
                                                ParenExpr(AppExpr("wrapValue", OptVarExpr("reader.GetInt64")))
                                            )
                                        )

                                        ElIfThenExpr(
                                            TypeAppExpr("typedefof", Int16()),
                                            AppLongIdentAndSingleParenArgExpr(
                                                "Some",
                                                ParenExpr(AppExpr("wrapValue", OptVarExpr("reader.GetInt16")))
                                            )
                                        )

                                        ElIfThenExpr(
                                            TypeAppExpr("typedefof", Byte()),
                                            AppLongIdentAndSingleParenArgExpr(
                                                "Some",
                                                ParenExpr(AppExpr("wrapValue", OptVarExpr("reader.GetByte")))
                                            )
                                        )

                                        ElIfThenExpr(
                                            TypeAppExpr("typedefof", Double()),
                                            AppLongIdentAndSingleParenArgExpr(
                                                "Some",
                                                ParenExpr(AppExpr("wrapValue", OptVarExpr("reader.GetDouble")))
                                            )
                                        )

                                        ElIfThenExpr(
                                            TypeAppExpr("typedefof", LongIdent("System.Single")),
                                            AppLongIdentAndSingleParenArgExpr(
                                                "Some",
                                                ParenExpr(AppExpr("wrapValue", OptVarExpr("reader.GetFloat")))
                                            )
                                        )

                                        ElIfThenExpr(
                                            TypeAppExpr("typedefof", Decimal()),
                                            AppLongIdentAndSingleParenArgExpr(
                                                "Some",
                                                ParenExpr(AppExpr("wrapValue", OptVarExpr("reader.GetDecimal")))
                                            )
                                        )

                                        ElIfThenExpr(
                                            TypeAppExpr("typedefof", String()),
                                            AppLongIdentAndSingleParenArgExpr(
                                                "Some",
                                                ParenExpr(AppExpr("wrapRef", OptVarExpr("reader.GetString")))
                                            )
                                        )

                                        ElIfThenExpr(
                                            TypeAppExpr("typedefof", LongIdent("System.DateTimeOffset")),
                                            AppLongIdentAndSingleParenArgExpr(
                                                "Some",
                                                ParenExpr(AppExpr("wrapValue", OptVarExpr("reader.GetDateTimeOffset")))
                                            )
                                        )

                                        ElIfThenExpr(
                                            TypeAppExpr("typedefof", LongIdent("System.DateOnly")),
                                            AppLongIdentAndSingleParenArgExpr(
                                                "Some",
                                                ParenExpr(AppExpr("wrapValue", OptVarExpr("reader.GetDateOnly")))
                                            )
                                        )

                                        ElIfThenExpr(
                                            TypeAppExpr("typedefof", LongIdent("System.TimeOnly")),
                                            AppLongIdentAndSingleParenArgExpr(
                                                "Some",
                                                ParenExpr(AppExpr("wrapValue", OptVarExpr("reader.GetTimeOnly")))
                                            )
                                        )

                                        ElIfThenExpr(
                                            TypeAppExpr("typedefof", LongIdent("System.DateTime")),
                                            AppLongIdentAndSingleParenArgExpr(
                                                "Some",
                                                ParenExpr(AppExpr("wrapValue", OptVarExpr("reader.GetDateTime")))
                                            )
                                        )

                                        ElIfThenExpr(
                                            TypeAppExpr("typedefof", LongIdent("byte []")),
                                            AppLongIdentAndSingleParenArgExpr(
                                                "Some",
                                                ParenExpr(
                                                    AppExpr("wrapRef", OptVarExpr("reader.GetFieldValue<byte []>"))
                                                )
                                            )
                                        )

                                        ElIfThenExpr(
                                            TypeAppExpr("typedefof", LongIdent("obj")),
                                            AppLongIdentAndSingleParenArgExpr(
                                                "Some",
                                                ParenExpr(AppExpr("wrapRef", OptVarExpr("reader.GetFieldValue")))
                                            )
                                        ) ],
                                      ConstantExpr(Unquoted "None")
                                  )
                              ) ]
                        )
                    )
                        .toStatic()
                        .toPrivate()
                }
            }
        }
        |> produces
            """
type Person () =
    static member private GetPrimitiveReader
        (t: System.Type, reader: Microsoft.Data.SqlClient.SqlDataReader, isOpt: bool, isNullable: bool)
        =
        let wrapValue get (ord: int) =
            if isOpt then
                (if reader.IsDBNull ord then None else get ord |> box) |> box
            elif isNullable then
                (if reader.IsDBNull ord then
                     System.Nullable ()
                 else
                     (get ord) System.Nullable |> box)
            else
                get ord |> box

        let wrapRef get (ord: int) =
            if isOpt then
                (if reader.IsDBNull ord then None else get ord |> Some) |> box
            else
                get ord |> box

        use wrapValue = reader.GetGuid

        if typedefof<System.Guid> then
            Some(wrapValue reader.GetGuid)
        elif typedefof<bool> then
            Some(wrapValue reader.GetBoolean)
        elif typedefof<int> then
            Some(wrapValue reader.GetInt32)
        elif typedefof<int64> then
            Some(wrapValue reader.GetInt64)
        elif typedefof<int16> then
            Some(wrapValue reader.GetInt16)
        elif typedefof<byte> then
            Some(wrapValue reader.GetByte)
        elif typedefof<double> then
            Some(wrapValue reader.GetDouble)
        elif typedefof<System.Single> then
            Some(wrapValue reader.GetFloat)
        elif typedefof<decimal> then
            Some(wrapValue reader.GetDecimal)
        elif typedefof<string> then
            Some(wrapRef reader.GetString)
        elif typedefof<System.DateTimeOffset> then
            Some(wrapValue reader.GetDateTimeOffset)
        elif typedefof<System.DateOnly> then
            Some(wrapValue reader.GetDateOnly)
        elif typedefof<System.TimeOnly> then
            Some(wrapValue reader.GetTimeOnly)
        elif typedefof<System.DateTime> then
            Some(wrapValue reader.GetDateTime)
        elif typedefof<byte []> then
            Some(wrapRef reader.GetFieldValue<byte []>)
        elif typedefof<obj> then
            Some(wrapRef reader.GetFieldValue)
        else
            None
"""

    [<Fact>]
    let ``Produces a method member with attributes``() =
        Oak() {
            AnonymousModule() {
                Class("Person") {
                    Method("this.Name", UnitPat(), ConstantExpr(Unquoted "23"))
                        .attribute("Obsolete")
                }
            }
        }
        |> produces
            """
type Person () =
    [<Obsolete>]
    member this.Name() = 23

"""

    [<Fact>]
    let ``Produces an inline method member``() =
        Oak() {
            AnonymousModule() {
                Class("Person") { Method("this.Name", UnitPat(), ConstantExpr(Unquoted "23")).toInlined() }
            }
        }
        |> produces
            """
type Person () =
    member inline this.Name() = 23
"""

    [<Fact>]
    let ``Produces an method member with type parameters``() =
        Oak() {
            AnonymousModule() {
                Class("Person") {
                    Method("this.Name", UnitPat(), ConstantExpr(Unquoted "23"))
                        .typeParams([ "'other" ])
                }
            }
        }
        |> produces
            """
type Person () =
    member this.Name<'other>() = 23
            """

    [<Fact>]
    let ``Produces a union with a method member ``() =
        Oak() {
            AnonymousModule() {
                (Union("Person") { UnionCase("Name") })
                    .members([ Method("this.Name", UnitPat(), ConstantExpr(Quoted "name")) ])
            }
        }
        |> produces
            """
type Person =
    | Name

    member this.Name() = "name"

"""

    [<Fact>]
    let ``Produces a generic union with a method member``() =
        Oak() {
            AnonymousModule() {
                (Union("Colors") {
                    UnionCase("Red", [ Field("a", LongIdent("string")); Field("b", LongIdent "'other") ])

                    UnionCase("Green")
                    UnionCase("Blue")
                    UnionCase("Yellow")
                })
                    .typeParams([ "'other" ])
                    .members([ Method("this.Name", UnitPat(), ConstantExpr(Quoted "name")) ])

            }
        }
        |> produces
            """
type Colors<'other> =
    | Red of a: string * b: 'other
    | Green
    | Blue
    | Yellow

    member this.Name() = "name"

"""
