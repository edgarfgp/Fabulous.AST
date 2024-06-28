(**
---
title: MemberDefinitions
category: widgets
index: 5
---
*)

(**
# F# Member Definitions
*)

#r "../../src/Fabulous.AST/bin/Release/netstandard2.1/publish/Fantomas.Core.dll"
#r "../../src/Fabulous.AST/bin/Release/netstandard2.1/publish/Fabulous.AST.dll"
#r "../../src/Fabulous.AST/bin/Release/netstandard2.1/publish/Fantomas.Core.dll"
#r "../../src/Fabulous.AST/bin/Release/netstandard2.1/publish/Fantomas.FCS.dll"

open Fabulous.AST
open Fantomas.Core
open type Fabulous.AST.Ast

(**
# Member Definitions
*)

Oak() {
    AnonymousModule() {
        Open("System")

        Class(
            "Person",
            ImplicitConstructor(
                ParenPat(
                    TuplePat(
                        [ ParameterPat("name", String())
                          ParameterPat("middle", String())
                          ParameterPat("?lastName", String())
                          ParameterPat("?age", Int()) ]
                    )
                )
            )
        ) {
            LetBindings(
                [ Value("_age", "defaultArg age 18").returnType(Int()).toMutable()
                  Function(
                      "Create",
                      [ "name"; "middle" ],
                      NewExpr(LongIdent "Person", ParenExpr(TupleExpr([ "name"; "middle" ])))
                  ) ]
            )

            ExplicitConstructor(
                ParenPat(TuplePat([ Constant "name"; Constant "middle" ])),
                NewExpr(LongIdent "Person", ParenExpr(TupleExpr([ "name"; "middle" ])))
            )

            Method(
                "Create2",
                ParenPat(TuplePat [ "name"; "middle" ]),
                NewExpr(LongIdent "Person", ParenExpr(TupleExpr([ "name"; "middle" ])))
            )
                .toStatic()

            Property("this.Name", "name")

            AutoProperty("Middle", "middle", true)

            AutoProperty("LastName", "lastName", true, true)

            Property(
                "this.Age",
                Getter(ConstantExpr(Constant("_age"))),
                Setter(ParenPat(NamedPat("value")), LongIdentSetExpr("_age", "value"))
            )

            AbstractSlot("GetValue", [ Unit() ], String())

            Default("this.GetValue", UnitPat(), ConstantExpr(String("Hello World")))

            InterfaceMember("IDisposable") { Method("this.Dispose", UnitPat(), ConstantUnit()) }
        }
    }
}
|> Gen.mkOak
|> CodeFormatter.FormatOakAsync
|> Async.RunSynchronously
|> printfn "%s"

(**
Will output the following code:
*)

open System

type Person(name: string, middle: string, ?lastName: string, ?age: int) =
    let mutable _age: int = defaultArg age 18
    let Create name middle = new Person(name, middle)

    new(name, middle) = new Person(name, middle)
    static member Create2(name, middle) = new Person(name, middle)
    member this.Name = name
    member val Middle = middle with get
    member val LastName = lastName with get, set

    member this.Age
        with get () = _age
        and set (value) = _age <- value

    abstract member GetValue: unit -> string
    default this.GetValue() = "Hello World"

    interface IDisposable with
        member this.Dispose() = ()
