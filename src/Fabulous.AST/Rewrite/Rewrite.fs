namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fantomas.Core.SyntaxOak

// Some Fantomas Oak DU names collide with widget modules in Fabulous.AST
// (TypeConstraint, ChainLink). Pattern matches still resolve correctly because
// the matched value carries its type, but construction sites need the
// fully-qualified case constructor — these aliases keep that readable.
type private FTypeConstraint = Fantomas.Core.SyntaxOak.TypeConstraint
type private FChainLink = Fantomas.Core.SyntaxOak.ChainLink

// Bottom-up rewriter over Fantomas Oak. Visits every Expr reachable from
// an Oak (including those inside type definitions, member bodies, attributes,
// patterns and types) and applies the caller's function after rebuilding each
// node's children. Reference equality is preserved when nothing changes, so
// unchanged subtrees aren't reallocated.
module private RewriteImpl =

    let inline refEq (a: 'a) (b: 'a) = System.Object.ReferenceEquals(a, b)

    let mapList (f: 'a -> 'a) (xs: 'a list) : 'a list =
        let mutable changed = false

        let result =
            xs
            |> List.map(fun x ->
                let x' = f x

                if not(refEq x x') then
                    changed <- true

                x')

        if changed then result else xs

    let mapOption (f: 'a -> 'a) (opt: 'a option) : 'a option =
        match opt with
        | None -> opt
        | Some v ->
            let v' = f v
            if refEq v v' then opt else Some v'

    // ===== DU walkers =====

    let rec rewriteExpr (f: Expr -> Expr) (e: Expr) : Expr =
        let inner =
            match e with
            | Expr.Lazy n ->
                let n' = rewriteExprLazyNode f n
                if refEq n n' then e else Expr.Lazy n'
            | Expr.Single n ->
                let n' = rewriteExprSingleNode f n
                if refEq n n' then e else Expr.Single n'
            | Expr.Constant _
            | Expr.Null _ -> e
            | Expr.Quote n ->
                let n' = rewriteExprQuoteNode f n
                if refEq n n' then e else Expr.Quote n'
            | Expr.Typed n ->
                let n' = rewriteExprTypedNode f n
                if refEq n n' then e else Expr.Typed n'
            | Expr.New n ->
                let n' = rewriteExprNewNode f n
                if refEq n n' then e else Expr.New n'
            | Expr.Tuple n ->
                let n' = rewriteExprTupleNode f n
                if refEq n n' then e else Expr.Tuple n'
            | Expr.StructTuple n ->
                let n' = rewriteExprStructTupleNode f n
                if refEq n n' then e else Expr.StructTuple n'
            | Expr.ArrayOrList n ->
                let n' = rewriteExprArrayOrListNode f n
                if refEq n n' then e else Expr.ArrayOrList n'
            | Expr.Record n ->
                let n' = rewriteExprRecordNode f n
                if refEq n n' then e else Expr.Record n'
            | Expr.InheritRecord n ->
                let n' = rewriteExprInheritRecordNode f n
                if refEq n n' then e else Expr.InheritRecord n'
            | Expr.AnonStructRecord n ->
                let n' = rewriteExprAnonStructRecordNode f n
                if refEq n n' then e else Expr.AnonStructRecord n'
            | Expr.ObjExpr n ->
                let n' = rewriteExprObjExprNode f n
                if refEq n n' then e else Expr.ObjExpr n'
            | Expr.While n ->
                let n' = rewriteExprWhileNode f n
                if refEq n n' then e else Expr.While n'
            | Expr.For n ->
                let n' = rewriteExprForNode f n
                if refEq n n' then e else Expr.For n'
            | Expr.ForEach n ->
                let n' = rewriteExprForEachNode f n
                if refEq n n' then e else Expr.ForEach n'
            | Expr.NamedComputation n ->
                let n' = rewriteExprNamedComputationNode f n
                if refEq n n' then e else Expr.NamedComputation n'
            | Expr.Computation n ->
                let n' = rewriteExprComputationNode f n
                if refEq n n' then e else Expr.Computation n'
            | Expr.CompExprBody n ->
                let n' = rewriteExprCompExprBodyNode f n
                if refEq n n' then e else Expr.CompExprBody n'
            | Expr.JoinIn n ->
                let n' = rewriteExprJoinInNode f n
                if refEq n n' then e else Expr.JoinIn n'
            | Expr.ParenLambda n ->
                let n' = rewriteExprParenLambdaNode f n
                if refEq n n' then e else Expr.ParenLambda n'
            | Expr.Lambda n ->
                let n' = rewriteExprLambdaNode f n
                if refEq n n' then e else Expr.Lambda n'
            | Expr.MatchLambda n ->
                let n' = rewriteExprMatchLambdaNode f n
                if refEq n n' then e else Expr.MatchLambda n'
            | Expr.Match n ->
                let n' = rewriteExprMatchNode f n
                if refEq n n' then e else Expr.Match n'
            | Expr.TraitCall n ->
                let n' = rewriteExprTraitCallNode f n
                if refEq n n' then e else Expr.TraitCall n'
            | Expr.ParenILEmbedded _
            | Expr.ParenFunctionNameWithStar _ -> e
            | Expr.Paren n ->
                let n' = rewriteExprParenNode f n
                if refEq n n' then e else Expr.Paren n'
            | Expr.Dynamic n ->
                let n' = rewriteExprDynamicNode f n
                if refEq n n' then e else Expr.Dynamic n'
            | Expr.PrefixApp n ->
                let n' = rewriteExprPrefixAppNode f n
                if refEq n n' then e else Expr.PrefixApp n'
            | Expr.SameInfixApps n ->
                let n' = rewriteExprSameInfixAppsNode f n
                if refEq n n' then e else Expr.SameInfixApps n'
            | Expr.InfixApp n ->
                let n' = rewriteExprInfixAppNode f n
                if refEq n n' then e else Expr.InfixApp n'
            | Expr.IndexWithoutDot n ->
                let n' = rewriteExprIndexWithoutDotNode f n
                if refEq n n' then e else Expr.IndexWithoutDot n'
            | Expr.AppLongIdentAndSingleParenArg n ->
                let n' = rewriteExprAppLongIdentAndSingleParenArgNode f n

                if refEq n n' then
                    e
                else
                    Expr.AppLongIdentAndSingleParenArg n'
            | Expr.AppSingleParenArg n ->
                let n' = rewriteExprAppSingleParenArgNode f n
                if refEq n n' then e else Expr.AppSingleParenArg n'
            | Expr.AppWithLambda n ->
                let n' = rewriteExprAppWithLambdaNode f n
                if refEq n n' then e else Expr.AppWithLambda n'
            | Expr.NestedIndexWithoutDot n ->
                let n' = rewriteExprNestedIndexWithoutDotNode f n
                if refEq n n' then e else Expr.NestedIndexWithoutDot n'
            | Expr.App n ->
                let n' = rewriteExprAppNode f n
                if refEq n n' then e else Expr.App n'
            | Expr.TypeApp n ->
                let n' = rewriteExprTypeAppNode f n
                if refEq n n' then e else Expr.TypeApp n'
            | Expr.TryWithSingleClause n ->
                let n' = rewriteExprTryWithSingleClauseNode f n
                if refEq n n' then e else Expr.TryWithSingleClause n'
            | Expr.TryWith n ->
                let n' = rewriteExprTryWithNode f n
                if refEq n n' then e else Expr.TryWith n'
            | Expr.TryFinally n ->
                let n' = rewriteExprTryFinallyNode f n
                if refEq n n' then e else Expr.TryFinally n'
            | Expr.IfThen n ->
                let n' = rewriteExprIfThenNode f n
                if refEq n n' then e else Expr.IfThen n'
            | Expr.IfThenElse n ->
                let n' = rewriteExprIfThenElseNode f n
                if refEq n n' then e else Expr.IfThenElse n'
            | Expr.IfThenElif n ->
                let n' = rewriteExprIfThenElifNode f n
                if refEq n n' then e else Expr.IfThenElif n'
            | Expr.Ident _
            | Expr.OptVar _ -> e
            | Expr.LongIdentSet n ->
                let n' = rewriteExprLongIdentSetNode f n
                if refEq n n' then e else Expr.LongIdentSet n'
            | Expr.DotIndexedGet n ->
                let n' = rewriteExprDotIndexedGetNode f n
                if refEq n n' then e else Expr.DotIndexedGet n'
            | Expr.DotIndexedSet n ->
                let n' = rewriteExprDotIndexedSetNode f n
                if refEq n n' then e else Expr.DotIndexedSet n'
            | Expr.NamedIndexedPropertySet n ->
                let n' = rewriteExprNamedIndexedPropertySetNode f n
                if refEq n n' then e else Expr.NamedIndexedPropertySet n'
            | Expr.DotNamedIndexedPropertySet n ->
                let n' = rewriteExprDotNamedIndexedPropertySetNode f n
                if refEq n n' then e else Expr.DotNamedIndexedPropertySet n'
            | Expr.Set n ->
                let n' = rewriteExprSetNode f n
                if refEq n n' then e else Expr.Set n'
            | Expr.LibraryOnlyStaticOptimization n ->
                let n' = rewriteExprLibraryOnlyStaticOptimizationNode f n

                if refEq n n' then
                    e
                else
                    Expr.LibraryOnlyStaticOptimization n'
            | Expr.InterpolatedStringExpr n ->
                let n' = rewriteExprInterpolatedStringExprNode f n
                if refEq n n' then e else Expr.InterpolatedStringExpr n'
            | Expr.IndexRangeWildcard _
            | Expr.TripleNumberIndexRange _ -> e
            | Expr.IndexRange n ->
                let n' = rewriteExprIndexRangeNode f n
                if refEq n n' then e else Expr.IndexRange n'
            | Expr.IndexFromEnd n ->
                let n' = rewriteExprIndexFromEndNode f n
                if refEq n n' then e else Expr.IndexFromEnd n'
            | Expr.Typar _ -> e
            | Expr.Chain n ->
                let n' = rewriteExprChain f n
                if refEq n n' then e else Expr.Chain n'
            | Expr.DotLambda n ->
                let n' = rewriteExprDotLambda f n
                if refEq n n' then e else Expr.DotLambda n'
            | Expr.BeginEnd n ->
                let n' = rewriteExprBeginEndNode f n
                if refEq n n' then e else Expr.BeginEnd n'
            | Expr.ExplicitConstructorThenExpr n ->
                let n' = rewriteExprExplicitConstructorThenExpr f n

                if refEq n n' then
                    e
                else
                    Expr.ExplicitConstructorThenExpr n'

        f inner

    and rewritePattern (f: Expr -> Expr) (p: Pattern) : Pattern =
        match p with
        | Pattern.OptionalVal _
        | Pattern.Null _
        | Pattern.Wild _
        | Pattern.Unit _
        | Pattern.Const _ -> p
        | Pattern.Or n ->
            let n' = rewritePatLeftMiddleRight f n
            if refEq n n' then p else Pattern.Or n'
        | Pattern.Ands n ->
            let n' = rewritePatAndsNode f n
            if refEq n n' then p else Pattern.Ands n'
        | Pattern.Parameter n ->
            let n' = rewritePatParameterNode f n
            if refEq n n' then p else Pattern.Parameter n'
        | Pattern.NamedParenStarIdent _
        | Pattern.Named _ -> p
        | Pattern.As n ->
            let n' = rewritePatLeftMiddleRight f n
            if refEq n n' then p else Pattern.As n'
        | Pattern.ListCons n ->
            let n' = rewritePatLeftMiddleRight f n
            if refEq n n' then p else Pattern.ListCons n'
        | Pattern.NamePatPairs n ->
            let n' = rewritePatNamePatPairsNode f n
            if refEq n n' then p else Pattern.NamePatPairs n'
        | Pattern.LongIdent n ->
            let n' = rewritePatLongIdentNode f n
            if refEq n n' then p else Pattern.LongIdent n'
        | Pattern.Paren n ->
            let n' = rewritePatParenNode f n
            if refEq n n' then p else Pattern.Paren n'
        | Pattern.Tuple n ->
            let n' = rewritePatTupleNode f n
            if refEq n n' then p else Pattern.Tuple n'
        | Pattern.StructTuple n ->
            let n' = rewritePatStructTupleNode f n
            if refEq n n' then p else Pattern.StructTuple n'
        | Pattern.ArrayOrList n ->
            let n' = rewritePatArrayOrListNode f n
            if refEq n n' then p else Pattern.ArrayOrList n'
        | Pattern.Record n ->
            let n' = rewritePatRecordNode f n
            if refEq n n' then p else Pattern.Record n'
        | Pattern.IsInst n ->
            let n' = rewritePatIsInstNode f n
            if refEq n n' then p else Pattern.IsInst n'
        | Pattern.QuoteExpr n ->
            let n' = rewriteExprQuoteNode f n
            if refEq n n' then p else Pattern.QuoteExpr n'

    and rewriteType (f: Expr -> Expr) (t: Type) : Type =
        match t with
        | Type.Funs n ->
            let n' = rewriteTypeFunsNode f n
            if refEq n n' then t else Type.Funs n'
        | Type.Tuple n ->
            let n' = rewriteTypeTupleNode f n
            if refEq n n' then t else Type.Tuple n'
        | Type.HashConstraint n ->
            let n' = rewriteTypeHashConstraintNode f n
            if refEq n n' then t else Type.HashConstraint n'
        | Type.MeasurePower n ->
            let n' = rewriteTypeMeasurePowerNode f n
            if refEq n n' then t else Type.MeasurePower n'
        | Type.StaticConstant _ -> t
        | Type.StaticConstantExpr n ->
            let n' = rewriteTypeStaticConstantExprNode f n
            if refEq n n' then t else Type.StaticConstantExpr n'
        | Type.StaticConstantNamed n ->
            let n' = rewriteTypeStaticConstantNamedNode f n
            if refEq n n' then t else Type.StaticConstantNamed n'
        | Type.Array n ->
            let n' = rewriteTypeArrayNode f n
            if refEq n n' then t else Type.Array n'
        | Type.Anon _
        | Type.Var _ -> t
        | Type.AppPostfix n ->
            let n' = rewriteTypeAppPostFixNode f n
            if refEq n n' then t else Type.AppPostfix n'
        | Type.AppPrefix n ->
            let n' = rewriteTypeAppPrefixNode f n
            if refEq n n' then t else Type.AppPrefix n'
        | Type.StructTuple n ->
            let n' = rewriteTypeStructTupleNode f n
            if refEq n n' then t else Type.StructTuple n'
        | Type.WithSubTypeConstraint c ->
            let c' = rewriteTypeConstraint f c
            if refEq c c' then t else Type.WithSubTypeConstraint c'
        | Type.WithGlobalConstraints n ->
            let n' = rewriteTypeWithGlobalConstraintsNode f n
            if refEq n n' then t else Type.WithGlobalConstraints n'
        | Type.LongIdent _ -> t
        | Type.AnonRecord n ->
            let n' = rewriteTypeAnonRecordNode f n
            if refEq n n' then t else Type.AnonRecord n'
        | Type.Paren n ->
            let n' = rewriteTypeParenNode f n
            if refEq n n' then t else Type.Paren n'
        | Type.SignatureParameter n ->
            let n' = rewriteTypeSignatureParameterNode f n
            if refEq n n' then t else Type.SignatureParameter n'
        | Type.Or n ->
            let n' = rewriteTypeOrNode f n
            if refEq n n' then t else Type.Or n'
        | Type.LongIdentApp n ->
            let n' = rewriteTypeLongIdentAppNode f n
            if refEq n n' then t else Type.LongIdentApp n'
        | Type.Intersection n ->
            let n' = rewriteTypeIntersectionNode f n
            if refEq n n' then t else Type.Intersection n'

    and rewriteMemberDefn (f: Expr -> Expr) (m: MemberDefn) : MemberDefn =
        match m with
        | MemberDefn.ImplicitInherit ic ->
            let ic' = rewriteInheritConstructor f ic
            if refEq ic ic' then m else MemberDefn.ImplicitInherit ic'
        | MemberDefn.Inherit n ->
            let n' = rewriteMemberDefnInheritNode f n
            if refEq n n' then m else MemberDefn.Inherit n'
        | MemberDefn.ValField n ->
            let n' = rewriteFieldNode f n
            if refEq n n' then m else MemberDefn.ValField n'
        | MemberDefn.Member n ->
            let n' = rewriteBindingNode f n
            if refEq n n' then m else MemberDefn.Member n'
        | MemberDefn.ExternBinding n ->
            let n' = rewriteExternBindingNode f n
            if refEq n n' then m else MemberDefn.ExternBinding n'
        | MemberDefn.DoExpr n ->
            let n' = rewriteExprSingleNode f n
            if refEq n n' then m else MemberDefn.DoExpr n'
        | MemberDefn.LetBinding n ->
            let n' = rewriteBindingListNode f n
            if refEq n n' then m else MemberDefn.LetBinding n'
        | MemberDefn.ExplicitCtor n ->
            let n' = rewriteMemberDefnExplicitCtorNode f n
            if refEq n n' then m else MemberDefn.ExplicitCtor n'
        | MemberDefn.Interface n ->
            let n' = rewriteMemberDefnInterfaceNode f n
            if refEq n n' then m else MemberDefn.Interface n'
        | MemberDefn.AutoProperty n ->
            let n' = rewriteMemberDefnAutoPropertyNode f n
            if refEq n n' then m else MemberDefn.AutoProperty n'
        | MemberDefn.AbstractSlot n ->
            let n' = rewriteMemberDefnAbstractSlotNode f n
            if refEq n n' then m else MemberDefn.AbstractSlot n'
        | MemberDefn.PropertyGetSet n ->
            let n' = rewriteMemberDefnPropertyGetSetNode f n
            if refEq n n' then m else MemberDefn.PropertyGetSet n'
        | MemberDefn.SigMember n ->
            let n' = rewriteMemberDefnSigMemberNode f n
            if refEq n n' then m else MemberDefn.SigMember n'

    and rewriteModuleDecl (f: Expr -> Expr) (d: ModuleDecl) : ModuleDecl =
        match d with
        | ModuleDecl.OpenList _
        | ModuleDecl.HashDirectiveList _ -> d
        | ModuleDecl.Attributes n ->
            let n' = rewriteModuleDeclAttributesNode f n
            if refEq n n' then d else ModuleDecl.Attributes n'
        | ModuleDecl.DeclExpr e ->
            let e' = rewriteExpr f e
            if refEq e e' then d else ModuleDecl.DeclExpr e'
        | ModuleDecl.Exception n ->
            let n' = rewriteExceptionDefnNode f n
            if refEq n n' then d else ModuleDecl.Exception n'
        | ModuleDecl.ExternBinding n ->
            let n' = rewriteExternBindingNode f n
            if refEq n n' then d else ModuleDecl.ExternBinding n'
        | ModuleDecl.TopLevelBinding n ->
            let n' = rewriteBindingNode f n
            if refEq n n' then d else ModuleDecl.TopLevelBinding n'
        | ModuleDecl.ModuleAbbrev _ -> d
        | ModuleDecl.NestedModule n ->
            let n' = rewriteNestedModuleNode f n
            if refEq n n' then d else ModuleDecl.NestedModule n'
        | ModuleDecl.TypeDefn td ->
            let td' = rewriteTypeDefn f td
            if refEq td td' then d else ModuleDecl.TypeDefn td'
        | ModuleDecl.Val n ->
            let n' = rewriteValNode f n
            if refEq n n' then d else ModuleDecl.Val n'

    and rewriteTypeDefn (f: Expr -> Expr) (td: TypeDefn) : TypeDefn =
        match td with
        | TypeDefn.Enum n ->
            let n' = rewriteTypeDefnEnumNode f n
            if refEq n n' then td else TypeDefn.Enum n'
        | TypeDefn.Union n ->
            let n' = rewriteTypeDefnUnionNode f n
            if refEq n n' then td else TypeDefn.Union n'
        | TypeDefn.Record n ->
            let n' = rewriteTypeDefnRecordNode f n
            if refEq n n' then td else TypeDefn.Record n'
        | TypeDefn.None n ->
            let n' = rewriteTypeNameNode f n
            if refEq n n' then td else TypeDefn.None n'
        | TypeDefn.Abbrev n ->
            let n' = rewriteTypeDefnAbbrevNode f n
            if refEq n n' then td else TypeDefn.Abbrev n'
        | TypeDefn.Explicit n ->
            let n' = rewriteTypeDefnExplicitNode f n
            if refEq n n' then td else TypeDefn.Explicit n'
        | TypeDefn.Augmentation n ->
            let n' = rewriteTypeDefnAugmentationNode f n
            if refEq n n' then td else TypeDefn.Augmentation n'
        | TypeDefn.Delegate n ->
            let n' = rewriteTypeDefnDelegateNode f n
            if refEq n n' then td else TypeDefn.Delegate n'
        | TypeDefn.Regular n ->
            let n' = rewriteTypeDefnRegularNode f n
            if refEq n n' then td else TypeDefn.Regular n'

    and rewriteTypeConstraint (f: Expr -> Expr) (c: TypeConstraint) : TypeConstraint =
        match c with
        | TypeConstraint.Single _ -> c
        | TypeConstraint.DefaultsToType n ->
            let n' = rewriteTypeConstraintDefaultsToTypeNode f n
            if refEq n n' then c else FTypeConstraint.DefaultsToType n'
        | TypeConstraint.SubtypeOfType n ->
            let n' = rewriteTypeConstraintSubtypeOfTypeNode f n
            if refEq n n' then c else FTypeConstraint.SubtypeOfType n'
        | TypeConstraint.SupportsMember n ->
            let n' = rewriteTypeConstraintSupportsMemberNode f n
            if refEq n n' then c else FTypeConstraint.SupportsMember n'
        | TypeConstraint.EnumOrDelegate n ->
            let n' = rewriteTypeConstraintEnumOrDelegateNode f n
            if refEq n n' then c else FTypeConstraint.EnumOrDelegate n'
        | TypeConstraint.WhereSelfConstrained t ->
            let t' = rewriteType f t

            if refEq t t' then
                c
            else
                FTypeConstraint.WhereSelfConstrained t'
        | TypeConstraint.WhereNotSupportsNull _ -> c

    and rewriteTyparDecls (f: Expr -> Expr) (td: TyparDecls) : TyparDecls =
        match td with
        | TyparDecls.PostfixList n ->
            let n' = rewriteTyparDeclsPostfixListNode f n
            if refEq n n' then td else TyparDecls.PostfixList n'
        | TyparDecls.PrefixList n ->
            let n' = rewriteTyparDeclsPrefixListNode f n
            if refEq n n' then td else TyparDecls.PrefixList n'
        | TyparDecls.SinglePrefix n ->
            let n' = rewriteTyparDeclNode f n
            if refEq n n' then td else TyparDecls.SinglePrefix n'

    and rewriteInheritConstructor (f: Expr -> Expr) (ic: InheritConstructor) : InheritConstructor =
        match ic with
        | InheritConstructor.TypeOnly n ->
            let n' = rewriteInheritConstructorTypeOnlyNode f n
            if refEq n n' then ic else InheritConstructor.TypeOnly n'
        | InheritConstructor.Unit n ->
            let n' = rewriteInheritConstructorUnitNode f n
            if refEq n n' then ic else InheritConstructor.Unit n'
        | InheritConstructor.Paren n ->
            let n' = rewriteInheritConstructorParenNode f n
            if refEq n n' then ic else InheritConstructor.Paren n'
        | InheritConstructor.Other n ->
            let n' = rewriteInheritConstructorOtherNode f n
            if refEq n n' then ic else InheritConstructor.Other n'

    and rewriteChainLink (f: Expr -> Expr) (link: ChainLink) : ChainLink =
        match link with
        | ChainLink.Identifier e ->
            let e' = rewriteExpr f e
            if refEq e e' then link else FChainLink.Identifier e'
        | ChainLink.Dot _ -> link
        | ChainLink.Expr e ->
            let e' = rewriteExpr f e
            if refEq e e' then link else FChainLink.Expr e'
        | ChainLink.AppParen n ->
            let n' = rewriteLinkSingleAppParen f n
            if refEq n n' then link else FChainLink.AppParen n'
        | ChainLink.AppUnit n ->
            let n' = rewriteLinkSingleAppUnit f n
            if refEq n n' then link else FChainLink.AppUnit n'
        | ChainLink.IndexExpr e ->
            let e' = rewriteExpr f e
            if refEq e e' then link else FChainLink.IndexExpr e'

    and rewriteComputationExpressionStatement
        (f: Expr -> Expr)
        (s: ComputationExpressionStatement)
        : ComputationExpressionStatement =
        match s with
        | ComputationExpressionStatement.LetOrUseStatement n ->
            let n' = rewriteExprLetOrUseNode f n

            if refEq n n' then
                s
            else
                ComputationExpressionStatement.LetOrUseStatement n'
        | ComputationExpressionStatement.LetOrUseBangStatement n ->
            let n' = rewriteExprLetOrUseBangNode f n

            if refEq n n' then
                s
            else
                ComputationExpressionStatement.LetOrUseBangStatement n'
        | ComputationExpressionStatement.AndBangStatement n ->
            let n' = rewriteExprAndBang f n

            if refEq n n' then
                s
            else
                ComputationExpressionStatement.AndBangStatement n'
        | ComputationExpressionStatement.OtherStatement e ->
            let e' = rewriteExpr f e

            if refEq e e' then
                s
            else
                ComputationExpressionStatement.OtherStatement e'

    // ===== Class walkers =====

    and rewriteOak (f: Expr -> Expr) (oak: Oak) : Oak =
        let mods' = oak.ModulesOrNamespaces |> mapList(rewriteModuleOrNamespaceNode f)

        if refEq mods' oak.ModulesOrNamespaces then
            oak
        else
            Oak(oak.ParsedHashDirectives, mods', oak.Range)

    and rewriteModuleOrNamespaceNode (f: Expr -> Expr) (n: ModuleOrNamespaceNode) : ModuleOrNamespaceNode =
        let decls' = n.Declarations |> mapList(rewriteModuleDecl f)

        if refEq decls' n.Declarations then
            n
        else
            ModuleOrNamespaceNode(n.Header, decls', n.Range)

    and rewriteNestedModuleNode (f: Expr -> Expr) (n: NestedModuleNode) : NestedModuleNode =
        let decls' = n.Declarations |> mapList(rewriteModuleDecl f)
        let attrs' = n.Attributes |> mapOption(rewriteMultipleAttributeListNode f)

        if refEq decls' n.Declarations && refEq attrs' n.Attributes then
            n
        else
            NestedModuleNode(
                n.XmlDoc,
                attrs',
                n.Module,
                n.Accessibility,
                n.IsRecursive,
                n.Identifier,
                n.Equals,
                decls',
                n.Range
            )

    and rewriteModuleDeclAttributesNode (f: Expr -> Expr) (n: ModuleDeclAttributesNode) : ModuleDeclAttributesNode =
        let expr' = rewriteExpr f n.Expr
        let attrs' = n.Attributes |> mapOption(rewriteMultipleAttributeListNode f)

        if refEq expr' n.Expr && refEq attrs' n.Attributes then
            n
        else
            ModuleDeclAttributesNode(attrs', expr', n.Range)

    and rewriteExceptionDefnNode (f: Expr -> Expr) (n: ExceptionDefnNode) : ExceptionDefnNode =
        let attrs' = n.Attributes |> mapOption(rewriteMultipleAttributeListNode f)
        let uc' = rewriteUnionCaseNode f n.UnionCase
        let members' = n.Members |> mapList(rewriteMemberDefn f)

        if refEq attrs' n.Attributes && refEq uc' n.UnionCase && refEq members' n.Members then
            n
        else
            ExceptionDefnNode(n.XmlDoc, attrs', n.Accessibility, uc', n.WithKeyword, members', n.Range)

    and rewriteBindingNode (f: Expr -> Expr) (n: BindingNode) : BindingNode =
        let attrs' = n.Attributes |> mapOption(rewriteMultipleAttributeListNode f)

        let fnName' =
            match n.FunctionName with
            | Choice1Of2 _ -> n.FunctionName
            | Choice2Of2 p ->
                let p' = rewritePattern f p
                if refEq p p' then n.FunctionName else Choice2Of2 p'

        let typars' = n.GenericTypeParameters |> mapOption(rewriteTyparDecls f)
        let parameters' = n.Parameters |> mapList(rewritePattern f)
        let returnType' = n.ReturnType |> mapOption(rewriteBindingReturnInfoNode f)
        let expr' = rewriteExpr f n.Expr

        if
            refEq attrs' n.Attributes
            && refEq fnName' n.FunctionName
            && refEq typars' n.GenericTypeParameters
            && refEq parameters' n.Parameters
            && refEq returnType' n.ReturnType
            && refEq expr' n.Expr
        then
            n
        else
            BindingNode(
                n.XmlDoc,
                attrs',
                n.LeadingKeyword,
                n.IsMutable,
                n.Inline,
                n.Accessibility,
                fnName',
                typars',
                parameters',
                returnType',
                n.Equals,
                expr',
                n.Range
            )

    and rewriteBindingListNode (f: Expr -> Expr) (n: BindingListNode) : BindingListNode =
        let bindings' = n.Bindings |> mapList(rewriteBindingNode f)

        if refEq bindings' n.Bindings then
            n
        else
            BindingListNode(bindings', n.Range)

    and rewriteBindingReturnInfoNode (f: Expr -> Expr) (n: BindingReturnInfoNode) : BindingReturnInfoNode =
        let t' = rewriteType f n.Type

        if refEq t' n.Type then
            n
        else
            BindingReturnInfoNode(n.Colon, t', n.Range)

    and rewriteValNode (f: Expr -> Expr) (n: ValNode) : ValNode =
        let attrs' = n.Attributes |> mapOption(rewriteMultipleAttributeListNode f)
        let typars' = n.TypeParams |> mapOption(rewriteTyparDecls f)
        let t' = rewriteType f n.Type
        let eo' = n.Expr |> mapOption(rewriteExpr f)

        if
            refEq attrs' n.Attributes
            && refEq typars' n.TypeParams
            && refEq t' n.Type
            && refEq eo' n.Expr
        then
            n
        else
            ValNode(
                n.XmlDoc,
                attrs',
                n.LeadingKeyword,
                n.Inline,
                n.IsMutable,
                n.Accessibility,
                n.Identifier,
                typars',
                t',
                n.Equals,
                eo',
                n.Range
            )

    and rewriteExternBindingNode (f: Expr -> Expr) (n: ExternBindingNode) : ExternBindingNode =
        let attrs' = n.Attributes |> mapOption(rewriteMultipleAttributeListNode f)

        let attrsOfType' =
            n.AttributesOfType |> mapOption(rewriteMultipleAttributeListNode f)

        let t' = rewriteType f n.Type
        let parameters' = n.Parameters |> mapList(rewriteExternBindingPatternNode f)

        if
            refEq attrs' n.Attributes
            && refEq attrsOfType' n.AttributesOfType
            && refEq t' n.Type
            && refEq parameters' n.Parameters
        then
            n
        else
            ExternBindingNode(
                n.XmlDoc,
                attrs',
                n.Extern,
                attrsOfType',
                t',
                n.Accessibility,
                n.Identifier,
                n.OpeningParen,
                parameters',
                n.ClosingParen,
                n.Range
            )

    and rewriteExternBindingPatternNode (f: Expr -> Expr) (n: ExternBindingPatternNode) : ExternBindingPatternNode =
        let attrs' = n.Attributes |> mapOption(rewriteMultipleAttributeListNode f)
        let t' = n.Type |> mapOption(rewriteType f)
        let pat' = n.Pattern |> mapOption(rewritePattern f)

        if refEq attrs' n.Attributes && refEq t' n.Type && refEq pat' n.Pattern then
            n
        else
            ExternBindingPatternNode(attrs', t', pat', n.Range)

    and rewriteFieldNode (f: Expr -> Expr) (n: FieldNode) : FieldNode =
        let attrs' = n.Attributes |> mapOption(rewriteMultipleAttributeListNode f)
        let t' = rewriteType f n.Type

        if refEq attrs' n.Attributes && refEq t' n.Type then
            n
        else
            FieldNode(n.XmlDoc, attrs', n.LeadingKeyword, n.MutableKeyword, n.Accessibility, n.Name, t', n.Range)

    and rewriteUnionCaseNode (f: Expr -> Expr) (n: UnionCaseNode) : UnionCaseNode =
        let attrs' = n.Attributes |> mapOption(rewriteMultipleAttributeListNode f)
        let fields' = n.Fields |> mapList(rewriteFieldNode f)

        if refEq attrs' n.Attributes && refEq fields' n.Fields then
            n
        else
            UnionCaseNode(n.XmlDoc, attrs', n.Bar, n.Identifier, fields', n.Range)

    and rewriteEnumCaseNode (f: Expr -> Expr) (n: EnumCaseNode) : EnumCaseNode =
        let attrs' = n.Attributes |> mapOption(rewriteMultipleAttributeListNode f)
        let constant' = rewriteExpr f n.Constant

        if refEq attrs' n.Attributes && refEq constant' n.Constant then
            n
        else
            EnumCaseNode(n.XmlDoc, n.Bar, attrs', n.Identifier, n.Equals, constant', n.Range)

    and rewriteAttributeNode (f: Expr -> Expr) (n: AttributeNode) : AttributeNode =
        let expr' = n.Expr |> mapOption(rewriteExpr f)

        if refEq expr' n.Expr then
            n
        else
            AttributeNode(n.TypeName, expr', n.Target, n.Range)

    and rewriteAttributeListNode (f: Expr -> Expr) (n: AttributeListNode) : AttributeListNode =
        let attrs' = n.Attributes |> mapList(rewriteAttributeNode f)

        if refEq attrs' n.Attributes then
            n
        else
            AttributeListNode(n.Opening, attrs', n.Closing, n.Range)

    and rewriteMultipleAttributeListNode (f: Expr -> Expr) (n: MultipleAttributeListNode) : MultipleAttributeListNode =
        let lists' = n.AttributeLists |> mapList(rewriteAttributeListNode f)

        if refEq lists' n.AttributeLists then
            n
        else
            MultipleAttributeListNode(lists', n.Range)

    and rewriteImplicitConstructorNode (f: Expr -> Expr) (n: ImplicitConstructorNode) : ImplicitConstructorNode =
        let attrs' = n.Attributes |> mapOption(rewriteMultipleAttributeListNode f)
        let pat' = rewritePattern f n.Pattern

        if refEq attrs' n.Attributes && refEq pat' n.Pattern then
            n
        else
            ImplicitConstructorNode(n.XmlDoc, attrs', n.Accessibility, pat', n.Self, n.Range)

    and rewriteTypeNameNode (f: Expr -> Expr) (n: TypeNameNode) : TypeNameNode =
        let attrs' = n.Attributes |> mapOption(rewriteMultipleAttributeListNode f)
        let typars' = n.TypeParameters |> mapOption(rewriteTyparDecls f)
        let constraints' = n.Constraints |> mapList(rewriteTypeConstraint f)
        let ic' = n.ImplicitConstructor |> mapOption(rewriteImplicitConstructorNode f)

        if
            refEq attrs' n.Attributes
            && refEq typars' n.TypeParameters
            && refEq constraints' n.Constraints
            && refEq ic' n.ImplicitConstructor
        then
            n
        else
            TypeNameNode(
                n.XmlDoc,
                attrs',
                n.LeadingKeyword,
                n.Accessibility,
                n.Identifier,
                typars',
                constraints',
                ic',
                n.EqualsToken,
                n.WithKeyword,
                n.Range
            )

    and rewriteInterfaceImplNode (f: Expr -> Expr) (n: InterfaceImplNode) : InterfaceImplNode =
        let t' = rewriteType f n.Type
        let bindings' = n.Bindings |> mapList(rewriteBindingNode f)
        let members' = n.Members |> mapList(rewriteMemberDefn f)

        if refEq t' n.Type && refEq bindings' n.Bindings && refEq members' n.Members then
            n
        else
            InterfaceImplNode(n.Interface, t', n.With, bindings', members', n.Range)

    // ===== Type nodes =====

    and rewriteTypeFunsNode (f: Expr -> Expr) (n: TypeFunsNode) : TypeFunsNode =
        let mutable changed = false

        let parameters' =
            n.Parameters
            |> List.map(fun (t, s) ->
                let t' = rewriteType f t

                if not(refEq t t') then
                    changed <- true

                (t', s))

        let returnType' = rewriteType f n.ReturnType

        if not changed && refEq returnType' n.ReturnType then
            n
        else
            TypeFunsNode(parameters', returnType', n.Range)

    and rewriteTypeTupleNode (f: Expr -> Expr) (n: TypeTupleNode) : TypeTupleNode =
        let mutable changed = false

        let path' =
            n.Path
            |> List.map(fun choice ->
                match choice with
                | Choice1Of2 t ->
                    let t' = rewriteType f t

                    if not(refEq t t') then
                        changed <- true

                    Choice1Of2 t'
                | other -> other)

        if not changed then n else TypeTupleNode(path', n.Range)

    and rewriteTypeStructTupleNode (f: Expr -> Expr) (n: TypeStructTupleNode) : TypeStructTupleNode =
        let mutable changed = false

        let path' =
            n.Path
            |> List.map(fun choice ->
                match choice with
                | Choice1Of2 t ->
                    let t' = rewriteType f t

                    if not(refEq t t') then
                        changed <- true

                    Choice1Of2 t'
                | other -> other)

        if not changed then
            n
        else
            TypeStructTupleNode(n.Keyword, path', n.ClosingParen, n.Range)

    and rewriteTypeHashConstraintNode (f: Expr -> Expr) (n: TypeHashConstraintNode) : TypeHashConstraintNode =
        let t' = rewriteType f n.Type

        if refEq t' n.Type then
            n
        else
            TypeHashConstraintNode(n.Hash, t', n.Range)

    and rewriteTypeMeasurePowerNode (f: Expr -> Expr) (n: TypeMeasurePowerNode) : TypeMeasurePowerNode =
        let t' = rewriteType f n.BaseMeasure

        if refEq t' n.BaseMeasure then
            n
        else
            TypeMeasurePowerNode(t', n.Exponent, n.Range)

    and rewriteTypeStaticConstantExprNode
        (f: Expr -> Expr)
        (n: TypeStaticConstantExprNode)
        : TypeStaticConstantExprNode =
        let e' = rewriteExpr f n.Expr

        if refEq e' n.Expr then
            n
        else
            TypeStaticConstantExprNode(n.Const, e', n.Range)

    and rewriteTypeStaticConstantNamedNode
        (f: Expr -> Expr)
        (n: TypeStaticConstantNamedNode)
        : TypeStaticConstantNamedNode =
        let id' = rewriteType f n.Identifier
        let v' = rewriteType f n.Value

        if refEq id' n.Identifier && refEq v' n.Value then
            n
        else
            TypeStaticConstantNamedNode(id', v', n.Range)

    and rewriteTypeArrayNode (f: Expr -> Expr) (n: TypeArrayNode) : TypeArrayNode =
        let t' = rewriteType f n.Type

        if refEq t' n.Type then
            n
        else
            TypeArrayNode(t', n.Rank, n.Range)

    and rewriteTypeAppPostFixNode (f: Expr -> Expr) (n: TypeAppPostFixNode) : TypeAppPostFixNode =
        let first' = rewriteType f n.First
        let last' = rewriteType f n.Last

        if refEq first' n.First && refEq last' n.Last then
            n
        else
            TypeAppPostFixNode(first', last', n.Range)

    and rewriteTypeAppPrefixNode (f: Expr -> Expr) (n: TypeAppPrefixNode) : TypeAppPrefixNode =
        let id' = rewriteType f n.Identifier
        let args' = n.Arguments |> mapList(rewriteType f)

        if refEq id' n.Identifier && refEq args' n.Arguments then
            n
        else
            TypeAppPrefixNode(id', n.PostIdentifier, n.LessThen, args', n.GreaterThan, n.Range)

    and rewriteTypeAnonRecordNode (f: Expr -> Expr) (n: TypeAnonRecordNode) : TypeAnonRecordNode =
        let mutable changed = false

        let fields' =
            n.Fields
            |> List.map(fun (name, t) ->
                let t' = rewriteType f t

                if not(refEq t t') then
                    changed <- true

                (name, t'))

        if not changed then
            n
        else
            TypeAnonRecordNode(n.Struct, n.Opening, fields', n.Closing, n.Range)

    and rewriteTypeParenNode (f: Expr -> Expr) (n: TypeParenNode) : TypeParenNode =
        let t' = rewriteType f n.Type

        if refEq t' n.Type then
            n
        else
            TypeParenNode(n.OpeningParen, t', n.ClosingParen, n.Range)

    and rewriteTypeSignatureParameterNode
        (f: Expr -> Expr)
        (n: TypeSignatureParameterNode)
        : TypeSignatureParameterNode =
        let attrs' = n.Attributes |> mapOption(rewriteMultipleAttributeListNode f)
        let t' = rewriteType f n.Type

        if refEq attrs' n.Attributes && refEq t' n.Type then
            n
        else
            TypeSignatureParameterNode(attrs', n.Identifier, t', n.Range)

    and rewriteTypeOrNode (f: Expr -> Expr) (n: TypeOrNode) : TypeOrNode =
        let lhs' = rewriteType f n.LeftHandSide
        let rhs' = rewriteType f n.RightHandSide

        if refEq lhs' n.LeftHandSide && refEq rhs' n.RightHandSide then
            n
        else
            TypeOrNode(lhs', n.Or, rhs', n.Range)

    and rewriteTypeLongIdentAppNode (f: Expr -> Expr) (n: TypeLongIdentAppNode) : TypeLongIdentAppNode =
        let t' = rewriteType f n.AppType

        if refEq t' n.AppType then
            n
        else
            TypeLongIdentAppNode(t', n.LongIdent, n.Range)

    and rewriteTypeIntersectionNode (f: Expr -> Expr) (n: TypeIntersectionNode) : TypeIntersectionNode =
        let mutable changed = false

        let items' =
            n.TypesAndSeparators
            |> List.map(fun choice ->
                match choice with
                | Choice1Of2 t ->
                    let t' = rewriteType f t

                    if not(refEq t t') then
                        changed <- true

                    Choice1Of2 t'
                | other -> other)

        if not changed then
            n
        else
            TypeIntersectionNode(items', n.Range)

    and rewriteTypeWithGlobalConstraintsNode
        (f: Expr -> Expr)
        (n: TypeWithGlobalConstraintsNode)
        : TypeWithGlobalConstraintsNode =
        let t' = rewriteType f n.Type
        let constraints' = n.TypeConstraints |> mapList(rewriteTypeConstraint f)

        if refEq t' n.Type && refEq constraints' n.TypeConstraints then
            n
        else
            TypeWithGlobalConstraintsNode(t', constraints', n.Range)

    // ===== TypeConstraint nodes =====

    and rewriteTypeConstraintDefaultsToTypeNode
        (f: Expr -> Expr)
        (n: TypeConstraintDefaultsToTypeNode)
        : TypeConstraintDefaultsToTypeNode =
        let t' = rewriteType f n.Type

        if refEq t' n.Type then
            n
        else
            TypeConstraintDefaultsToTypeNode(n.Default, n.Typar, t', n.Range)

    and rewriteTypeConstraintSubtypeOfTypeNode
        (f: Expr -> Expr)
        (n: TypeConstraintSubtypeOfTypeNode)
        : TypeConstraintSubtypeOfTypeNode =
        let t' = rewriteType f n.Type

        if refEq t' n.Type then
            n
        else
            TypeConstraintSubtypeOfTypeNode(n.Typar, t', n.Range)

    and rewriteTypeConstraintSupportsMemberNode
        (f: Expr -> Expr)
        (n: TypeConstraintSupportsMemberNode)
        : TypeConstraintSupportsMemberNode =
        let t' = rewriteType f n.Type
        let m' = rewriteMemberDefn f n.MemberSig

        if refEq t' n.Type && refEq m' n.MemberSig then
            n
        else
            TypeConstraintSupportsMemberNode(t', m', n.Range)

    and rewriteTypeConstraintEnumOrDelegateNode
        (f: Expr -> Expr)
        (n: TypeConstraintEnumOrDelegateNode)
        : TypeConstraintEnumOrDelegateNode =
        let ts' = n.Types |> mapList(rewriteType f)

        if refEq ts' n.Types then
            n
        else
            TypeConstraintEnumOrDelegateNode(n.Typar, n.Verb, ts', n.Range)

    // ===== TyparDecls nodes =====

    and rewriteTyparDeclNode (f: Expr -> Expr) (n: TyparDeclNode) : TyparDeclNode =
        let attrs' = n.Attributes |> mapOption(rewriteMultipleAttributeListNode f)
        let mutable changed = false

        let constraints' =
            n.IntersectionConstraints
            |> List.map(fun choice ->
                match choice with
                | Choice1Of2 t ->
                    let t' = rewriteType f t

                    if not(refEq t t') then
                        changed <- true

                    Choice1Of2 t'
                | other -> other)

        if refEq attrs' n.Attributes && not changed then
            n
        else
            TyparDeclNode(attrs', n.TypeParameter, constraints', n.Range)

    and rewriteTyparDeclsPostfixListNode (f: Expr -> Expr) (n: TyparDeclsPostfixListNode) : TyparDeclsPostfixListNode =
        let decls' = n.Decls |> mapList(rewriteTyparDeclNode f)
        let constraints' = n.Constraints |> mapList(rewriteTypeConstraint f)

        if refEq decls' n.Decls && refEq constraints' n.Constraints then
            n
        else
            TyparDeclsPostfixListNode(n.LessThan, decls', constraints', n.GreaterThan, n.Range)

    and rewriteTyparDeclsPrefixListNode (f: Expr -> Expr) (n: TyparDeclsPrefixListNode) : TyparDeclsPrefixListNode =
        let decls' = n.Decls |> mapList(rewriteTyparDeclNode f)

        if refEq decls' n.Decls then
            n
        else
            TyparDeclsPrefixListNode(n.OpeningParen, decls', n.ClosingParen, n.Range)

    // ===== Pattern nodes =====

    and rewritePatLeftMiddleRight (f: Expr -> Expr) (n: PatLeftMiddleRight) : PatLeftMiddleRight =
        let lhs' = rewritePattern f n.LeftHandSide
        let rhs' = rewritePattern f n.RightHandSide

        if refEq lhs' n.LeftHandSide && refEq rhs' n.RightHandSide then
            n
        else
            PatLeftMiddleRight(lhs', n.Middle, rhs', n.Range)

    and rewritePatAndsNode (f: Expr -> Expr) (n: PatAndsNode) : PatAndsNode =
        let pats' = n.Patterns |> mapList(rewritePattern f)

        if refEq pats' n.Patterns then
            n
        else
            PatAndsNode(pats', n.Range)

    and rewritePatParameterNode (f: Expr -> Expr) (n: PatParameterNode) : PatParameterNode =
        let attrs' = n.Attributes |> mapOption(rewriteMultipleAttributeListNode f)
        let pat' = rewritePattern f n.Pattern
        let t' = n.Type |> mapOption(rewriteType f)

        if refEq attrs' n.Attributes && refEq pat' n.Pattern && refEq t' n.Type then
            n
        else
            PatParameterNode(attrs', pat', t', n.Range)

    and rewritePatNamePatPairsNode (f: Expr -> Expr) (n: PatNamePatPairsNode) : PatNamePatPairsNode =
        let typars' = n.TyparDecls |> mapOption(rewriteTyparDecls f)
        let pairs' = n.Pairs |> mapList(rewriteNamePatPair f)

        if refEq typars' n.TyparDecls && refEq pairs' n.Pairs then
            n
        else
            PatNamePatPairsNode(n.Identifier, typars', n.OpeningParen, pairs', n.ClosingParen, n.Range)

    and rewriteNamePatPair (f: Expr -> Expr) (n: NamePatPair) : NamePatPair =
        let pat' = rewritePattern f n.Pattern

        if refEq pat' n.Pattern then
            n
        else
            NamePatPair(n.Ident, n.Equals, pat', n.Range)

    and rewritePatLongIdentNode (f: Expr -> Expr) (n: PatLongIdentNode) : PatLongIdentNode =
        let typars' = n.TyparDecls |> mapOption(rewriteTyparDecls f)
        let parameters' = n.Parameters |> mapList(rewritePattern f)

        if refEq typars' n.TyparDecls && refEq parameters' n.Parameters then
            n
        else
            PatLongIdentNode(n.Accessibility, n.Identifier, typars', parameters', n.Range)

    and rewritePatParenNode (f: Expr -> Expr) (n: PatParenNode) : PatParenNode =
        let pat' = rewritePattern f n.Pattern

        if refEq pat' n.Pattern then
            n
        else
            PatParenNode(n.OpeningParen, pat', n.ClosingParen, n.Range)

    and rewritePatTupleNode (f: Expr -> Expr) (n: PatTupleNode) : PatTupleNode =
        let mutable changed = false

        let items' =
            n.Items
            |> List.map(fun choice ->
                match choice with
                | Choice1Of2 p ->
                    let p' = rewritePattern f p

                    if not(refEq p p') then
                        changed <- true

                    Choice1Of2 p'
                | other -> other)

        if not changed then n else PatTupleNode(items', n.Range)

    and rewritePatStructTupleNode (f: Expr -> Expr) (n: PatStructTupleNode) : PatStructTupleNode =
        let pats' = n.Patterns |> mapList(rewritePattern f)

        if refEq pats' n.Patterns then
            n
        else
            PatStructTupleNode(pats', n.Range)

    and rewritePatArrayOrListNode (f: Expr -> Expr) (n: PatArrayOrListNode) : PatArrayOrListNode =
        let pats' = n.Patterns |> mapList(rewritePattern f)

        if refEq pats' n.Patterns then
            n
        else
            PatArrayOrListNode(n.OpenToken, pats', n.CloseToken, n.Range)

    and rewritePatRecordNode (f: Expr -> Expr) (n: PatRecordNode) : PatRecordNode =
        let fields' = n.Fields |> mapList(rewritePatRecordField f)

        if refEq fields' n.Fields then
            n
        else
            PatRecordNode(n.OpeningNode, fields', n.ClosingNode, n.Range)

    and rewritePatRecordField (f: Expr -> Expr) (n: PatRecordField) : PatRecordField =
        let pat' = rewritePattern f n.Pattern

        if refEq pat' n.Pattern then
            n
        else
            PatRecordField(n.Prefix, n.FieldName, n.Equals, pat', n.Range)

    and rewritePatIsInstNode (f: Expr -> Expr) (n: PatIsInstNode) : PatIsInstNode =
        let t' = rewriteType f n.Type

        if refEq t' n.Type then
            n
        else
            PatIsInstNode(n.Token, t', n.Range)

    // ===== Expr nodes =====

    and rewriteExprLazyNode (f: Expr -> Expr) (n: ExprLazyNode) : ExprLazyNode =
        let e' = rewriteExpr f n.Expr

        if refEq e' n.Expr then
            n
        else
            ExprLazyNode(n.LazyWord, e', n.Range)

    and rewriteExprSingleNode (f: Expr -> Expr) (n: ExprSingleNode) : ExprSingleNode =
        let e' = rewriteExpr f n.Expr

        if refEq e' n.Expr then
            n
        else
            ExprSingleNode(n.Leading, n.AddSpace, n.SupportsStroustrup, e', n.Range)

    and rewriteExprQuoteNode (f: Expr -> Expr) (n: ExprQuoteNode) : ExprQuoteNode =
        let e' = rewriteExpr f n.Expr

        if refEq e' n.Expr then
            n
        else
            ExprQuoteNode(n.OpenToken, e', n.CloseToken, n.Range)

    and rewriteExprTypedNode (f: Expr -> Expr) (n: ExprTypedNode) : ExprTypedNode =
        let e' = rewriteExpr f n.Expr
        let t' = rewriteType f n.Type

        if refEq e' n.Expr && refEq t' n.Type then
            n
        else
            ExprTypedNode(e', n.Operator, t', n.Range)

    and rewriteExprNewNode (f: Expr -> Expr) (n: ExprNewNode) : ExprNewNode =
        let t' = rewriteType f n.Type
        let args' = rewriteExpr f n.Arguments

        if refEq t' n.Type && refEq args' n.Arguments then
            n
        else
            ExprNewNode(n.NewKeyword, t', args', n.Range)

    and rewriteExprTupleNode (f: Expr -> Expr) (n: ExprTupleNode) : ExprTupleNode =
        let mutable changed = false

        let items' =
            n.Items
            |> List.map(fun choice ->
                match choice with
                | Choice1Of2 e ->
                    let e' = rewriteExpr f e

                    if not(refEq e e') then
                        changed <- true

                    Choice1Of2 e'
                | other -> other)

        if not changed then n else ExprTupleNode(items', n.Range)

    and rewriteExprStructTupleNode (f: Expr -> Expr) (n: ExprStructTupleNode) : ExprStructTupleNode =
        let tuple' = rewriteExprTupleNode f n.Tuple

        if refEq tuple' n.Tuple then
            n
        else
            ExprStructTupleNode(n.Struct, tuple', n.ClosingParen, n.Range)

    and rewriteExprArrayOrListNode (f: Expr -> Expr) (n: ExprArrayOrListNode) : ExprArrayOrListNode =
        let elements' = n.Elements |> mapList(rewriteExpr f)

        if refEq elements' n.Elements then
            n
        else
            ExprArrayOrListNode(n.Opening, elements', n.Closing, n.Range)

    and rewriteExprRecordNode (f: Expr -> Expr) (n: ExprRecordNode) : ExprRecordNode =
        let copyInfo' = n.CopyInfo |> mapOption(rewriteExpr f)
        let fields' = n.Fields |> mapList(rewriteRecordFieldNode f)

        if refEq copyInfo' n.CopyInfo && refEq fields' n.Fields then
            n
        else
            ExprRecordNode(n.OpeningBrace, copyInfo', fields', n.ClosingBrace, n.Range)

    and rewriteRecordFieldNode (f: Expr -> Expr) (n: RecordFieldNode) : RecordFieldNode =
        let e' = rewriteExpr f n.Expr

        if refEq e' n.Expr then
            n
        else
            RecordFieldNode(n.FieldName, n.Equals, e', n.Range)

    and rewriteExprInheritRecordNode (f: Expr -> Expr) (n: ExprInheritRecordNode) : ExprInheritRecordNode =
        let ic' = rewriteInheritConstructor f n.InheritConstructor
        let fields' = n.Fields |> mapList(rewriteRecordFieldNode f)

        if refEq ic' n.InheritConstructor && refEq fields' n.Fields then
            n
        else
            ExprInheritRecordNode(n.OpeningBrace, ic', fields', n.ClosingBrace, n.Range)

    and rewriteExprAnonStructRecordNode (f: Expr -> Expr) (n: ExprAnonStructRecordNode) : ExprAnonStructRecordNode =
        let copyInfo' = n.CopyInfo |> mapOption(rewriteExpr f)
        let fields' = n.Fields |> mapList(rewriteRecordFieldNode f)

        if refEq copyInfo' n.CopyInfo && refEq fields' n.Fields then
            n
        else
            ExprAnonStructRecordNode(n.Struct, n.OpeningBrace, copyInfo', fields', n.ClosingBrace, n.Range)

    and rewriteExprObjExprNode (f: Expr -> Expr) (n: ExprObjExprNode) : ExprObjExprNode =
        let t' = rewriteType f n.Type
        let e' = n.Expr |> mapOption(rewriteExpr f)
        let bindings' = n.Bindings |> mapList(rewriteBindingNode f)
        let members' = n.Members |> mapList(rewriteMemberDefn f)
        let interfaces' = n.Interfaces |> mapList(rewriteInterfaceImplNode f)

        if
            refEq t' n.Type
            && refEq e' n.Expr
            && refEq bindings' n.Bindings
            && refEq members' n.Members
            && refEq interfaces' n.Interfaces
        then
            n
        else
            ExprObjExprNode(
                n.OpeningBrace,
                n.New,
                t',
                e',
                n.With,
                bindings',
                members',
                interfaces',
                n.ClosingBrace,
                n.Range
            )

    and rewriteExprWhileNode (f: Expr -> Expr) (n: ExprWhileNode) : ExprWhileNode =
        let we' = rewriteExpr f n.WhileExpr
        let de' = rewriteExpr f n.DoExpr

        if refEq we' n.WhileExpr && refEq de' n.DoExpr then
            n
        else
            ExprWhileNode(n.While, we', de', n.Range)

    and rewriteExprForNode (f: Expr -> Expr) (n: ExprForNode) : ExprForNode =
        let ib' = rewriteExpr f n.IdentBody
        let tb' = rewriteExpr f n.ToBody
        let db' = rewriteExpr f n.DoBody

        if refEq ib' n.IdentBody && refEq tb' n.ToBody && refEq db' n.DoBody then
            n
        else
            ExprForNode(n.For, n.Ident, n.Equals, ib', n.Direction, tb', db', n.Range)

    and rewriteExprForEachNode (f: Expr -> Expr) (n: ExprForEachNode) : ExprForEachNode =
        let p' = rewritePattern f n.Pattern
        let ee' = rewriteExpr f n.EnumExpr
        let be' = rewriteExpr f n.BodyExpr

        if refEq p' n.Pattern && refEq ee' n.EnumExpr && refEq be' n.BodyExpr then
            n
        else
            ExprForEachNode(n.For, p', ee', n.IsArrow, be', n.Range)

    and rewriteExprNamedComputationNode (f: Expr -> Expr) (n: ExprNamedComputationNode) : ExprNamedComputationNode =
        let ne' = rewriteExpr f n.Name
        let be' = rewriteExpr f n.Body

        if refEq ne' n.Name && refEq be' n.Body then
            n
        else
            ExprNamedComputationNode(ne', n.OpeningBrace, be', n.ClosingBrace, n.Range)

    and rewriteExprComputationNode (f: Expr -> Expr) (n: ExprComputationNode) : ExprComputationNode =
        let be' = rewriteExpr f n.Body

        if refEq be' n.Body then
            n
        else
            ExprComputationNode(n.OpeningBrace, be', n.ClosingBrace, n.Range)

    and rewriteExprCompExprBodyNode (f: Expr -> Expr) (n: ExprCompExprBodyNode) : ExprCompExprBodyNode =
        let stmts' = n.Statements |> mapList(rewriteComputationExpressionStatement f)

        if refEq stmts' n.Statements then
            n
        else
            ExprCompExprBodyNode(stmts', n.Range)

    and rewriteExprJoinInNode (f: Expr -> Expr) (n: ExprJoinInNode) : ExprJoinInNode =
        let lhs' = rewriteExpr f n.LeftHandSide
        let rhs' = rewriteExpr f n.RightHandSide

        if refEq lhs' n.LeftHandSide && refEq rhs' n.RightHandSide then
            n
        else
            ExprJoinInNode(lhs', n.In, rhs', n.Range)

    and rewriteExprParenLambdaNode (f: Expr -> Expr) (n: ExprParenLambdaNode) : ExprParenLambdaNode =
        let lambda' = rewriteExprLambdaNode f n.Lambda

        if refEq lambda' n.Lambda then
            n
        else
            ExprParenLambdaNode(n.OpeningParen, lambda', n.ClosingParen, n.Range)

    and rewriteExprLambdaNode (f: Expr -> Expr) (n: ExprLambdaNode) : ExprLambdaNode =
        let parameters' = n.Parameters |> mapList(rewritePattern f)
        let e' = rewriteExpr f n.Expr

        if refEq parameters' n.Parameters && refEq e' n.Expr then
            n
        else
            ExprLambdaNode(n.Fun, parameters', n.Arrow, e', n.Range)

    and rewriteExprMatchLambdaNode (f: Expr -> Expr) (n: ExprMatchLambdaNode) : ExprMatchLambdaNode =
        let clauses' = n.Clauses |> mapList(rewriteMatchClauseNode f)

        if refEq clauses' n.Clauses then
            n
        else
            ExprMatchLambdaNode(n.Function, clauses', n.Range)

    and rewriteExprMatchNode (f: Expr -> Expr) (n: ExprMatchNode) : ExprMatchNode =
        let me' = rewriteExpr f n.MatchExpr
        let clauses' = n.Clauses |> mapList(rewriteMatchClauseNode f)

        if refEq me' n.MatchExpr && refEq clauses' n.Clauses then
            n
        else
            ExprMatchNode(n.Match, me', n.With, clauses', n.Range)

    and rewriteMatchClauseNode (f: Expr -> Expr) (n: MatchClauseNode) : MatchClauseNode =
        let p' = rewritePattern f n.Pattern
        let we' = n.WhenExpr |> mapOption(rewriteExpr f)
        let be' = rewriteExpr f n.BodyExpr

        if refEq p' n.Pattern && refEq we' n.WhenExpr && refEq be' n.BodyExpr then
            n
        else
            MatchClauseNode(n.Bar, p', we', n.Arrow, be', n.Range)

    and rewriteExprTraitCallNode (f: Expr -> Expr) (n: ExprTraitCallNode) : ExprTraitCallNode =
        let t' = rewriteType f n.Type
        let m' = rewriteMemberDefn f n.MemberDefn
        let e' = rewriteExpr f n.Expr

        if refEq t' n.Type && refEq m' n.MemberDefn && refEq e' n.Expr then
            n
        else
            ExprTraitCallNode(t', m', e', n.Range)

    and rewriteExprParenNode (f: Expr -> Expr) (n: ExprParenNode) : ExprParenNode =
        let e' = rewriteExpr f n.Expr

        if refEq e' n.Expr then
            n
        else
            ExprParenNode(n.OpeningParen, e', n.ClosingParen, n.Range)

    and rewriteExprDynamicNode (f: Expr -> Expr) (n: ExprDynamicNode) : ExprDynamicNode =
        let func' = rewriteExpr f n.FuncExpr
        let arg' = rewriteExpr f n.ArgExpr

        if refEq func' n.FuncExpr && refEq arg' n.ArgExpr then
            n
        else
            ExprDynamicNode(func', arg', n.Range)

    and rewriteExprPrefixAppNode (f: Expr -> Expr) (n: ExprPrefixAppNode) : ExprPrefixAppNode =
        let e' = rewriteExpr f n.Expr

        if refEq e' n.Expr then
            n
        else
            ExprPrefixAppNode(n.Operator, e', n.Range)

    and rewriteExprSameInfixAppsNode (f: Expr -> Expr) (n: ExprSameInfixAppsNode) : ExprSameInfixAppsNode =
        let leading' = rewriteExpr f n.LeadingExpr
        let mutable changed = false

        let subs' =
            n.SubsequentExpressions
            |> List.map(fun (op, e) ->
                let e' = rewriteExpr f e

                if not(refEq e e') then
                    changed <- true

                (op, e'))

        if refEq leading' n.LeadingExpr && not changed then
            n
        else
            ExprSameInfixAppsNode(leading', subs', n.Range)

    and rewriteExprInfixAppNode (f: Expr -> Expr) (n: ExprInfixAppNode) : ExprInfixAppNode =
        let lhs' = rewriteExpr f n.LeftHandSide
        let rhs' = rewriteExpr f n.RightHandSide

        if refEq lhs' n.LeftHandSide && refEq rhs' n.RightHandSide then
            n
        else
            ExprInfixAppNode(lhs', n.Operator, rhs', n.Range)

    and rewriteExprIndexWithoutDotNode (f: Expr -> Expr) (n: ExprIndexWithoutDotNode) : ExprIndexWithoutDotNode =
        let id' = rewriteExpr f n.Identifier
        let idx' = rewriteExpr f n.Index

        if refEq id' n.Identifier && refEq idx' n.Index then
            n
        else
            ExprIndexWithoutDotNode(id', idx', n.Range)

    and rewriteExprAppLongIdentAndSingleParenArgNode
        (f: Expr -> Expr)
        (n: ExprAppLongIdentAndSingleParenArgNode)
        : ExprAppLongIdentAndSingleParenArgNode =
        let arg' = rewriteExpr f n.ArgExpr

        if refEq arg' n.ArgExpr then
            n
        else
            ExprAppLongIdentAndSingleParenArgNode(n.FunctionName, arg', n.Range)

    and rewriteExprAppSingleParenArgNode (f: Expr -> Expr) (n: ExprAppSingleParenArgNode) : ExprAppSingleParenArgNode =
        let func' = rewriteExpr f n.FunctionExpr
        let arg' = rewriteExpr f n.ArgExpr

        if refEq func' n.FunctionExpr && refEq arg' n.ArgExpr then
            n
        else
            ExprAppSingleParenArgNode(func', arg', n.Range)

    and rewriteExprAppWithLambdaNode (f: Expr -> Expr) (n: ExprAppWithLambdaNode) : ExprAppWithLambdaNode =
        let func' = rewriteExpr f n.FunctionName
        let args' = n.Arguments |> mapList(rewriteExpr f)

        let lambda' =
            match n.Lambda with
            | Choice1Of2 l ->
                let l' = rewriteExprLambdaNode f l
                if refEq l l' then n.Lambda else Choice1Of2 l'
            | Choice2Of2 ml ->
                let ml' = rewriteExprMatchLambdaNode f ml
                if refEq ml ml' then n.Lambda else Choice2Of2 ml'

        if refEq func' n.FunctionName && refEq args' n.Arguments && refEq lambda' n.Lambda then
            n
        else
            ExprAppWithLambdaNode(func', args', n.OpeningParen, lambda', n.ClosingParen, n.Range)

    and rewriteExprNestedIndexWithoutDotNode
        (f: Expr -> Expr)
        (n: ExprNestedIndexWithoutDotNode)
        : ExprNestedIndexWithoutDotNode =
        let id' = rewriteExpr f n.Identifier
        let idx' = rewriteExpr f n.Index
        let arg' = rewriteExpr f n.Argument

        if refEq id' n.Identifier && refEq idx' n.Index && refEq arg' n.Argument then
            n
        else
            ExprNestedIndexWithoutDotNode(id', idx', arg', n.Range)

    and rewriteExprAppNode (f: Expr -> Expr) (n: ExprAppNode) : ExprAppNode =
        let func' = rewriteExpr f n.FunctionExpr
        let args' = n.Arguments |> mapList(rewriteExpr f)

        if refEq func' n.FunctionExpr && refEq args' n.Arguments then
            n
        else
            ExprAppNode(func', args', n.Range)

    and rewriteExprTypeAppNode (f: Expr -> Expr) (n: ExprTypeAppNode) : ExprTypeAppNode =
        let id' = rewriteExpr f n.Identifier
        let typeParams' = n.TypeParameters |> mapList(rewriteType f)

        if refEq id' n.Identifier && refEq typeParams' n.TypeParameters then
            n
        else
            ExprTypeAppNode(id', n.LessThan, typeParams', n.GreaterThan, n.Range)

    and rewriteExprTryWithSingleClauseNode
        (f: Expr -> Expr)
        (n: ExprTryWithSingleClauseNode)
        : ExprTryWithSingleClauseNode =
        let try' = rewriteExpr f n.TryExpr
        let clause' = rewriteMatchClauseNode f n.Clause

        if refEq try' n.TryExpr && refEq clause' n.Clause then
            n
        else
            ExprTryWithSingleClauseNode(n.Try, try', n.With, clause', n.Range)

    and rewriteExprTryWithNode (f: Expr -> Expr) (n: ExprTryWithNode) : ExprTryWithNode =
        let try' = rewriteExpr f n.TryExpr
        let clauses' = n.Clauses |> mapList(rewriteMatchClauseNode f)

        if refEq try' n.TryExpr && refEq clauses' n.Clauses then
            n
        else
            ExprTryWithNode(n.Try, try', n.With, clauses', n.Range)

    and rewriteExprTryFinallyNode (f: Expr -> Expr) (n: ExprTryFinallyNode) : ExprTryFinallyNode =
        let try' = rewriteExpr f n.TryExpr
        let fin' = rewriteExpr f n.FinallyExpr

        if refEq try' n.TryExpr && refEq fin' n.FinallyExpr then
            n
        else
            ExprTryFinallyNode(n.Try, try', n.Finally, fin', n.Range)

    and rewriteExprIfThenNode (f: Expr -> Expr) (n: ExprIfThenNode) : ExprIfThenNode =
        let ie' = rewriteExpr f n.IfExpr
        let te' = rewriteExpr f n.ThenExpr

        if refEq ie' n.IfExpr && refEq te' n.ThenExpr then
            n
        else
            ExprIfThenNode(n.If, ie', n.Then, te', n.Range)

    and rewriteExprIfThenElseNode (f: Expr -> Expr) (n: ExprIfThenElseNode) : ExprIfThenElseNode =
        let ie' = rewriteExpr f n.IfExpr
        let te' = rewriteExpr f n.ThenExpr
        let ee' = rewriteExpr f n.ElseExpr

        if refEq ie' n.IfExpr && refEq te' n.ThenExpr && refEq ee' n.ElseExpr then
            n
        else
            ExprIfThenElseNode(n.If, ie', n.Then, te', n.Else, ee', n.Range)

    and rewriteExprIfThenElifNode (f: Expr -> Expr) (n: ExprIfThenElifNode) : ExprIfThenElifNode =
        let branches' = n.Branches |> mapList(rewriteExprIfThenNode f)

        let elseBranch' =
            match n.Else with
            | None -> n.Else
            | Some(stn, e) ->
                let e' = rewriteExpr f e
                if refEq e e' then n.Else else Some(stn, e')

        if refEq branches' n.Branches && refEq elseBranch' n.Else then
            n
        else
            ExprIfThenElifNode(branches', elseBranch', n.Range)

    and rewriteExprLongIdentSetNode (f: Expr -> Expr) (n: ExprLongIdentSetNode) : ExprLongIdentSetNode =
        let rhs' = rewriteExpr f n.Expr

        if refEq rhs' n.Expr then
            n
        else
            ExprLongIdentSetNode(n.Identifier, rhs', n.Range)

    and rewriteExprDotIndexedGetNode (f: Expr -> Expr) (n: ExprDotIndexedGetNode) : ExprDotIndexedGetNode =
        let obj' = rewriteExpr f n.ObjectExpr
        let idx' = rewriteExpr f n.IndexExpr

        if refEq obj' n.ObjectExpr && refEq idx' n.IndexExpr then
            n
        else
            ExprDotIndexedGetNode(obj', idx', n.Range)

    and rewriteExprDotIndexedSetNode (f: Expr -> Expr) (n: ExprDotIndexedSetNode) : ExprDotIndexedSetNode =
        let obj' = rewriteExpr f n.ObjectExpr
        let idx' = rewriteExpr f n.Index
        let v' = rewriteExpr f n.Value

        if refEq obj' n.ObjectExpr && refEq idx' n.Index && refEq v' n.Value then
            n
        else
            ExprDotIndexedSetNode(obj', idx', v', n.Range)

    and rewriteExprNamedIndexedPropertySetNode
        (f: Expr -> Expr)
        (n: ExprNamedIndexedPropertySetNode)
        : ExprNamedIndexedPropertySetNode =
        let idx' = rewriteExpr f n.Index
        let v' = rewriteExpr f n.Value

        if refEq idx' n.Index && refEq v' n.Value then
            n
        else
            ExprNamedIndexedPropertySetNode(n.Identifier, idx', v', n.Range)

    and rewriteExprDotNamedIndexedPropertySetNode
        (f: Expr -> Expr)
        (n: ExprDotNamedIndexedPropertySetNode)
        : ExprDotNamedIndexedPropertySetNode =
        let id' = rewriteExpr f n.Identifier
        let p' = rewriteExpr f n.Property
        let s' = rewriteExpr f n.Set

        if refEq id' n.Identifier && refEq p' n.Property && refEq s' n.Set then
            n
        else
            ExprDotNamedIndexedPropertySetNode(id', n.Name, p', s', n.Range)

    and rewriteExprSetNode (f: Expr -> Expr) (n: ExprSetNode) : ExprSetNode =
        let id' = rewriteExpr f n.Identifier
        let s' = rewriteExpr f n.Set

        if refEq id' n.Identifier && refEq s' n.Set then
            n
        else
            ExprSetNode(id', s', n.Range)

    and rewriteExprLibraryOnlyStaticOptimizationNode
        (f: Expr -> Expr)
        (n: ExprLibraryOnlyStaticOptimizationNode)
        : ExprLibraryOnlyStaticOptimizationNode =
        let opt' = rewriteExpr f n.OptimizedExpr
        let e' = rewriteExpr f n.Expr

        if refEq opt' n.OptimizedExpr && refEq e' n.Expr then
            n
        else
            ExprLibraryOnlyStaticOptimizationNode(opt', n.Constraints, e', n.Range)

    and rewriteExprInterpolatedStringExprNode
        (f: Expr -> Expr)
        (n: ExprInterpolatedStringExprNode)
        : ExprInterpolatedStringExprNode =
        let mutable changed = false

        let parts' =
            n.Parts
            |> List.map(fun choice ->
                match choice with
                | Choice2Of2 fillExpr ->
                    let fillExpr' = rewriteFillExprNode f fillExpr

                    if not(refEq fillExpr fillExpr') then
                        changed <- true

                    Choice2Of2 fillExpr'
                | other -> other)

        if not changed then
            n
        else
            ExprInterpolatedStringExprNode(parts', n.Range)

    and rewriteFillExprNode (f: Expr -> Expr) (n: FillExprNode) : FillExprNode =
        let e' = rewriteExpr f n.Expr

        if refEq e' n.Expr then
            n
        else
            FillExprNode(e', n.Ident, n.Range)

    and rewriteExprIndexRangeNode (f: Expr -> Expr) (n: ExprIndexRangeNode) : ExprIndexRangeNode =
        let from' = n.From |> mapOption(rewriteExpr f)
        let to' = n.To |> mapOption(rewriteExpr f)

        if refEq from' n.From && refEq to' n.To then
            n
        else
            ExprIndexRangeNode(from', n.Dots, to', n.Range)

    and rewriteExprIndexFromEndNode (f: Expr -> Expr) (n: ExprIndexFromEndNode) : ExprIndexFromEndNode =
        let e' = rewriteExpr f n.Expr

        if refEq e' n.Expr then
            n
        else
            ExprIndexFromEndNode(e', n.Range)

    and rewriteExprChain (f: Expr -> Expr) (n: ExprChain) : ExprChain =
        let links' = n.Links |> mapList(rewriteChainLink f)

        if refEq links' n.Links then
            n
        else
            ExprChain(links', n.Range)

    and rewriteLinkSingleAppParen (f: Expr -> Expr) (n: LinkSingleAppParen) : LinkSingleAppParen =
        let func' = rewriteExpr f n.FunctionName
        let paren' = rewriteExprParenNode f n.Paren

        if refEq func' n.FunctionName && refEq paren' n.Paren then
            n
        else
            LinkSingleAppParen(func', paren', n.Range)

    and rewriteLinkSingleAppUnit (f: Expr -> Expr) (n: LinkSingleAppUnit) : LinkSingleAppUnit =
        let func' = rewriteExpr f n.FunctionName

        if refEq func' n.FunctionName then
            n
        else
            LinkSingleAppUnit(func', n.Unit, n.Range)

    and rewriteExprDotLambda (f: Expr -> Expr) (n: ExprDotLambda) : ExprDotLambda =
        let e' = rewriteExpr f n.Expr

        if refEq e' n.Expr then
            n
        else
            ExprDotLambda(n.Underscore, n.Dot, e', n.Range)

    and rewriteExprBeginEndNode (f: Expr -> Expr) (n: ExprBeginEndNode) : ExprBeginEndNode =
        let e' = rewriteExpr f n.Expr

        if refEq e' n.Expr then
            n
        else
            ExprBeginEndNode(n.Begin, e', n.End, n.Range)

    and rewriteExprExplicitConstructorThenExpr
        (f: Expr -> Expr)
        (n: ExprExplicitConstructorThenExpr)
        : ExprExplicitConstructorThenExpr =
        let e' = rewriteExpr f n.Expr

        if refEq e' n.Expr then
            n
        else
            ExprExplicitConstructorThenExpr(n.Then, e', n.Range)

    and rewriteExprLetOrUseNode (f: Expr -> Expr) (n: ExprLetOrUseNode) : ExprLetOrUseNode =
        let b' = rewriteBindingNode f n.Binding

        if refEq b' n.Binding then
            n
        else
            ExprLetOrUseNode(b', n.In, n.Range)

    and rewriteExprLetOrUseBangNode (f: Expr -> Expr) (n: ExprLetOrUseBangNode) : ExprLetOrUseBangNode =
        let p' = rewritePattern f n.Pattern
        let e' = rewriteExpr f n.Expression

        if refEq p' n.Pattern && refEq e' n.Expression then
            n
        else
            ExprLetOrUseBangNode(n.LeadingKeyword, p', n.Equals, e', n.Range)

    and rewriteExprAndBang (f: Expr -> Expr) (n: ExprAndBang) : ExprAndBang =
        let p' = rewritePattern f n.Pattern
        let e' = rewriteExpr f n.Expression

        if refEq p' n.Pattern && refEq e' n.Expression then
            n
        else
            ExprAndBang(n.LeadingKeyword, p', n.Equals, e', n.Range)

    // ===== InheritConstructor nodes =====

    and rewriteInheritConstructorTypeOnlyNode
        (f: Expr -> Expr)
        (n: InheritConstructorTypeOnlyNode)
        : InheritConstructorTypeOnlyNode =
        let t' = rewriteType f n.Type

        if refEq t' n.Type then
            n
        else
            InheritConstructorTypeOnlyNode(n.InheritKeyword, t', n.Range)

    and rewriteInheritConstructorUnitNode
        (f: Expr -> Expr)
        (n: InheritConstructorUnitNode)
        : InheritConstructorUnitNode =
        let t' = rewriteType f n.Type

        if refEq t' n.Type then
            n
        else
            InheritConstructorUnitNode(n.InheritKeyword, t', n.OpeningParen, n.ClosingParen, n.Range)

    and rewriteInheritConstructorParenNode
        (f: Expr -> Expr)
        (n: InheritConstructorParenNode)
        : InheritConstructorParenNode =
        let t' = rewriteType f n.Type
        let e' = rewriteExpr f n.Expr

        if refEq t' n.Type && refEq e' n.Expr then
            n
        else
            InheritConstructorParenNode(n.InheritKeyword, t', e', n.Range)

    and rewriteInheritConstructorOtherNode
        (f: Expr -> Expr)
        (n: InheritConstructorOtherNode)
        : InheritConstructorOtherNode =
        let t' = rewriteType f n.Type
        let e' = rewriteExpr f n.Expr

        if refEq t' n.Type && refEq e' n.Expr then
            n
        else
            InheritConstructorOtherNode(n.InheritKeyword, t', e', n.Range)

    // ===== MemberDefn nodes =====

    and rewriteMemberDefnInheritNode (f: Expr -> Expr) (n: MemberDefnInheritNode) : MemberDefnInheritNode =
        let t' = rewriteType f n.BaseType

        if refEq t' n.BaseType then
            n
        else
            MemberDefnInheritNode(n.Inherit, t', n.Range)

    and rewriteMemberDefnExplicitCtorNode
        (f: Expr -> Expr)
        (n: MemberDefnExplicitCtorNode)
        : MemberDefnExplicitCtorNode =
        let attrs' = n.Attributes |> mapOption(rewriteMultipleAttributeListNode f)
        let p' = rewritePattern f n.Pattern
        let e' = rewriteExpr f n.Expr

        if refEq attrs' n.Attributes && refEq p' n.Pattern && refEq e' n.Expr then
            n
        else
            MemberDefnExplicitCtorNode(n.XmlDoc, attrs', n.Accessibility, n.New, p', n.Alias, n.Equals, e', n.Range)

    and rewriteMemberDefnInterfaceNode (f: Expr -> Expr) (n: MemberDefnInterfaceNode) : MemberDefnInterfaceNode =
        let t' = rewriteType f n.Type
        let members' = n.Members |> mapList(rewriteMemberDefn f)

        if refEq t' n.Type && refEq members' n.Members then
            n
        else
            MemberDefnInterfaceNode(n.Interface, t', n.With, members', n.Range)

    and rewriteMemberDefnAutoPropertyNode
        (f: Expr -> Expr)
        (n: MemberDefnAutoPropertyNode)
        : MemberDefnAutoPropertyNode =
        let attrs' = n.Attributes |> mapOption(rewriteMultipleAttributeListNode f)
        let t' = n.Type |> mapOption(rewriteType f)
        let e' = rewriteExpr f n.Expr

        if refEq attrs' n.Attributes && refEq t' n.Type && refEq e' n.Expr then
            n
        else
            MemberDefnAutoPropertyNode(
                n.XmlDoc,
                attrs',
                n.LeadingKeyword,
                n.Accessibility,
                n.Identifier,
                t',
                n.Equals,
                e',
                n.WithGetSet,
                n.Range
            )

    and rewriteMemberDefnAbstractSlotNode
        (f: Expr -> Expr)
        (n: MemberDefnAbstractSlotNode)
        : MemberDefnAbstractSlotNode =
        let attrs' = n.Attributes |> mapOption(rewriteMultipleAttributeListNode f)
        let typars' = n.TypeParams |> mapOption(rewriteTyparDecls f)
        let t' = rewriteType f n.Type

        if refEq attrs' n.Attributes && refEq typars' n.TypeParams && refEq t' n.Type then
            n
        else
            MemberDefnAbstractSlotNode(
                n.XmlDoc,
                attrs',
                n.LeadingKeyword,
                n.Identifier,
                typars',
                t',
                n.WithGetSet,
                n.Range
            )

    and rewriteMemberDefnPropertyGetSetNode
        (f: Expr -> Expr)
        (n: MemberDefnPropertyGetSetNode)
        : MemberDefnPropertyGetSetNode =
        let attrs' = n.Attributes |> mapOption(rewriteMultipleAttributeListNode f)
        let first' = rewritePropertyGetSetBindingNode f n.FirstBinding
        let last' = n.LastBinding |> mapOption(rewritePropertyGetSetBindingNode f)

        if
            refEq attrs' n.Attributes
            && refEq first' n.FirstBinding
            && refEq last' n.LastBinding
        then
            n
        else
            MemberDefnPropertyGetSetNode(
                n.XmlDoc,
                attrs',
                n.LeadingKeyword,
                n.Inline,
                n.Accessibility,
                n.MemberName,
                n.WithKeyword,
                first',
                n.AndKeyword,
                last',
                n.Range
            )

    and rewritePropertyGetSetBindingNode (f: Expr -> Expr) (n: PropertyGetSetBindingNode) : PropertyGetSetBindingNode =
        let attrs' = n.Attributes |> mapOption(rewriteMultipleAttributeListNode f)
        let parameters' = n.Parameters |> mapList(rewritePattern f)
        let returnType' = n.ReturnType |> mapOption(rewriteBindingReturnInfoNode f)
        let e' = rewriteExpr f n.Expr

        if
            refEq attrs' n.Attributes
            && refEq parameters' n.Parameters
            && refEq returnType' n.ReturnType
            && refEq e' n.Expr
        then
            n
        else
            PropertyGetSetBindingNode(
                n.Inline,
                attrs',
                n.Accessibility,
                n.LeadingKeyword,
                parameters',
                returnType',
                n.Equals,
                e',
                n.Range
            )

    and rewriteMemberDefnSigMemberNode (f: Expr -> Expr) (n: MemberDefnSigMemberNode) : MemberDefnSigMemberNode =
        let v' = rewriteValNode f n.Val

        if refEq v' n.Val then
            n
        else
            MemberDefnSigMemberNode(v', n.WithGetSet, n.Range)

    // ===== TypeDefn nodes =====
    // TypeName and Members live on the ITypeDefn interface, not the concrete class.

    and rewriteTypeDefnEnumNode (f: Expr -> Expr) (n: TypeDefnEnumNode) : TypeDefnEnumNode =
        let tn = (n :> ITypeDefn).TypeName
        let ms = (n :> ITypeDefn).Members
        let tn' = rewriteTypeNameNode f tn
        let cases' = n.EnumCases |> mapList(rewriteEnumCaseNode f)
        let members' = ms |> mapList(rewriteMemberDefn f)

        if refEq tn' tn && refEq cases' n.EnumCases && refEq members' ms then
            n
        else
            TypeDefnEnumNode(tn', cases', members', n.Range)

    and rewriteTypeDefnUnionNode (f: Expr -> Expr) (n: TypeDefnUnionNode) : TypeDefnUnionNode =
        let tn = (n :> ITypeDefn).TypeName
        let ms = (n :> ITypeDefn).Members
        let tn' = rewriteTypeNameNode f tn
        let cases' = n.UnionCases |> mapList(rewriteUnionCaseNode f)
        let members' = ms |> mapList(rewriteMemberDefn f)

        if refEq tn' tn && refEq cases' n.UnionCases && refEq members' ms then
            n
        else
            TypeDefnUnionNode(tn', n.Accessibility, cases', members', n.Range)

    and rewriteTypeDefnRecordNode (f: Expr -> Expr) (n: TypeDefnRecordNode) : TypeDefnRecordNode =
        let tn = (n :> ITypeDefn).TypeName
        let ms = (n :> ITypeDefn).Members
        let tn' = rewriteTypeNameNode f tn
        let fields' = n.Fields |> mapList(rewriteFieldNode f)
        let members' = ms |> mapList(rewriteMemberDefn f)

        if refEq tn' tn && refEq fields' n.Fields && refEq members' ms then
            n
        else
            TypeDefnRecordNode(tn', n.Accessibility, n.OpeningBrace, fields', n.ClosingBrace, members', n.Range)

    and rewriteTypeDefnAbbrevNode (f: Expr -> Expr) (n: TypeDefnAbbrevNode) : TypeDefnAbbrevNode =
        let tn = (n :> ITypeDefn).TypeName
        let ms = (n :> ITypeDefn).Members
        let tn' = rewriteTypeNameNode f tn
        let t' = rewriteType f n.Type
        let members' = ms |> mapList(rewriteMemberDefn f)

        if refEq tn' tn && refEq t' n.Type && refEq members' ms then
            n
        else
            TypeDefnAbbrevNode(tn', t', members', n.Range)

    and rewriteTypeDefnExplicitNode (f: Expr -> Expr) (n: TypeDefnExplicitNode) : TypeDefnExplicitNode =
        let tn = (n :> ITypeDefn).TypeName
        let ms = (n :> ITypeDefn).Members
        let tn' = rewriteTypeNameNode f tn
        let body' = rewriteTypeDefnExplicitBodyNode f n.Body
        let members' = ms |> mapList(rewriteMemberDefn f)

        if refEq tn' tn && refEq body' n.Body && refEq members' ms then
            n
        else
            TypeDefnExplicitNode(tn', body', members', n.Range)

    and rewriteTypeDefnExplicitBodyNode (f: Expr -> Expr) (n: TypeDefnExplicitBodyNode) : TypeDefnExplicitBodyNode =
        let members' = n.Members |> mapList(rewriteMemberDefn f)

        if refEq members' n.Members then
            n
        else
            TypeDefnExplicitBodyNode(n.Kind, members', n.End, n.Range)

    and rewriteTypeDefnAugmentationNode (f: Expr -> Expr) (n: TypeDefnAugmentationNode) : TypeDefnAugmentationNode =
        let tn = (n :> ITypeDefn).TypeName
        let ms = (n :> ITypeDefn).Members
        let tn' = rewriteTypeNameNode f tn
        let members' = ms |> mapList(rewriteMemberDefn f)

        if refEq tn' tn && refEq members' ms then
            n
        else
            TypeDefnAugmentationNode(tn', members', n.Range)

    and rewriteTypeDefnDelegateNode (f: Expr -> Expr) (n: TypeDefnDelegateNode) : TypeDefnDelegateNode =
        let tn = (n :> ITypeDefn).TypeName
        let tn' = rewriteTypeNameNode f tn
        let tl' = rewriteTypeFunsNode f n.TypeList

        if refEq tn' tn && refEq tl' n.TypeList then
            n
        else
            TypeDefnDelegateNode(tn', n.DelegateNode, tl', n.Range)

    and rewriteTypeDefnRegularNode (f: Expr -> Expr) (n: TypeDefnRegularNode) : TypeDefnRegularNode =
        let tn = (n :> ITypeDefn).TypeName
        let ms = (n :> ITypeDefn).Members
        let tn' = rewriteTypeNameNode f tn
        let members' = ms |> mapList(rewriteMemberDefn f)

        if refEq tn' tn && refEq members' ms then
            n
        else
            TypeDefnRegularNode(tn', members', n.Range)

/// <summary>
/// Bottom-up rewriter for expressions inside an Oak tree. Visits every
/// <see cref="T:Fantomas.Core.SyntaxOak.Expr"/> reachable from the input
/// (including those nested inside type definitions, member bodies, patterns,
/// types and attributes) and applies the caller-supplied function after each
/// node's children are rebuilt. Reference equality is preserved on unchanged
/// subtrees, so a no-op rewrite returns the same Oak instance.
/// </summary>
[<Class>]
type Rewrite =
    /// <summary>Rewrites every Expr in the Oak produced by <paramref name="widget"/>.</summary>
    static member expr(f: Expr -> Expr, widget: WidgetBuilder<Oak>) : WidgetBuilder<Oak> =
        let oak = Gen.mkOak widget
        let oak' = RewriteImpl.rewriteOak f oak

        if RewriteImpl.refEq oak oak' then
            widget
        else
            Ast.EscapeHatch oak'

    /// <summary>Rewrites every Expr reachable from <paramref name="expr"/>.</summary>
    static member expr(f: Expr -> Expr, expr: Expr) : Expr = RewriteImpl.rewriteExpr f expr

    /// <summary>
    /// Rewrites every Expr in a raw <see cref="T:Fantomas.Core.SyntaxOak.Oak"/>.
    /// Distinct name (not overloaded) because F# overload resolution can't
    /// reliably disambiguate <c>Oak</c> from <c>WidgetBuilder&lt;Oak&gt;</c>
    /// when the function argument is polymorphic (e.g. <c>id</c>).
    /// </summary>
    static member exprInOak(f: Expr -> Expr, oak: Oak) : Oak = RewriteImpl.rewriteOak f oak
