namespace AstEditor

/// The built-in example scripts offered from the toolbar's Examples menu. Each is a full F#
/// script ending in the generated source (`... |> Gen.mkOak |> Gen.run`).
module Examples =

    let private hello =
        """open Fabulous.AST
open type Fabulous.AST.Ast

Oak() {
    AnonymousModule() {
        Value("greeting", String("Hello from Fabulous.AST"))
        AppExpr("printfn", [ String("%s"); Constant("greeting") ])
    }
}
|> Gen.mkOak
|> Gen.run"""

    let private record =
        """open Fabulous.AST
open type Fabulous.AST.Ast

Oak() {
    AnonymousModule() {
        Record("Person") {
            Field("Name", String())
            Field("Age", Int())
        }

        Value("alice", RecordExpr([ RecordFieldExpr("Name", String("Alice")); RecordFieldExpr("Age", Int(30)) ]))
        AppExpr("printfn", [ String("%A"); Constant("alice") ])
    }
}
|> Gen.mkOak
|> Gen.run"""

    let private union =
        """open Fabulous.AST
open type Fabulous.AST.Ast

Oak() {
    AnonymousModule() {
        Union("Shape") {
            UnionCase("Rectangle", [ Field("width", Float()); Field("length", Float()) ])
            UnionCase("Circle", Field("radius", Float()))
        }
    }
}
|> Gen.mkOak
|> Gen.run"""

    let private classes =
        """open Fabulous.AST
open type Fabulous.AST.Ast

Oak() {
    AnonymousModule() {
        TypeDefn("Person", Constructor(TuplePat([ ParameterPat("name", String()); ParameterPat("age", Int()) ]))) {
            Member("this.Name", ConstantExpr("name"))
            Member("this.Age", ConstantExpr("age"))
        }
    }
}
|> Gen.mkOak
|> Gen.run"""

    let private functions =
        """open Fabulous.AST
open type Fabulous.AST.Ast

Oak() {
    AnonymousModule() {
        Function("add", [ ParameterPat("a"); ParameterPat("b") ], InfixAppExpr("a", "+", "b"))
        Function("square", ParameterPat("x"), InfixAppExpr("x", "*", "x"))

        AppExpr("printfn", [ ConstantExpr(String("%d %d")); ParenExpr(AppExpr("add", [ Int(1); Int(2) ])); ParenExpr(AppExpr("square", Int(4))) ])
    }
}
|> Gen.mkOak
|> Gen.run"""

    let private rewrite =
        """open Fabulous.AST
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text
open type Fabulous.AST.Ast

let intExpr (value: int) : Expr =
    Expr.Constant(Constant.FromText(SingleTextNode(string value, Range.range0)))

let (|IntLit|_|) (e: Expr) : int option =
    match e with
    | Expr.Constant(Constant.FromText n) ->
        match System.Int32.TryParse n.Text with
        | true, v -> Some v
        | _ -> None
    | _ -> None

// Constant-fold trivial arithmetic: x*1, x+0, and literal folding.
let foldConstants (e: Expr) : Expr =
    match e with
    | Expr.InfixApp n ->
        match n.Operator.Text, n.LeftHandSide, n.RightHandSide with
        | "+", lhs, IntLit 0
        | "+", IntLit 0, lhs
        | "*", lhs, IntLit 1
        | "*", IntLit 1, lhs -> lhs
        | "+", IntLit a, IntLit b -> intExpr (a + b)
        | "*", IntLit a, IntLit b -> intExpr (a * b)
        | _ -> e
    | _ -> e

Oak() {
    AnonymousModule() {
        Value("width", Int(10))
        Value("area", InfixAppExpr(InfixAppExpr(ConstantExpr(Constant "width"), "*", Int(1)), "+", Int(0)))
        Value("total", InfixAppExpr(Int(2), "+", Int(3)))
        AppExpr("printfn", [ String("area = %d, total = %d"); Constant("area"); Constant("total") ])
    }
}
|> Rewrite.expr foldConstants
|> Gen.mkOak
|> Gen.run"""

    /// (name, script) — in menu order. The first is the default document on a fresh start.
    let all =
        [ "Hello", hello
          "Record", record
          "Union", union
          "Class", classes
          "Functions", functions
          "Rewrite", rewrite ]

    /// Starter content for File ▸ New.
    let blank =
        """open Fabulous.AST
open type Fabulous.AST.Ast

Oak() {
    AnonymousModule() {
        Value("x", Int(42))
    }
}
|> Gen.mkOak
|> Gen.run"""
