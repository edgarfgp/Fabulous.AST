(**
---
title: ModuleDeclarations
category: widgets
index: 4
---
*)

(**
# Module declarations
*)

#r "../../src/Fabulous.AST/bin/Release/netstandard2.1/publish/Fantomas.Core.dll"
#r "../../src/Fabulous.AST/bin/Release/netstandard2.1/publish/Fabulous.AST.dll"
#r "../../src/Fabulous.AST/bin/Release/netstandard2.1/publish/Fantomas.Core.dll"
#r "../../src/Fabulous.AST/bin/Release/netstandard2.1/publish/Fantomas.FCS.dll"
#r "../../src/Fabulous.AST/bin/Release/netstandard2.1/publish/Fabulous.Builders.dll"

open Fabulous.AST
open Fantomas.Core
open type Fabulous.AST.Ast

Oak() {
    AnonymousModule() {
        Open("Fabulous.AST.StackAllocatedCollections")
        Open("System")
        OpenType("Fabulous.AST.Ast")

        NoWarn("0044")

        HashDirective("if", "!DEBUG")
        Value(ConstantPat(Constant("str")), ConstantExpr(String("Not debugging!")))
        HashDirective("else")
        Value(ConstantPat(Constant("str")), ConstantExpr(String("Debugging!")))
        HashDirective("endif")

        (Record("HEX") {
            Field("R", LongIdent("int"))
            Field("G", LongIdent("int"))
            Field("B", LongIdent("int"))
        })
            .attribute(Attribute("Obsolete"))

        ModuleDeclAttribute(ConstantExpr(Constant "do printfn \"Executing...\""), Attribute("Obsolete"))

        ExternBinding(LongIdent "void", "HelloWorld")

        ExternBinding(
            LongIdent "void",
            "HelloWorld2",
            [ ExternBindingPat(LongIdent "string", ConstantPat(Constant "x"))
              ExternBindingPat(Int(), ConstantPat(Constant "y")) ]
        )

        ExceptionDefn("Error")

        ExceptionDefn("Error1", Field("string"))

        ExceptionDefn("Error2", Field("msg", String())).members() {
            Property(ConstantPat(Constant("Message")), ConstantExpr(String(""))).toStatic()
        }

        NestedModule("Values") { Value("myValue", Int(12)) }

        NestedModule("Functions") { Function("myFunctionValue", "p", Int(12)) }
    }
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)
