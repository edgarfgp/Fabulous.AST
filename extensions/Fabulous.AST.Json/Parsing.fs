namespace Fabulous.AST.Json

open System
open System.Globalization
open System.Collections.Generic
open System.Text.Json
open System.Text.Json.Nodes
open Fabulous.AST
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module Parsing =
    let toPascalCase (s: string) (fallback: string) =
        let s = if String.IsNullOrWhiteSpace(s) then fallback else s

        let parts =
            s
            |> Seq.map(fun c -> if Char.IsLetterOrDigit(c) then c else ' ')
            |> Seq.toArray
            |> String
            |> fun t -> t.Split([| ' ' |], StringSplitOptions.RemoveEmptyEntries)

        if parts.Length = 0 then
            fallback
        else
            parts
            |> Array.map(fun p ->
                if p.Length = 0 then
                    p
                else
                    let first = Char.ToUpper(p[0], CultureInfo.InvariantCulture)

                    if p.Length = 1 then
                        string first
                    else
                        string first + p.Substring(1))
            |> String.concat ""

    let ident(name: string) = SingleTextNode(name, Range.Zero)

    let identList(name: string) =
        IdentListNode([ IdentifierOrDot.Ident(ident name) ], Range.Zero)

    let longIdent(name: string) = Type.LongIdent(identList name)

    let listType(t: Type) : Type =
        Type.AppPostfix(TypeAppPostFixNode(t, longIdent "list", Range.Zero))

    let optionType(t: Type) : Type =
        Type.AppPostfix(TypeAppPostFixNode(t, longIdent "option", Range.Zero))

    let tryJsonValue<'T>(v: JsonValue) : bool =
        try
            let _ = v.GetValue<'T>()
            true
        with _ ->
            false

    // Functional generation state for the parser
    type GenState =
        { Emitted: Set<string>
          DeclsRev: Fantomas.Core.SyntaxOak.ModuleDecl list }

    /// Generate a ModuleOrNamespaceNode containing type declarations inferred from the given JSON input.
    /// rootNameOpt allows overriding the default root type name ("Root").
    /// nodeOptions and documentOptions allow controlling System.Text.Json parsing behavior.
    let generateModule
        (rootNameOpt: voption<string>)
        (nodeOptions: JsonNodeOptions)
        (documentOptions: JsonDocumentOptions)
        (input: string)
        : ModuleOrNamespaceNode =
        let root =
            try
                JsonNode.Parse(input, nodeOptions, documentOptions)
            with ex ->
                // Provide context about the input and preserve the original exception
                raise (System.Text.Json.JsonException($"Failed to parse JSON input in generateModule. Input: {input}", ex))

        let emptyState =
            { Emitted = Set.empty
              DeclsRev = [] }

        let addDecl (decl: ModuleDecl) (st: GenState) =
            { st with DeclsRev = decl :: st.DeclsRev }

        let markEmitted name (st: GenState) =
            { st with Emitted = st.Emitted.Add name }

        // Resolve the desired root/type names (allow override via modifier)
        let resolvedRootName =
            match rootNameOpt with
            | ValueSome rn when not(String.IsNullOrWhiteSpace rn) -> toPascalCase rn "Root"
            | _ -> "Root"

        let resolvedItemName = toPascalCase ($"{resolvedRootName}Item") "Item"

        // Forward-declared recursive functions that return a Type along with updated state
        let rec genType (suggestedName: string) (node: JsonNode) (st: GenState) : Type * GenState =
            match node with
            | :? JsonObject as obj ->
                let typeName = toPascalCase suggestedName "Anon"
                let st' = ensureRecord typeName obj st
                longIdent typeName, st'
            | :? JsonArray as arr ->
                if arr.Count = 0 then
                    // empty arrays -> obj list
                    listType(Type.LongIdent(identList "obj")), st
                else
                    match arr[0] with
                    | :? JsonObject ->
                        // Special handling: scan all objects to infer optional fields and types
                        let objs =
                            arr
                            |> Seq.choose (function
                                | :? JsonObject as o -> Some o
                                | _ -> None)
                            |> Seq.toArray

                        if objs.Length = 0 then
                            // Fallback to previous behavior if no objects
                            let first = arr[0]
                            let elemName = toPascalCase (suggestedName + "Item") "Item"
                            let elemType, st' = genType elemName first st
                            listType elemType, st'
                        else
                            // Collect union of keys preserving first-seen order
                            let keysRev, _seen =
                                (([], Set.empty<string>), objs)
                                ||> Array.fold(fun (keysRev, seen) o ->
                                    ((keysRev, seen), Seq.toArray o)
                                    ||> Array.fold(fun (keysRev, seen) p ->
                                        if seen.Contains p.Key then
                                            keysRev, seen
                                        else
                                            p.Key :: keysRev, seen.Add p.Key))

                            let keys = List.rev keysRev

                            // Determine option keys and type overrides
                            let (optionKeys, typeOverrides, st') =
                                ((Set.empty<string>, Map.empty<string, Type>, st), keys)
                                ||> List.fold(fun (optSet, overrides, stAcc) k ->
                                    let values =
                                        objs |> Array.choose(fun o -> if o.ContainsKey k then Some o[k] else None)

                                    let hasMissing = values.Length <> objs.Length
                                    // In System.Text.Json.Nodes, null values are represented by null JsonNode references
                                    let hasNull = values |> Array.exists isNull

                                    let optSet' = if hasMissing || hasNull then Set.add k optSet else optSet

                                    let sampleOpt = values |> Array.tryFind(fun v -> not(isNull v))

                                    let fieldType, stAcc' =
                                        match sampleOpt with
                                        | Some v -> genType (toPascalCase k "Field") v stAcc
                                        | None -> longIdent "obj", stAcc

                                    optSet', Map.add k fieldType overrides, stAcc')

                            // Build a representative object containing all keys (values unused due to overrides)
                            let rep = JsonObject()
                            keys |> List.iter(fun k -> rep.Add(k, JsonValue.Create ""))

                            let elemName = toPascalCase (suggestedName + "Item") "Item"
                            let st'' = ensureRecordWith elemName rep optionKeys typeOverrides st'
                            listType(longIdent elemName), st''
                    | first ->
                        let elemName = toPascalCase (suggestedName + "Item") "Item"
                        let elemType, st' = genType elemName first st
                        listType elemType, st'
            | :? JsonValue as v ->
                let t =
                    if tryJsonValue<bool> v then longIdent "bool"
                    elif tryJsonValue<int64> v then longIdent "int"
                    elif tryJsonValue<double> v then longIdent "float"
                    else longIdent "string"

                t, st
            | _ -> longIdent "obj", st

        and ensureRecordWith
            (typeName: string)
            (obj: JsonObject)
            (optionKeys: Set<string>)
            (typeOverrides: Map<string, Type>)
            (st: GenState)
            : GenState =
            if st.Emitted.Contains typeName then
                st
            else
                // Generate fields first (may recursively generate child types)
                let fieldsRev, st' =
                    (([], st), Seq.toArray obj)
                    ||> Array.fold(fun (accFields, stAcc) prop ->
                        let fname = prop.Key
                        let fnode = prop.Value

                        let baseType, stAcc' =
                            match Map.tryFind fname typeOverrides with
                            | Some t -> t, stAcc
                            | None ->
                                if isNull fnode then
                                    longIdent "string", stAcc
                                else
                                    genType (toPascalCase fname "Field") fnode stAcc

                        let ftype =
                            if optionKeys.Contains fname then
                                optionType baseType
                            else
                                baseType

                        let fieldNode =
                            FieldNode(None, None, None, None, None, Some(ident fname), ftype, Range.Zero)

                        fieldNode :: accFields, stAcc')

                // Build a record definition and turn it into a ModuleDecl node
                let fields = List.rev fieldsRev

                let typeNameNode =
                    TypeNameNode(
                        None,
                        None,
                        SingleTextNode.``type``,
                        None,
                        identList typeName,
                        None,
                        [],
                        None,
                        Some(SingleTextNode.equals),
                        None,
                        Range.Zero
                    )

                let recordNode =
                    TypeDefnRecordNode(
                        typeNameNode,
                        None,
                        SingleTextNode.leftCurlyBrace,
                        fields,
                        SingleTextNode.rightCurlyBrace,
                        [],
                        Range.Zero
                    )

                let modDecl = ModuleDecl.TypeDefn(TypeDefn.Record(recordNode))
                st' |> addDecl modDecl |> markEmitted typeName

        and ensureRecord (typeName: string) (obj: JsonObject) (st: GenState) : GenState =
            // Default behavior: no optional keys and no overrides
            ensureRecordWith typeName obj Set.empty Map.empty st

        // Helpers to build abbreviation (alias) declarations
        let mkAliasDecl name (t: Type) : ModuleDecl =
            let typeNameNode =
                TypeNameNode(
                    None,
                    None,
                    SingleTextNode.``type``,
                    None,
                    identList name,
                    None,
                    [],
                    None,
                    Some(SingleTextNode.equals),
                    None,
                    Range.Zero
                )

            TypeDefnAbbrevNode(typeNameNode, t, [], Range.Zero)
            |> TypeDefn.Abbrev
            |> ModuleDecl.TypeDefn

        // Produce top-level based on the root node
        if isNull root then
            // Invalid JSON -> nothing
            ModuleOrNamespaceNode(None, [], Range.Zero)
        else
            let finalState =
                match root with
                | :? JsonObject as o -> emptyState |> ensureRecord resolvedRootName o
                | :? JsonArray as a ->
                    if a.Count = 0 then
                        // type Root = obj list
                        let aliasType = listType(longIdent "obj")
                        emptyState |> addDecl(mkAliasDecl resolvedRootName aliasType)
                    else
                        match a[0] with
                        | :? JsonObject ->
                            // Scan all objects to compute optional fields and types
                            let objs =
                                a
                                |> Seq.choose (function
                                    | :? JsonObject as o -> Some o
                                    | _ -> None)
                                |> Seq.toArray

                            if objs.Length = 0 then
                                // Fallback to first element inference
                                let elemType, st' = genType resolvedItemName a[0] emptyState
                                st' |> addDecl(mkAliasDecl resolvedRootName (listType elemType))
                            else
                                let keysRev, _seen =
                                    (([], Set.empty<string>), objs)
                                    ||> Array.fold(fun (keysRev, seen) o ->
                                        ((keysRev, seen), Seq.toArray o)
                                        ||> Array.fold(fun (keysRev, seen) p ->
                                            if seen.Contains p.Key then
                                                keysRev, seen
                                            else
                                                p.Key :: keysRev, seen.Add p.Key))

                                let keys = List.rev keysRev

                                let (optionKeys, typeOverrides, st') =
                                    ((Set.empty<string>, Map.empty<string, Type>, emptyState), keys)
                                    ||> List.fold(fun (optSet, overrides, stAcc) k ->
                                        let values =
                                            objs |> Array.choose(fun o -> if o.ContainsKey k then Some o[k] else None)

                                        let hasMissing = values.Length <> objs.Length
                                        let hasNull = values |> Array.exists isNull
                                        let optSet' = if hasMissing || hasNull then Set.add k optSet else optSet

                                        let sampleOpt = values |> Array.tryFind(fun v -> not(isNull v))

                                        let fieldType, stAcc' =
                                            match sampleOpt with
                                            | Some v -> genType (toPascalCase k "Field") v stAcc
                                            | None -> longIdent "obj", stAcc

                                        optSet', Map.add k fieldType overrides, stAcc')

                                let rep = JsonObject()
                                keys |> List.iter(fun k -> rep.Add(k, JsonValue.Create ""))

                                let st'' = ensureRecordWith resolvedItemName rep optionKeys typeOverrides st'

                                st''
                                |> addDecl(mkAliasDecl resolvedRootName (listType(longIdent resolvedItemName)))
                        | elem ->
                            let elemType, st' = genType resolvedItemName elem emptyState
                            st' |> addDecl(mkAliasDecl resolvedRootName (listType elemType))
                | :? JsonValue as v ->
                    let t =
                        if tryJsonValue<bool> v then longIdent "bool"
                        elif tryJsonValue<int64> v then longIdent "int"
                        elif tryJsonValue<double> v then longIdent "float"
                        else longIdent "string"

                    emptyState |> addDecl(mkAliasDecl resolvedRootName t)
                | _ -> emptyState

            ModuleOrNamespaceNode(None, List.rev finalState.DeclsRev, Range.Zero)
