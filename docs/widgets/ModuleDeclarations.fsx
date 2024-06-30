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
|> CodeFormatter.FormatOakAsync
|> Async.RunSynchronously
|> printfn "%s"

(**
# Generated code:
*)

open Fabulous.AST.StackAllocatedCollections
open System
open type Fabulous.AST.Ast

#nowarn "0044"
#if !DEBUG
let str = "Not debugging!"
#else
let str = "Debugging!"
#endif

[<Obsolete>]
type HEX =
    { R: int
      G: int
      B: int }

[<Obsolete>]
do printfn "Executing..."

extern void HelloWorld()
extern void HelloWorld2(string x, int y)
exception Error
exception Error1 of string

exception Error2 of msg: string with
    static member Message = ""

module Values =
    let myValue = 12

module Functions =
    let myFunctionValue p = 12
