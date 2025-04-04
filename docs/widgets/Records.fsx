(**
---
title: Records
category: widgets
index: 13
---
*)

(**
# Records

## Contents
- [Overview](#overview)
- [Basic Usage](#basic-usage)
- [Record Fields](#record-fields)
- [Type Parameters](#type-parameters)
- [Record Attributes](#record-attributes)
- [Access Modifiers](#access-modifiers)
- [XML Documentation](#xml-documentation)
- [Record Members](#record-members)
- [Mutable Fields](#mutable-fields)
- [Creating Records in F#](#creating-records-in-f)
- [Copy and Update Record Expressions](#copy-and-update-record-expressions)
- [Mutually Recursive Records](#mutually-recursive-records)
- [Pattern Matching with Records](#pattern-matching-with-records)

## Overview
Records in F# represent simple aggregates of named values, optionally with members. They can be either structs or reference types (reference types by default). Records provide automatic equality and comparison, and they're immutable by default.

## Basic Usage
Create a record with the `Record` widget:
*)

#r "../../src/Fabulous.AST/bin/Release/netstandard2.1/publish/Fantomas.Core.dll"
#r "../../src/Fabulous.AST/bin/Release/netstandard2.1/publish/Fabulous.AST.dll"
#r "../../src/Fabulous.AST/bin/Release/netstandard2.1/publish/Fantomas.FCS.dll"

open Fabulous.AST
open type Fabulous.AST.Ast

Oak() {
    AnonymousModule() {
        Record("Point") {
            Field("X", Float())
            Field("Y", Float())
            Field("Z", Float())
        }
    }
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)

(**
## Record Fields
Add fields to records with the `Field` widget. Each field has a name and a type:
*)

Oak() {
    AnonymousModule() {
        Record("Person") {
            Field("Name", String())
            Field("Age", Int())
            Field("Address", String())
        }
    }
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)

(**
## Type Parameters
Add type parameters to records with the `typeParams` method:
*)

Oak() {
    AnonymousModule() {
        Record("KeyValuePair") {
            Field("Key", LongIdent("'K"))
            Field("Value", LongIdent("'V"))
        }
        |> _.typeParams(PostfixList([ "'K"; "'V" ]))
    }
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)

(**
## Attributes
Add attributes to records with the `attribute` or `attributes` methods:
*)

Oak() {
    AnonymousModule() {
        // Create a struct record
        Record("StructPoint") {
            Field("X", Float())
            Field("Y", Float())
        }
        |> _.attribute(Attribute("Struct"))

        // Create a record with reference equality
        Record("ReferencePoint") {
            Field("X", Float())
            Field("Y", Float())
        }
        |> _.attribute(Attribute("ReferenceEquality"))
    }
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)

(**
## Access Modifiers
Set access modifiers for records with the `toPublic`, `toPrivate`, and `toInternal` methods:
*)

Oak() {
    AnonymousModule() {
        Record("PublicRecord") {
            Field("Field1", Int())
            Field("Field2", String())
        }
        |> _.toPublic()

        Record("PrivateRecord") {
            Field("Field1", Int())
            Field("Field2", String())
        }
        |> _.toPrivate()

        Record("InternalRecord") {
            Field("Field1", Int())
            Field("Field2", String())
        }
        |> _.toInternal()
    }
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)

(**
## XML Documentation
Add XML documentation to records with the `xmlDocs` method:
*)

Oak() {
    AnonymousModule() {
        Record("DocumentedRecord") {
            Field("Field1", Int())
            Field("Field2", String())
        }
        |> _.xmlDocs([ "A well-documented record type"; "Use this for important data" ])
    }
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)

(**
## Members
Add members to records with the `members` method:
*)

Oak() {
    AnonymousModule() {
        (Record("Point") {
            Field("X", Float())
            Field("Y", Float())
        })
            .members() {
            Member(
                "this.ToString()",
                AppExpr("sprintf", [ String("Point(%f, %f)"); Constant "this.X"; Constant "this.Y" ])
            )

            Member(
                "this.Magnitude",
                AppExpr(
                    "sqrt",
                    InfixAppExpr(InfixAppExpr("this.X", "*", "this.X"), "+", InfixAppExpr("this.Y", "*", "this.Y"))
                )
            )

            Member("Default", RecordExpr([ RecordFieldExpr("X", Float(0.0)); RecordFieldExpr("Y", Float(0.0)) ]))
                .toStatic()
        }
    }
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)

(**
## Mutable Fields
Create mutable fields in records:
*)

Oak() {
    AnonymousModule() {
        Record("MutableRecord") {
            Field("Id", Int()).toMutable()
            Field("Name", String()).toMutable()
            Field("Age", Int()).toMutable()
        }
    }
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"

// produces the following code:
(*** include-output ***)

(**
## Creating Records
When using the generated F# code, you can create records using a record expression:
*)

Oak() {
    AnonymousModule() {
        // Using the Point record defined earlier
        Value("myPoint", RecordExpr([ RecordFieldExpr("X", Float(1.0)); RecordFieldExpr("Y", Float(2.0)) ]))
    }
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"
// produces the following code:
(*** include-output ***)

(**
## Copy and Update Records
F# allows you to create a new record from an existing one using the "copy and update" expression:
*)

Oak() {
    AnonymousModule() {
        // Using the Point record defined earlier
        Value("myPoint", RecordExpr("myPoint", [ RecordFieldExpr("Y", Float(3.0)) ]))
    }
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"
// produces the following code:
(*** include-output ***)

(**
## Mutually Recursive Records
Create mutually recursive records using the `and` keyword in F#:
 *)

Oak() {
    AnonymousModule() {
        Record("Person") {
            Field("Name", String())
            Field("Age", Int())
            Field("Address", LongIdent("Address"))
        }

        Record("Address") {
            Field("Line1", String())
            Field("Line2", String())
            Field("Occupant", LongIdent("Person"))
        }
        |> _.toRecursive()
    }
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"
// produces the following code:
(*** include-output ***)

(**
## Pattern Matching:
*)

Oak() {
    AnonymousModule() {
        Value(
            "point",
            RecordExpr(
                [ RecordFieldExpr("X", Float(10.0))
                  RecordFieldExpr("Y", Float(0.0))
                  RecordFieldExpr("Z", Float(-1.0)) ]
            )
        )

        Value(
            "matchPoint",
            MatchExpr(
                "point",
                [ MatchClauseExpr(
                      RecordPat(
                          [ RecordFieldPat("X", Float(0.0))
                            RecordFieldPat("Y", Float(0.0))
                            RecordFieldPat("Z", Float(0.0)) ]
                      ),
                      AppExpr("printfn", [ (String("Point is at the origin.")) ])
                  )
                  MatchClauseExpr(
                      RecordPat(
                          [ RecordFieldPat("X", "x")
                            RecordFieldPat("Y", Float(0.0))
                            RecordFieldPat("Z", Float(0.0)) ]
                      ),
                      AppExpr("printfn", [ String("Point is on the x-axis. Value is %f."); (Constant "x") ])
                  )
                  MatchClauseExpr(
                      RecordPat(
                          [ RecordFieldPat("X", Constant "x")
                            RecordFieldPat("Y", Constant "y")
                            RecordFieldPat("Z", "z") ]
                      ),
                      AppExpr(
                          "printfn",
                          [ String("Point is at (%f, %f, %f).")
                            (Constant "x")
                            (Constant "y")
                            (Constant "z") ]
                      )
                  ) ]
            )
        )
    }
}
|> Gen.mkOak
|> Gen.run
|> printfn "%s"
// produces the following code:
(*** include-output ***)
