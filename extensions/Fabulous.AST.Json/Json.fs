namespace Fabulous.AST.Json

open System.Runtime.CompilerServices
open System.Text.Json
open System.Text.Json.Nodes
open Fabulous.AST
open Fantomas.Core.SyntaxOak

module Json =
    let Input = Attributes.defineScalar<string> "Input"

    let RootName = Attributes.defineScalar<string> "RootName"

    let NodePropertyNameCaseInsensitive =
        Attributes.defineScalar<bool> "PropertyNameCaseInsensitive"

    let DocumentAllowTrailingCommas =
        Attributes.defineScalar<bool> "DocumentAllowTrailingCommas"

    let DocumentCommentHandling =
        Attributes.defineScalar<JsonCommentHandling> "DocumentReadCommentHandling"

    let DocumentMaxDepth = Attributes.defineScalar<int> "DocumentMaxDepth"

    let DocumentOptions = Attributes.defineScalar<JsonDocumentOptions> "DocumentOptions"

    let SerializerOptions =
        Attributes.defineScalar<JsonSerializerOptions> "SerializerOptions"

    let WidgetKey =
        Widgets.register "Json" (fun widget ->
            let input = Widgets.getScalarValue widget Input
            let rootName = Widgets.tryGetScalarValue widget RootName
            let serializerOptions = Widgets.tryGetScalarValue widget SerializerOptions

            let nodePropertyNameCaseInsensitive =
                Widgets.tryGetScalarValue widget NodePropertyNameCaseInsensitive

            let documentOptions = Widgets.tryGetScalarValue widget DocumentOptions

            let documentAllowTrailingCommas =
                Widgets.tryGetScalarValue widget DocumentAllowTrailingCommas

            let commentHandling = Widgets.tryGetScalarValue widget DocumentCommentHandling
            let maxDepth = Widgets.tryGetScalarValue widget DocumentMaxDepth

            if isNull input || input.Trim().Length = 0 then
                invalidArg "json" "JSON input must not be null or empty."

            // Build JsonDocumentOptions with clear precedence (high → low):
            // 1) Per-scalar overrides (documentAllowTrailingCommas, commentHandling, maxDepth)
            // 2) DocumentOptions (as a seed if provided)
            // 3) SerializerOptions mapped to JsonDocumentOptions (as a seed if DocumentOptions not provided)
            // 4) Defaults from new JsonDocumentOptions()
            let mutable docOptions = JsonDocumentOptions()

            // Seed from DocumentOptions if provided
            documentOptions |> ValueOption.iter(fun o -> docOptions <- o)

            // If DocumentOptions not supplied, seed from SerializerOptions
            match documentOptions, serializerOptions with
            | ValueSome _, _ -> ()
            | ValueNone, ValueSome s ->
                docOptions.AllowTrailingCommas <- s.AllowTrailingCommas
                docOptions.CommentHandling <- s.ReadCommentHandling
                docOptions.MaxDepth <- s.MaxDepth
            | ValueNone, ValueNone -> ()

            // Finally, explicit per-scalar overrides always win
            documentAllowTrailingCommas
            |> ValueOption.iter(fun v -> docOptions.AllowTrailingCommas <- v)

            commentHandling |> ValueOption.iter(fun v -> docOptions.CommentHandling <- v)
            maxDepth |> ValueOption.iter(fun v -> docOptions.MaxDepth <- v)

            let nodeOptions =
                let caseInsensitiveValue =
                    nodePropertyNameCaseInsensitive |> ValueOption.defaultValue false

                JsonNodeOptions(PropertyNameCaseInsensitive = caseInsensitiveValue)

            Parsing.generateModule rootName nodeOptions docOptions input)

[<AutoOpen>]
module JsonBuilders =
    type Ast with
        /// <summary>
        /// Parses a JSON string and generates F# types (records and aliases) that model its shape.
        /// The resulting declarations can be yielded inside Modules/Namespaces.
        /// </summary>
        /// <param name="json">JSON input text.</param>
        /// <code lang="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         Json("{""name"":""Alice"",""age"":30}")
        ///     }
        /// }
        /// </code>
        static member Json(json: string) =
            WidgetBuilder<ModuleOrNamespaceNode>(Json.WidgetKey, Json.Input.WithValue(json))

type JsonModifiers =
    /// <summary>
    /// Set the root F# type name (default is "Root").
    /// </summary>
    /// <param name="this">Current widget.</param>
    /// <param name="value">Root type name to use.</param>
    /// <code lang="fsharp">
    /// Oak() {
    ///     AnonymousModule() {
    ///         Json("{\"id\":1}").rootName("Person")
    ///     }
    /// }
    /// </code>
    [<Extension>]
    static member inline rootName(this: WidgetBuilder<ModuleOrNamespaceNode>, value: string) =
        this.AddScalar(Json.RootName.WithValue(value))

    /// <summary>
    /// Parse property names case-insensitively.
    /// </summary>
    /// <param name="this">Current widget.</param>
    /// <param name="value">Set to true to make property name matching ignore case.</param>
    /// <code lang="fsharp">
    /// Oak() {
    ///     AnonymousModule() {
    ///         Json("[ { \"ID\": 1 }, { \"id\": 2 } ]").nodePropertyNameCaseInsensitive(true)
    ///     }
    /// }
    /// </code>
    [<Extension>]
    static member inline nodePropertyNameCaseInsensitive(this: WidgetBuilder<ModuleOrNamespaceNode>, value: bool) =
        this.AddScalar(Json.NodePropertyNameCaseInsensitive.WithValue(value))

    /// <summary>
    /// Use a JsonDocumentOptions instance for parsing.
    /// </summary>
    /// <param name="this">Current widget.</param>
    /// <param name="options">JsonDocumentOptions to apply.</param>
    /// <code lang="fsharp">
    /// let opts = JsonDocumentOptions(AllowTrailingCommas = true, CommentHandling = JsonCommentHandling.Skip)
    /// Oak() {
    ///     AnonymousModule() { Json("{ // ok\n  \"id\": 1,\n}").documentOptions(opts) }
    /// }
    /// </code>
    [<Extension>]
    static member inline documentOptions(this: WidgetBuilder<ModuleOrNamespaceNode>, options: JsonDocumentOptions) =
        this.AddScalar(Json.DocumentOptions.WithValue(options))

    /// <summary>
    /// Allow or disallow trailing commas in the JSON input.
    /// </summary>
    /// <param name="this">Current widget.</param>
    /// <param name="value">Set to true to allow trailing commas.</param>
    /// <code lang="fsharp">
    /// Oak() {
    ///     AnonymousModule() { Json("[1,2,]").documentAllowTrailingCommas(true) }
    /// }
    /// </code>
    [<Extension>]
    static member inline documentAllowTrailingCommas(this: WidgetBuilder<ModuleOrNamespaceNode>, value: bool) =
        this.AddScalar(Json.DocumentAllowTrailingCommas.WithValue(value))

    /// <summary>
    /// Set how comments are handled while parsing.
    /// </summary>
    /// <param name="this">Current widget.</param>
    /// <param name="value">Comment handling mode (e.g., JsonCommentHandling.Skip).</param>
    /// <code lang="fsharp">
    /// Oak() {
    ///     AnonymousModule() {
    ///         Json("{ // comment\n  \"id\": 1 }").documentCommentHandling(JsonCommentHandling.Skip)
    ///     }
    /// }
    /// </code>
    [<Extension>]
    static member inline documentCommentHandling
        (this: WidgetBuilder<ModuleOrNamespaceNode>, value: JsonCommentHandling)
        =
        this.AddScalar(Json.DocumentCommentHandling.WithValue(value))

    /// <summary>
    /// Set the maximum depth for reading JSON (0 uses framework default).
    /// </summary>
    /// <param name="this">Current widget.</param>
    /// <param name="value">Maximum depth.</param>
    /// <code lang="fsharp">
    /// Oak() { AnonymousModule() { Json("{\"id\":1}").documentMaxDepth(64) } }
    /// </code>
    [<Extension>]
    static member inline documentMaxDepth(this: WidgetBuilder<ModuleOrNamespaceNode>, value: int) =
        this.AddScalar(Json.DocumentMaxDepth.WithValue(value))

    /// <summary>
    /// Seed parsing behavior from a JsonSerializerOptions instance.
    /// </summary>
    /// <param name="this">Current widget.</param>
    /// <param name="options">JsonSerializerOptions whose related members are used.</param>
    /// <code lang="fsharp">
    /// let ser = JsonSerializerOptions(AllowTrailingCommas = true, ReadCommentHandling = JsonCommentHandling.Skip)
    /// Oak() { AnonymousModule() { Json("{ /* ok */ \"id\": 1, }").serializerOptions(ser) } }
    /// </code>
    [<Extension>]
    static member inline serializerOptions(this: WidgetBuilder<ModuleOrNamespaceNode>, options: JsonSerializerOptions) =
        this.AddScalar(Json.SerializerOptions.WithValue(options))
