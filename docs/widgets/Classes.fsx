(**
---
title: Classes
category: widgets
index: 15
---
*)

(**
# Classes
*)

#r "../../src/Fabulous.AST/bin/Release/netstandard2.1/publish/Fantomas.Core.dll"
#r "../../src/Fabulous.AST/bin/Release/netstandard2.1/publish/Fabulous.AST.dll"
#r "../../src/Fabulous.AST/bin/Release/netstandard2.1/publish/Fantomas.FCS.dll"

open Fabulous.AST
open type Fabulous.AST.Ast

Oak() {
    AnonymousModule() {
        TypeDefn(
            "Person2",
            ImplicitConstructor(
                ParenPat(
                    TuplePat(
                        [ ParameterPat(ConstantPat(Constant("name")), String())
                          ParameterPat(ConstantPat(Constant("lastName")), String())
                          ParameterPat(ConstantPat(Constant("?age")), Int()) ]
                    )
                )
            )
        ) {
            Member(ConstantPat(Constant("this.Name")), ConstantExpr(Constant "name"))
        }

        TypeDefn("Person3", ParenPat()) { Member(ConstantPat(Constant("this.Name")), ConstantExpr(String "")) }
        |> _.typeParams(PostfixList([ "'a"; "'b" ]))

    }
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)
