namespace Fabulous.AST.Tests.Extensions

open System.Text.Json
open Xunit
open Fabulous.AST
open Fabulous.AST.Json
open Fabulous.AST.Tests
open type Ast

module JsonGeneration =

    [<Fact>]
    let ``Generates record from simple object``() =
        Oak() {
            let json =
                """
{
    "name": "Alice",
    "age": 30,
    "active": true
}
    """

            AnonymousModule() { Json(json) }
        }
        |> produces
            """

type Root =
    { name: string; age: int; active: bool }

"""

    [<Fact>]
    let ``Generates nested records``() =
        Oak() { AnonymousModule() { Json("{ \"user\": { \"name\": \"Alice\" } }") } }
        |> produces
            """

type User = { name: string }
type Root = { user: User }

"""

    [<Fact>]
    let ``Generates alias for root array and element record``() =
        Oak() { AnonymousModule() { Json("[ { \"id\": 1 } ]") } }
        |> produces
            """

type RootItem = { id: int }
type Root = RootItem list

"""

    [<Fact>]
    let ``Generates alias for root string``() =
        Oak() { AnonymousModule() { Json("\"hello\"") } }
        |> produces
            """

type Root = string

"""

    [<Fact>]
    let ``Generates complex nested records``() =
        Oak() {
            AnonymousModule() {
                Json(
                    "{ \"user\": { \"name\": \"Alice\", \"address\": { \"street\": \"Main\", \"zip\": \"12345\" }, \"orders\": [ { \"id\": 1, \"items\": [ { \"sku\": \"A\", \"qty\": 2 }, { \"sku\": \"B\", \"qty\": 1 } ] } ] }, \"active\": true, \"scores\": [1, 2, 3] }"
                )
            }
        }
        |> produces
            """

type Address = { street: string; zip: string }
type ItemsItem = { sku: string; qty: int }
type OrdersItem = { id: int; items: ItemsItem list }

type User =
    { name: string
      address: Address
      orders: OrdersItem list }

type Root =
    { user: User
      active: bool
      scores: int list }

"""

    [<Fact>]
    let ``Marks fields optional across array of objects when missing or null``() =
        Oak() { AnonymousModule() { Json("[ { \"id\": 1, \"name\": \"Alice\" }, { \"id\": null } ]") } }
        |> produces
            """

type RootItem = { id: int option; name: string option }
type Root = RootItem list

"""

    [<Fact>]
    let ``Parses with comments and trailing commas when enabled``() =
        let json =
            """
{
  // a comment
  "id": 1,
  "name": "Alice", // trailing comma
}
"""

        Oak() {
            AnonymousModule() {
                Json(json).documentAllowTrailingCommas(true).documentCommentHandling(JsonCommentHandling.Skip)
            }
        }
        |> produces
            """

type Root = { id: int; name: string }

"""

    [<Fact>]
    let ``Parses array with trailing comma when enabled``() =
        Oak() { AnonymousModule() { Json("[1,2,]").documentAllowTrailingCommas(true) } }
        |> produces
            """

type Root = int list

"""

    [<Fact>]
    let ``Allows comments and trailing commas via serializerOptions without granular overrides``() =
        let ser =
            JsonSerializerOptions(AllowTrailingCommas = true, ReadCommentHandling = JsonCommentHandling.Skip)

        let json =
            """
{
  // comment is allowed by serializer options
  "id": 1,
}
"""

        Oak() { AnonymousModule() { Json(json).serializerOptions(ser) } }
        |> produces
            """

type Root = { id: int }

"""

    [<Fact>]
    let ``Overrides root type name via modifier``() =
        Oak() { AnonymousModule() { Json("{ \"id\": 1 }").rootName("Person") } }
        |> produces
            """

type Person = { id: int }

"""

    [<Fact>]
    let ``Falls back to obj list for empty arrays``() =
        Oak() { AnonymousModule() { Json("{ \"items\": [] }") } }
        |> produces
            """

type Root = { items: obj list }

"""

    [<Fact>]
    let ``Array of objects treats differently-cased keys as distinct by default and marks optional``() =
        Oak() { AnonymousModule() { Json("[ { \"ID\": 1 }, { \"id\": 2 } ]") } }
        |> produces
            """

type RootItem = { ID: int option; id: int option }
type Root = RootItem list

"""

    [<Fact>]
    let ``Case-insensitive node option does not merge keys but makes both non-optional``() =
        Oak() { AnonymousModule() { Json("[ { \"ID\": 1 }, { \"id\": 2 } ]").nodePropertyNameCaseInsensitive(true) } }
        |> produces
            """

type RootItem = { ID: int; id: int }
type Root = RootItem list

"""

    [<Fact>]
    let ``Generates types for AWS Lambda listFunctions result``() =
        let json =
            """
{
  "Functions": [
    {
      "FunctionName": "playground-stage-sayHello",
      "FunctionArn": "function:playground-stage-sayHello",
      "Runtime": "dotnet8",
      "Role": "arn:aws:iam::role/stage-playgroundstagesayHelloServiceRole",
      "Handler": "Playground::Playground.Handlers::sayHello",
      "CodeSize": 1433013,
      "Description": "",
      "Timeout": 30,
      "MemorySize": 128,
      "LastModified": "2025-10-09T21:04:17.915+0000",
      "CodeSha256": "65kj8eiE/j+T5cOLIE2TNQwrfJ54AN2tArpz+Bzw8DU=",
      "Version": "$LATEST",
      "TracingConfig": {
        "Mode": "PassThrough"
      },
      "RevisionId": "3ee21dc5-dd18-43aa-9935-2c4f759fe609",
      "PackageType": "Zip",
      "Architectures": [
        "x86_64"
      ],
      "EphemeralStorage": {
        "Size": 512
      },
      "SnapStart": {
        "ApplyOn": "None",
        "OptimizationStatus": "Off"
      },
      "LoggingConfig": {
        "LogFormat": "Text",
        "LogGroup": "/aws/lambda/playground-stage-sayHello"
      }
    },
    {
      "FunctionName": "Playground-SayHello",
      "FunctionArn": "function:Playground-SayHello",
      "Runtime": "dotnet8",
      "Role": "arn:aws:iam::role/MyFirstStack-PlaygroundSayHelloServiceRole",
      "Handler": "Playground::Playground.Handlers::sayHello",
      "CodeSize": 1433013,
      "Description": "A simple hello world lambda",
      "Timeout": 30,
      "MemorySize": 256,
      "LastModified": "2025-10-09T20:34:20.000+0000",
      "CodeSha256": "65kj8eiE/j+T5cOLIE2TNQwrfJ54AN2tArpz+Bzw8DU=",
      "Version": "$LATEST",
      "VpcConfig": {
        "SubnetIds": [],
        "SecurityGroupIds": [],
        "VpcId": "",
        "Ipv6AllowedForDualStack": false
      },
      "TracingConfig": {
        "Mode": "PassThrough"
      },
      "RevisionId": "aa88cba3-5bdf-4bda-9cf3-620a22b7f981",
      "PackageType": "Zip",
      "Architectures": [
        "x86_64"
      ],
      "EphemeralStorage": {
        "Size": 512
      },
      "SnapStart": {
        "ApplyOn": "None",
        "OptimizationStatus": "Off"
      },
      "LoggingConfig": {
        "LogFormat": "Text",
        "LogGroup": "/aws/lambda/Playground-SayHello"
      }
    },
    {
      "FunctionName": "playground-prod-sayHello",
      "FunctionArn": "function:playground-prod-sayHello",
      "Runtime": "dotnet8",
      "Role": "prod-playgroundprodsayHelloServiceRoleDA20BB37",
      "Handler": "Playground::Playground.Handlers::sayHello",
      "CodeSize": 1433013,
      "Description": "",
      "Timeout": 30,
      "MemorySize": 128,
      "LastModified": "2025-10-09T21:05:16.677+0000",
      "CodeSha256": "65kj8eiE/+Bzw8DU=",
      "Version": "$LATEST",
      "TracingConfig": {
        "Mode": "PassThrough"
      },
      "RevisionId": "ee213c08-34db-4e0e-9830-6392a8d0d989",
      "PackageType": "Zip",
      "Architectures": [
        "x86_64"
      ],
      "EphemeralStorage": {
        "Size": 512
      },
      "SnapStart": {
        "ApplyOn": "None",
        "OptimizationStatus": "Off"
      },
      "LoggingConfig": {
        "LogFormat": "Text",
        "LogGroup": "/aws/lambda/playground-prod-sayHello"
      }
    }
  ]
}
"""

        Oak() { AnonymousModule() { Json(json) } }
        |> produces
            """

type TracingConfig = { Mode: string }
type EphemeralStorage = { Size: int }

type SnapStart =
    { ApplyOn: string
      OptimizationStatus: string }

type LoggingConfig = { LogFormat: string; LogGroup: string }

type VpcConfig =
    { SubnetIds: obj list
      SecurityGroupIds: obj list
      VpcId: string
      Ipv6AllowedForDualStack: bool }

type FunctionsItem =
    { FunctionName: string
      FunctionArn: string
      Runtime: string
      Role: string
      Handler: string
      CodeSize: int
      Description: string
      Timeout: int
      MemorySize: int
      LastModified: string
      CodeSha256: string
      Version: string
      TracingConfig: TracingConfig
      RevisionId: string
      PackageType: string
      Architectures: string list
      EphemeralStorage: EphemeralStorage
      SnapStart: SnapStart
      LoggingConfig: LoggingConfig
      VpcConfig: VpcConfig option }

type Root = { Functions: FunctionsItem list }

"""

    [<Fact>]
    let ``Handles field names starting with digits``() =
        Oak() { AnonymousModule() { Json("{ \"123field\": \"value\", \"1st\": 1 }") } }
        |> produces
            """

type Root = { _123field: string; _1st: int }

"""

    [<Fact>]
    let ``Handles large int64 numbers correctly``() =
        Oak() { AnonymousModule() { Json("{ \"bigNum\": 9223372036854775807 }") } }
        |> produces
            """

type Root = { bigNum: int64 }

"""

    [<Fact>]
    let ``Handles floating point numbers``() =
        Oak() { AnonymousModule() { Json("{ \"price\": 123.456789012345 }") } }
        |> produces
            """

type Root = { price: float }

"""

    [<Fact>]
    let ``Handles null-only fields as obj option``() =
        Oak() { AnonymousModule() { Json("{ \"data\": null }") } }
        |> produces
            """

type Root = { data: obj }

"""

    [<Fact>]
    let ``Handles type name starting with digit in nested object``() =
        Oak() { AnonymousModule() { Json("{ \"123nested\": { \"value\": 1 } }") } }
        |> produces
            """

type _123nested = { value: int }
type Root = { _123nested: _123nested }

"""

    [<Fact>]
    let ``Escapes F# reserved keywords in field names``() =
        Oak() { AnonymousModule() { Json("{ \"type\": \"post\", \"module\": \"core\", \"match\": true }") } }
        |> produces
            """

type Root =
    { ``type``: string
      ``module``: string
      ``match``: bool }

"""

    [<Fact>]
    let ``Handles field names with special characters``() =
        Oak() { AnonymousModule() { Json("{ \"field:name\": 1, \"field}name\": 2, \"field//comment\": 3 }") } }
        |> produces
            """

type Root =
    { ``field:name``: int
      ``field}name``: int
      ``field//comment``: int }

"""

    [<Fact>]
    let ``Handles field names with backticks``() =
        Oak() { AnonymousModule() { Json("{ \"field``name\": 1 }") } }
        |> produces
            """

type Root = { ``field````name``: int }

"""

    [<Fact>]
    let ``Handles field names with spaces``() =
        Oak() { AnonymousModule() { Json("{ \"field name\": 1, \"another field\": 2 }") } }
        |> produces
            """

type Root =
    { ``field name``: int
      ``another field``: int }

"""

    [<Fact>]
    let ``Normal field names are not wrapped in backticks``() =
        Oak() { AnonymousModule() { Json("{ \"normalName\": 1, \"another_field\": 2 }") } }
        |> produces
            """

type Root = { normalName: int; another_field: int }

"""
