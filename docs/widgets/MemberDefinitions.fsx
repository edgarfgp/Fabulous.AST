(**
---
title: MemberDefinitions
category: widgets
index: 6
---
*)

(**
# Member Definitions
*)

#r "../../src/Fabulous.AST/bin/Release/netstandard2.1/publish/Fantomas.Core.dll"
#r "../../src/Fabulous.AST/bin/Release/netstandard2.1/publish/Fabulous.AST.dll"
#r "../../src/Fabulous.AST/bin/Release/netstandard2.1/publish/Fantomas.FCS.dll"

open Fabulous.AST
open type Fabulous.AST.Ast

Oak() {
    AnonymousModule() {
        Open("System")

        TypeDefn(
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

            Member(
                "Create2",
                ParenPat(TuplePat [ "name"; "middle" ]),
                NewExpr(LongIdent "Person", ParenExpr(TupleExpr([ "name"; "middle" ])))
            )
                .toStatic()

            Member("this.Name", "name")

            MemberVal("Middle", "middle", true)

            MemberVal("LastName", "lastName", true, true)

            Member(
                "this.Age",
                Getter(ConstantExpr(Constant("_age"))),
                Setter(ParenPat(NamedPat("value")), LongIdentSetExpr("_age", "value"))
            )

            AbstractMember("GetValue", [ Unit() ], String())

            Default("this.GetValue", UnitPat(), ConstantExpr(String("Hello World")))

            InterfaceWith("IDisposable") { Member("this.Dispose", UnitPat(), ConstantUnit()) }
        }
    }
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)
