(**
---
title: Records
category: widgets
index: 5
---
*)

(**
# Records
*)

(**
## Constructors
| Constructors                                      | Description |
|---------------------------------------------------|-------------|
| Record(name: string) | Creates a new record with the specified name. |

## Properties
| Properties            | Description |
|-----------------------|-------------|
| members() | Sets the members of the record. |
| xmlDocs(, xmlDocs: string list) | Adds XML documentation to the record. |
| typeParams(typeParams: string list) | Adds type parameters to the record. |
| attributes(attributes: WidgetBuilder<AttributeNode> list) | Adds attributes to the record. |
| attributes(attributes: string list) | Adds attributes to the record. |
| attribute(attribute: WidgetBuilder<AttributeNode>) | Adds an attribute to the record. |
| attribute(attribute: string) | Adds an attribute to the record. |
| toPrivate() | Makes the record private. |
| toInternal() | Makes the record internal. |
| toPublic() | Makes the record public. |
*)

(**

## Usage

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
        (Record("Person") {
            Field("Name", "string")
            Field("Age", "int")
            Field("Address", "string")
        })
            .xmlDocs([ "Normal record" ])
            .members (
                [ let parameters =
                      TuplePat(
                          [ ParameterPat("name", "string")
                            ParameterPat("age", "int")
                            ParameterPat("address", "string") ]
                      )

                  Method(
                      "Create",
                      parameters,
                      RecordExpr() {
                          RecordFieldExpr("Name", ConstantExpr(Unquoted "name"))
                          RecordFieldExpr("Age", ConstantExpr(Unquoted "age"))
                          RecordFieldExpr("Address", ConstantExpr(Unquoted "address"))
                      }
                  )
                      .toStatic ()

                  Property("this.NameValue", ConstantExpr(Unquoted "this.Name")) ]
            )

        (Record("Person") {
            Field("Name", "string")
            Field("Age", "int")
            Field("Address", "string")
        })
            .typeParams([ "'T" ])
            .xmlDocs ([ "Generic record" ])
    }
}
|> Gen.mkOak
|> CodeFormatter.FormatOakAsync
|> Async.RunSynchronously
|> printfn "%s"

(**
Will output the following code:
*)

/// Normal record
type Person =
    { Name: string
      Age: int
      Address: string }

    static member Create(name: string, age: int, address: string) =
        { Name = name
          Age = age
          Address = address }

    member this.NameValue = this.Name

/// Generic record
type Person<'T> =
    { Name: string
      Age: int
      Address: string }
