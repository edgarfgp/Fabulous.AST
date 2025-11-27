namespace Fabulous.AST.Json

open System.Runtime.CompilerServices
open System.Text.Json
open System.Text.Json.Nodes
open Fabulous.AST
open Fantomas.Core.SyntaxOak

module Json =
    let Input = Attributes.defineScalar<string> "Input"

    let RootName = Attributes.defineScalar<string> "RootName"

    let NodeOptions = Attributes.defineScalar<JsonNodeOptions> "NodeOptions"

    let SerializerOptions =
        Attributes.defineScalar<JsonSerializerOptions> "SerializerOptions"

    let AllowTrailingCommas = Attributes.defineScalar<bool> "AllowTrailingCommas"

    let ReadCommentHandling =
        Attributes.defineScalar<JsonCommentHandling> "ReadCommentHandling"

    let MaxDepth = Attributes.defineScalar<int> "MaxDepth"

    let SerializerPropertyNameCaseInsensitive =
        Attributes.defineScalar<bool> "SerializerPropertyNameCaseInsensitive"

    let NodePropertyNameCaseInsensitive =
        Attributes.defineScalar<bool> "NodePropertyNameCaseInsensitive"

    let WidgetKey =
        Widgets.register "Json" (fun widget ->
            let input = Widgets.getScalarValue widget Input
            let rootName = Widgets.tryGetScalarValue widget RootName
            let serializerOptions = Widgets.tryGetScalarValue widget SerializerOptions
            let nodeOptionsAttr = Widgets.tryGetScalarValue widget NodeOptions

            let allowTrailingCommas = Widgets.tryGetScalarValue widget AllowTrailingCommas
            let readCommentHandling = Widgets.tryGetScalarValue widget ReadCommentHandling
            let maxDepth = Widgets.tryGetScalarValue widget MaxDepth

            let serCaseInsensitive =
                Widgets.tryGetScalarValue widget SerializerPropertyNameCaseInsensitive

            let nodeCaseInsensitive =
                Widgets.tryGetScalarValue widget NodePropertyNameCaseInsensitive

            let docOptions =
                let allowTrailingCommasValue =
                    match allowTrailingCommas with
                    | ValueSome v -> v
                    | ValueNone ->
                        match serializerOptions with
                        | ValueSome so -> so.AllowTrailingCommas
                        | ValueNone -> JsonDocumentOptions().AllowTrailingCommas

                let commentHandlingValue =
                    match readCommentHandling with
                    | ValueSome v -> v
                    | ValueNone ->
                        match serializerOptions with
                        | ValueSome so -> so.ReadCommentHandling
                        | ValueNone -> JsonDocumentOptions().CommentHandling

                let maxDepthValue =
                    match maxDepth with
                    | ValueSome v -> v
                    | ValueNone ->
                        match serializerOptions with
                        | ValueSome so -> so.MaxDepth
                        | ValueNone -> JsonDocumentOptions().MaxDepth

                JsonDocumentOptions(
                    AllowTrailingCommas = allowTrailingCommasValue,
                    CommentHandling = commentHandlingValue,
                    MaxDepth = maxDepthValue
                )

            let nodeOptions =
                let baseCaseInsensitive =
                    match nodeOptionsAttr with
                    | ValueSome no -> no.PropertyNameCaseInsensitive
                    | ValueNone -> JsonNodeOptions().PropertyNameCaseInsensitive

                let caseInsensitiveValue =
                    match nodeCaseInsensitive with
                    | ValueSome v -> v
                    | ValueNone ->
                        match serCaseInsensitive with
                        | ValueSome v -> v
                        | ValueNone ->
                            match serializerOptions with
                            | ValueSome so -> so.PropertyNameCaseInsensitive
                            | ValueNone -> baseCaseInsensitive

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
    /// Overrides the default root type name ("Root") used for the generated F# declarations.
    /// </summary>
    /// <param name="value">The desired type name for the root (PascalCase recommended).</param>
    /// <param name="this">Current widget.</param>
    /// <remarks>
    /// By default, the generator names the top-level type <c>Root</c>. Use this modifier to provide
    /// a more descriptive name, e.g. <c>FunctionsResponse</c>.
    /// </remarks>
    /// <code lang="fsharp">
    /// Oak() {
    ///     AnonymousModule() {
    ///         Json("{""name"":""Alice""}")
    ///             .rootName("Person")
    ///     }
    /// }
    /// // => type Person = { name: string }
    /// </code>
    [<Extension>]
    static member inline rootName(this: WidgetBuilder<ModuleOrNamespaceNode>, value: string) =
        this.AddScalar(Json.RootName.WithValue(value))

    /// <summary>
    /// Supplies <see cref="T:System.Text.Json.JsonSerializerOptions"/> used as defaults when inferring
    /// parsing behavior (e.g., AllowTrailingCommas, ReadCommentHandling, MaxDepth, PropertyNameCaseInsensitive).
    /// </summary>
    /// <param name="options">Pre-configured <c>JsonSerializerOptions</c> instance.</param>
    /// <param name="this">Current widget.</param>
    /// <remarks>
    /// Explicit granular modifiers (e.g. <see cref="allowTrailingCommas"/>) take precedence
    /// over values coming from <c>JsonSerializerOptions</c>.
    /// </remarks>
    /// <code lang="fsharp">
    /// let ser = JsonSerializerOptions(AllowTrailingCommas = true,
    ///                                 ReadCommentHandling = JsonCommentHandling.Skip,
    ///                                 PropertyNameCaseInsensitive = true)
    ///
    /// Oak() {
    ///     AnonymousModule() {
    ///         Json("{ //comment\n \"id\":1, }")
    ///             .serializerOptions(ser)
    ///     }
    /// }
    /// // => type Root = { id: int }
    /// </code>
    [<Extension>]
    static member inline serializerOptions(this: WidgetBuilder<ModuleOrNamespaceNode>, options: JsonSerializerOptions) =
        this.AddScalar(Json.SerializerOptions.WithValue(options))

    /// <summary>
    /// Supplies <see cref="T:System.Text.Json.Nodes.JsonNodeOptions"/> controlling node parsing behavior,
    /// notably <c>PropertyNameCaseInsensitive</c>.
    /// </summary>
    /// <param name="options">Pre-configured <c>JsonNodeOptions</c> instance.</param>
    /// <param name="this">Current widget.</param>
    /// <remarks>
    /// Precedence for <c>PropertyNameCaseInsensitive</c> is:
    /// explicit <see cref="nodePropertyNameCaseInsensitive"/> modifier >
    /// <see cref="serializerPropertyNameCaseInsensitive"/> >
    /// <c>JsonSerializerOptions.PropertyNameCaseInsensitive</c> >
    /// value from the provided <c>JsonNodeOptions</c> >
    /// default.
    /// </remarks>
    /// <code lang="fsharp">
    /// let node = JsonNodeOptions(PropertyNameCaseInsensitive = true)
    /// Oak() { AnonymousModule() { Json("{ \"ID\": 1 }").nodeOptions(node) } }
    /// // => type Root = { ID: int }
    /// </code>
    [<Extension>]
    static member inline nodeOptions(this: WidgetBuilder<ModuleOrNamespaceNode>, options: JsonNodeOptions) =
        this.AddScalar(Json.NodeOptions.WithValue(options))

    /// <summary>
    /// Overrides the <c>AllowTrailingCommas</c> behavior used when parsing the JSON input.
    /// </summary>
    /// <param name="value"><c>true</c> to allow trailing commas; otherwise <c>false</c>.</param>
    /// <param name="this">Current widget.</param>
    /// <remarks>
    /// This setting takes precedence over the value coming from <see cref="serializerOptions"/>.
    /// </remarks>
    /// <code lang="fsharp">
    /// Oak() { AnonymousModule() { Json("[1,2,]").allowTrailingCommas(true) } }
    /// // => type Root = int list
    /// </code>
    [<Extension>]
    static member inline allowTrailingCommas(this: WidgetBuilder<ModuleOrNamespaceNode>, value: bool) =
        this.AddScalar(Json.AllowTrailingCommas.WithValue(value))

    /// <summary>
    /// Overrides the <c>ReadCommentHandling</c> behavior used when parsing the JSON input.
    /// </summary>
    /// <param name="value">A <see cref="T:System.Text.Json.JsonCommentHandling"/> value, e.g. <c>Skip</c>.</param>
    /// <param name="this">Current widget.</param>
    /// <remarks>
    /// This setting takes precedence over the value coming from <see cref="serializerOptions"/>.
    /// </remarks>
    /// <code lang="fsharp">
    /// Oak() {
    ///     AnonymousModule() {
    ///         Json("{ //comment\n \"id\": 1 }")
    ///             .readCommentHandling(System.Text.Json.JsonCommentHandling.Skip)
    ///     }
    /// }
    /// // => type Root = { id: int }
    /// </code>
    [<Extension>]
    static member inline readCommentHandling(this: WidgetBuilder<ModuleOrNamespaceNode>, value: JsonCommentHandling) =
        this.AddScalar(Json.ReadCommentHandling.WithValue(value))

    /// <summary>
    /// Overrides the <c>MaxDepth</c> used when parsing the JSON input.
    /// </summary>
    /// <param name="value">Maximum depth allowed when reading JSON.</param>
    /// <param name="this">Current widget.</param>
    /// <remarks>
    /// This setting takes precedence over the value coming from <see cref="serializerOptions"/>.
    /// </remarks>
    /// <code lang="fsharp">
    /// Oak() { AnonymousModule() { Json("{\"a\":{\"b\":{\"c\":1}}}").maxDepth(64) } }
    /// // => type B = { c: int }
    /// //    type A = { b: B }
    /// //    type Root = { a: A }
    /// </code>
    [<Extension>]
    static member inline maxDepth(this: WidgetBuilder<ModuleOrNamespaceNode>, value: int) =
        this.AddScalar(Json.MaxDepth.WithValue(value))

    /// <summary>
    /// Overrides <c>PropertyNameCaseInsensitive</c> as read from <see cref="JsonSerializerOptions"/>.
    /// Affects how object property names are matched while parsing.
    /// </summary>
    /// <param name="value">Whether matching is case-insensitive.</param>
    /// <param name="this">Current widget.</param>
    /// <remarks>
    /// Precedence: this modifier > <see cref="Json.serializerOptions"/> value > defaults.
    /// Use <see cref="nodePropertyNameCaseInsensitive"/> to target <c>JsonNodeOptions</c> explicitly.
    /// </remarks>
    /// <code lang="fsharp">
    /// Oak() { AnonymousModule() { Json("{ \"ID\": 1 }").serializerPropertyNameCaseInsensitive(true) } }
    /// // => type Root = { ID: int }
    /// </code>
    [<Extension>]
    static member inline serializerPropertyNameCaseInsensitive
        (this: WidgetBuilder<ModuleOrNamespaceNode>, value: bool)
        =
        this.AddScalar(Json.SerializerPropertyNameCaseInsensitive.WithValue(value))

    /// <summary>
    /// Overrides <c>PropertyNameCaseInsensitive</c> for the underlying <see cref="T:System.Text.Json.Nodes.JsonNodeOptions"/>.
    /// </summary>
    /// <param name="value">Whether matching is case-insensitive.</param>
    /// <param name="this">Current widget.</param>
    /// <remarks>
    /// Precedence: this modifier > <see cref="serializerPropertyNameCaseInsensitive"/> >
    /// <c>JsonSerializerOptions.PropertyNameCaseInsensitive</c> > provided <c>JsonNodeOptions</c> > default.
    /// </remarks>
    /// <code lang="fsharp">
    /// Oak() { AnonymousModule() { Json("{ \"ID\": 1 }").nodePropertyNameCaseInsensitive(true) } }
    /// // => type Root = { ID: int }
    /// </code>
    [<Extension>]
    static member inline nodePropertyNameCaseInsensitive(this: WidgetBuilder<ModuleOrNamespaceNode>, value: bool) =
        this.AddScalar(Json.NodePropertyNameCaseInsensitive.WithValue(value))
