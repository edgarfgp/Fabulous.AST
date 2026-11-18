namespace AstEditor

open System.Text
open FSharp.Compiler.Syntax
open FSharp.Compiler.Text

/// Renders the untyped F# syntax tree (from FCS) of the generated code as a compact,
/// human-readable tree — the AST inspector's view of what the DSL built. It summarises the high-level structure
/// (modules, type definitions, fields/cases, bindings, top-level expressions) rather than
/// dumping every node, and is a hand-written walk (no reflection) so the labels stay meaningful.
module SyntaxTree =

    /// A source span: 1-based start/end line, 0-based start/end column (FCS conventions).
    type Span =
        { StartLine: int
          StartColumn: int
          EndLine: int
          EndColumn: int }

    /// The rendered tree plus, for each line, the span of source that node covers — so
    /// clicking a tree line can select the code it came from.
    type Tree =
        { Text: string
          Spans: Span option[] }

    /// A node in the tree: a label, the source range it covers (if any), and children.
    type private Node =
        { Label: string
          Range: range option
          Children: Node list }

    let private leaf label =
        { Label = label
          Range = None
          Children = [] }

    let private leafR label (r: range) =
        { Label = label
          Range = Some r
          Children = [] }

    let private nodeR label (r: range) children =
        { Label = label
          Range = Some r
          Children = children }

    let private identText(id: Ident) = id.idText

    let private longText(ids: LongIdent) =
        ids |> List.map identText |> String.concat "."

    let private synLongText(SynLongIdent(ids, _, _)) = longText ids

    /// A shallow, readable rendering of a type (field/parameter types).
    let rec private typeText(t: SynType) : string =
        match t with
        | SynType.LongIdent lid -> synLongText lid
        | SynType.App(typeName = tn; typeArgs = args) ->
            let argText = args |> List.map typeText |> String.concat ", "
            $"{typeText tn}<{argText}>"
        | SynType.Fun(argType = a; returnType = b) -> $"{typeText a} -> {typeText b}"
        | SynType.Tuple _ -> "tuple"
        | SynType.Array(elementType = e) -> $"{typeText e}[]"
        | SynType.Var(typar = SynTypar(id, _, _)) -> "'" + identText id
        | SynType.Paren(innerType = inner) -> typeText inner
        | _ -> "_"

    /// The bound name of a let binding's head pattern.
    let rec private patText(p: SynPat) : string =
        match p with
        | SynPat.Named(ident = SynIdent(id, _)) -> identText id
        | SynPat.LongIdent(longDotId = lid) -> synLongText lid
        | SynPat.Typed(pat = inner) -> patText inner
        | SynPat.Paren(pat = inner) -> patText inner
        | SynPat.Wild _ -> "_"
        | _ -> "_"

    /// A literal's rendered value.
    let private constText(c: SynConst) : string =
        match c with
        | SynConst.String(text = s) -> "\"" + s + "\""
        | SynConst.Int32 n -> string n
        | SynConst.Bool b -> (if b then "true" else "false")
        | SynConst.Double d -> string d
        | SynConst.Single f -> string f
        | SynConst.Char ch -> "'" + string ch + "'"
        | SynConst.Unit -> "()"
        | _ -> "const"

    let private isUnit(e: SynExpr) =
        match e with
        | SynExpr.Const(constant = SynConst.Unit) -> true
        | _ -> false

    /// The function-name label at the head of an application spine (e.g. "Record").
    let rec private headLabel(e: SynExpr) : string =
        match e with
        | SynExpr.Ident id -> identText id
        | SynExpr.LongIdent(longDotId = lid) -> synLongText lid
        | SynExpr.App(funcExpr = f) -> headLabel f
        | SynExpr.TypeApp(expr = inner)
        | SynExpr.Paren(expr = inner) -> headLabel inner
        | _ -> "App"

    /// Render an expression as a node tree — deep enough to show the structure of the code
    /// (applications, `{ }` blocks, lists, tuples, literals). Every node carries its range.
    let rec private exprNode(e: SynExpr) : Node =
        let r = e.Range

        match e with
        | SynExpr.Paren(expr = inner)
        | SynExpr.Typed(expr = inner)
        | SynExpr.TypeApp(expr = inner) -> exprNode inner
        | SynExpr.Const(constant = c) -> leafR (constText c) r
        | SynExpr.Ident id -> leafR (identText id) r
        | SynExpr.LongIdent(longDotId = lid) -> leafR (synLongText lid) r
        | SynExpr.Tuple(exprs = exprs) -> nodeR "tuple" r (exprs |> List.map exprNode)
        | SynExpr.ArrayOrList(exprs = exprs) -> nodeR "[ ]" r (exprs |> List.map exprNode)
        | SynExpr.ArrayOrListComputed(expr = body) -> nodeR "[ ]" r (seqNodes body)
        | SynExpr.ComputationExpr(expr = body) -> nodeR "{ }" r (seqNodes body)
        | SynExpr.App _ ->
            // `a |> b |> c` flattens to one "|>" node; otherwise show the function head with
            // its (tuple-flattened) arguments.
            match pipeChain e with
            | Some stages -> nodeR "|>" r (stages |> List.map exprNode)
            | None ->
                let head, args = appSpine e
                nodeR ("App " + headLabel head) r (appArgNodes args)
        | SynExpr.Lambda(body = body) -> nodeR "lambda" r [ exprNode body ]
        | SynExpr.IfThenElse(ifExpr = c; thenExpr = t; elseExpr = el) ->
            nodeR "if" r ([ exprNode c; exprNode t ] @ (el |> Option.map exprNode |> Option.toList))
        | SynExpr.Match(expr = target; clauses = clauses) ->
            nodeR "match" r (exprNode target :: (clauses |> List.map clauseNode))
        | SynExpr.Sequential _ -> nodeR "seq" r (seqNodes e)
        | _ -> leafR "expr" r

    and private clauseNode(SynMatchClause(pat = pat; resultExpr = result; range = r)) =
        nodeR $"| {patText pat} ->" r [ exprNode result ]

    and private bindingNode(b: SynBinding) =
        let (SynBinding(headPat = pat; expr = body)) = b
        nodeR $"Let {patText pat}" b.RangeOfBindingWithRhs [ exprNode body ]

    and private appSpine(e: SynExpr) : SynExpr * SynExpr list =
        let rec go acc e =
            match e with
            | SynExpr.App(funcExpr = f; argExpr = a) -> go (a :: acc) f
            | _ -> e, acc

        go [] e

    and private appArgNodes(args: SynExpr list) : Node list =
        args
        |> List.filter(isUnit >> not)
        |> List.collect(fun a ->
            // Flatten a parenthesised tuple call `f(a, b)` into separate argument nodes.
            match a with
            | SynExpr.Paren(expr = SynExpr.Tuple(exprs = exprs)) -> exprs |> List.map exprNode
            | _ -> [ exprNode a ])

    and private seqNodes(e: SynExpr) : Node list =
        match e with
        | SynExpr.Sequential(expr1 = a; expr2 = b) -> seqNodes a @ seqNodes b
        | SynExpr.YieldOrReturn(expr = inner)
        | SynExpr.YieldOrReturnFrom(expr = inner) -> seqNodes inner
        | _ -> [ exprNode e ]

    and private pipeChain(e: SynExpr) : SynExpr list option =
        // FCS represents an infix operator as `LongIdent op_PipeRight` (not a bare Ident).
        let isPipe e =
            match e with
            | SynExpr.Ident id -> id.idText = "op_PipeRight"
            | SynExpr.LongIdent(longDotId = SynLongIdent(id = [ id ])) -> id.idText = "op_PipeRight"
            | _ -> false

        match e with
        | SynExpr.App(funcExpr = SynExpr.App(funcExpr = op; argExpr = lhs); argExpr = rhs) when isPipe op ->
            let left =
                match pipeChain lhs with
                | Some xs -> xs
                | None -> [ lhs ]

            Some(left @ [ rhs ])
        | _ -> None

    let private fieldNode(SynField(idOpt = idOpt; fieldType = ft; range = r)) =
        let name = idOpt |> Option.map identText |> Option.defaultValue "_"
        leafR $"Field {name} : {typeText ft}" r

    let private unionCaseNode(SynUnionCase(ident = SynIdent(id, _); caseType = caseType; range = r)) =
        let name = identText id

        match caseType with
        | SynUnionCaseKind.Fields(_ :: _ as fields) -> nodeR $"UnionCase {name}" r (fields |> List.map fieldNode)
        | _ -> leafR $"UnionCase {name}" r

    let private memberNode(m: SynMemberDefn) =
        match m with
        | SynMemberDefn.Member(memberDefn = b) -> bindingNode b
        | SynMemberDefn.LetBindings(bindings = bs) -> nodeR "let" m.Range (bs |> List.map bindingNode)
        | SynMemberDefn.ImplicitCtor _ -> leafR "new (primary constructor)" m.Range
        | SynMemberDefn.AutoProperty(ident = id) -> leafR $"member val {identText id}" m.Range
        | SynMemberDefn.Interface(interfaceType = t) -> leafR $"interface {typeText t}" m.Range
        | SynMemberDefn.AbstractSlot(slotSig = SynValSig(ident = SynIdent(id, _))) ->
            leafR $"abstract {identText id}" m.Range
        | _ -> leafR "member" m.Range

    let private typeDefnNode(td: SynTypeDefn) =
        let (SynTypeDefn(typeInfo = SynComponentInfo(longId = lid); typeRepr = repr; members = extra)) =
            td

        let name = longText lid
        let r = td.Range
        let members = extra |> List.map memberNode

        match repr with
        | SynTypeDefnRepr.Simple(simpleRepr = SynTypeDefnSimpleRepr.Record(recordFields = fields)) ->
            nodeR $"Record {name}" r ((fields |> List.map fieldNode) @ members)
        | SynTypeDefnRepr.Simple(simpleRepr = SynTypeDefnSimpleRepr.Union(unionCases = cases)) ->
            nodeR $"Union {name}" r ((cases |> List.map unionCaseNode) @ members)
        | SynTypeDefnRepr.Simple(simpleRepr = SynTypeDefnSimpleRepr.Enum(cases = cases)) ->
            nodeR
                $"Enum {name}"
                r
                (cases
                 |> List.map(fun (SynEnumCase(ident = SynIdent(id, _); range = cr)) -> leafR (identText id) cr))
        | SynTypeDefnRepr.Simple(simpleRepr = SynTypeDefnSimpleRepr.TypeAbbrev(rhsType = ty)) ->
            leafR $"Abbrev {name} = {typeText ty}" r
        | SynTypeDefnRepr.ObjectModel(members = ms) -> nodeR $"Type {name}" r ((ms |> List.map memberNode) @ members)
        | _ -> nodeR $"Type {name}" r members

    let rec private declNodes(decl: SynModuleDecl) : Node list =
        let r = decl.Range

        match decl with
        | SynModuleDecl.Open(target = SynOpenDeclTarget.ModuleOrNamespace(longId = lid)) ->
            [ leafR $"open {synLongText lid}" r ]
        | SynModuleDecl.Open _ -> [ leafR "open …" r ]
        | SynModuleDecl.Types(typeDefns = tds) -> tds |> List.map typeDefnNode
        | SynModuleDecl.Let(bindings = bs) -> bs |> List.map bindingNode
        | SynModuleDecl.Expr(expr = e) -> [ exprNode e ]
        | SynModuleDecl.NestedModule(moduleInfo = SynComponentInfo(longId = lid); decls = decls) ->
            [ nodeR $"module {longText lid}" r (decls |> List.collect declNodes) ]
        | _ -> [ leafR "declaration" r ]

    let private moduleNode(SynModuleOrNamespace(longId = lid; decls = decls; kind = kind; range = r)) =
        let label =
            match kind, lid with
            | SynModuleOrNamespaceKind.AnonModule, _ -> "module (anonymous)"
            | SynModuleOrNamespaceKind.DeclaredNamespace, _ -> $"namespace {longText lid}"
            | _, [] -> "module"
            | _, _ -> $"module {longText lid}"

        nodeR label r (decls |> List.collect declNodes)

    let private spanOf(r: range) =
        { StartLine = r.StartLine
          StartColumn = r.StartColumn
          EndLine = r.EndLine
          EndColumn = r.EndColumn }

    /// Render a node tree with ├─ / └─ connectors, collecting one span per line so the tree
    /// text and the span array line up index-for-index.
    let private render(root: Node) : Tree =
        let sb = StringBuilder()
        let spans = ResizeArray<Span option>()
        sb.Append(root.Label).Append('\n') |> ignore
        spans.Add(root.Range |> Option.map spanOf)

        let rec go (prefix: string) (n: Node) =
            let count = List.length n.Children

            n.Children
            |> List.iteri(fun i child ->
                let isLast = i = count - 1
                let branch = if isLast then "└─ " else "├─ "
                sb.Append(prefix).Append(branch).Append(child.Label).Append('\n') |> ignore
                spans.Add(child.Range |> Option.map spanOf)
                let childPrefix = prefix + (if isLast then "   " else "│  ")
                go childPrefix child)

        go "" root

        { Text = sb.ToString().TrimEnd('\n')
          Spans = spans.ToArray() }

    /// A tree that is just a message (no spans), for placeholder states.
    let message(text: string) : Tree =
        { Text = text
          Spans = [||] }

    /// Summarise a parsed implementation file as a syntax tree (text + per-line spans).
    let summarize(input: ParsedInput) : Tree =
        match input with
        | ParsedInput.ImplFile(ParsedImplFileInput(contents = modules)) ->
            match modules with
            | [ single ] -> render(moduleNode single)
            | _ ->
                render
                    { Label = "file"
                      Range = None
                      Children = modules |> List.map moduleNode }
        | ParsedInput.SigFile _ -> message "// (signature file)"
