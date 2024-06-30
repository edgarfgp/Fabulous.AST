namespace Fabulous.AST.Tests

open Fantomas.Core

open Fabulous.AST

open type Ast
open Xunit

module WidgetTests =
    [<Fact>]
    let ``string -> OAK``() =
        let source =
            """
type Person() =
    static member private GetPrimitiveReader(t: System.Type, reader: Microsoft.Data.SqlClient.SqlDataReader, isOpt: bool, isNullable: bool) =
        let wrapValue get (ord: int) =
            if isOpt then (if reader.IsDBNull ord then None else get ord |> Some) |> box
            elif isNullable then (if reader.IsDBNull ord then System.Nullable() else get ord |> System.Nullable) |> box
            else get ord |> box

        let wrapRef get (ord: int) =
            if isOpt then (if reader.IsDBNull ord then None else get ord |> Some) |> box
            else get ord |> box

        if t = typedefof<System.Guid> then Some(wrapValue reader.GetGuid)
        else if t = typedefof<bool> then Some(wrapValue reader.GetBoolean)
        else if t = typedefof<int> then Some(wrapValue reader.GetInt32)
        else if t = typedefof<int64> then Some(wrapValue reader.GetInt64)
        else if t = typedefof<int16> then Some(wrapValue reader.GetInt16)
        else if t = typedefof<byte> then Some(wrapValue reader.GetByte)
        else if t = typedefof<double> then Some(wrapValue reader.GetDouble)
        else if t = typedefof<System.Single> then Some(wrapValue reader.GetFloat)
        else if t = typedefof<decimal> then Some(wrapValue reader.GetDecimal)
        else if t = typedefof<string> then Some(wrapRef reader.GetString)
        else if t = typedefof<System.DateTimeOffset> then Some(wrapValue reader.GetDateTimeOffset)
        else if t = typedefof<System.DateOnly> then Some(wrapValue reader.GetDateOnly)
        else if t = typedefof<System.TimeOnly> then Some(wrapValue reader.GetTimeOnly)
        else if t = typedefof<System.DateTime> then Some(wrapValue reader.GetDateTime)
        else if t = typedefof<byte []> then Some(wrapRef reader.GetFieldValue<byte []>)
        else if t = typedefof<obj> then Some(wrapRef reader.GetFieldValue)
        else None

"""

        let rootNode = CodeFormatter.ParseOakAsync(false, source) |> Async.RunSynchronously
        Assert.NotNull(rootNode)
