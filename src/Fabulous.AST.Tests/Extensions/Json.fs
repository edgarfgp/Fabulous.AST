namespace Fabulous.AST.Tests.Extensions

open Xunit
open Fabulous.AST
open Fabulous.AST.Json
open Fabulous.AST.Tests
open type Ast

module JsonGeneration =

    [<Fact>]
    let ``Generates record from simple object``() =
        Oak() { AnonymousModule() { Json("{ \"name\": \"Alice\", \"age\": 30, \"active\": true }") } }
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
                Json(json).allowTrailingCommas(true).readCommentHandling(System.Text.Json.JsonCommentHandling.Skip)
            }
        }
        |> produces
            """

type Root = { id: int; name: string }

"""

    [<Fact>]
    let ``Parses array with trailing comma when enabled``() =
        Oak() { AnonymousModule() { Json("[1,2,]").allowTrailingCommas(true) } }
        |> produces
            """

type Root = int list

"""

    [<Fact>]
    let ``Allows comments and trailing commas via serializerOptions without granular overrides``() =
        let ser =
            System.Text.Json.JsonSerializerOptions(
                AllowTrailingCommas = true,
                ReadCommentHandling = System.Text.Json.JsonCommentHandling.Skip
            )

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
