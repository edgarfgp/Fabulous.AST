namespace Fabulous.AST

type StringVariant =
    | SingleQuoted of string
    | DoubleQuoted of string
    | TripleQuoted of string
    | Unquoted of string

[<RequireQualifiedAccess>]
module StringVariant =
    let normalize variant =
        match variant with
        | DoubleQuoted s -> s
        | Unquoted s -> s
        | TripleQuoted s -> s
        | SingleQuoted s -> s

[<RequireQualifiedAccess>]
module StringParsing =
    /// Adds double backticks to the identifier if necessary.
    let normalizeIdentifierBackticks(variant: StringVariant) =
        match variant with
        | DoubleQuoted identifier ->
            if System.String.IsNullOrEmpty identifier then
                failwith "This is not a valid identifier"
            else
                let trimmed = identifier.Trim()
                Fantomas.FCS.Syntax.PrettyNaming.NormalizeIdentifierBackticks trimmed
        | Unquoted identifier ->
            if System.String.IsNullOrEmpty identifier then
                failwith "This is not a valid identifier"
            else
                let trimmed = identifier.Trim()
                Fantomas.FCS.Syntax.PrettyNaming.NormalizeIdentifierBackticks trimmed
        | TripleQuoted identifier ->
            if System.String.IsNullOrEmpty identifier then
                failwith "This is not a valid identifier"
            else
                let trimmed = identifier.Trim()
                Fantomas.FCS.Syntax.PrettyNaming.NormalizeIdentifierBackticks trimmed
        | SingleQuoted identifier ->
            if System.String.IsNullOrEmpty identifier then
                failwith "This is not a valid identifier"
            else
                let trimmed = identifier.Trim()
                $"'{trimmed}'"

    /// Adds quotes to the identifier if necessary.
    let normalizeIdentifierQuotes(variant: StringVariant) =
        match variant with
        | DoubleQuoted identifier ->
            if identifier = null then
                failwith "This is not a valid identifier"
            else
                $"\"{identifier}\""
        | Unquoted identifier ->
            if identifier = null then
                failwith "This is not a valid identifier"
            else
                identifier
        | TripleQuoted identifier ->
            if identifier = null then
                failwith "This is not a valid identifier"
            else
                $"\"\"\"{identifier}\"\"\""
        | SingleQuoted identifier ->
            if identifier = null then
                failwith "This is not a valid identifier"
            else
                identifier
