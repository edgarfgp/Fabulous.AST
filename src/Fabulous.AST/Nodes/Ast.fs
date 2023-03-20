namespace Fabulous.AST

open type Fabulous.AST.Node
open Fabulous.AST.WidgetCollectionAttributeDefinitions

type AstContent = { ModuleDecls: WidgetBuilder<IFabNodeBase> list }

type RootAstBuilder() =
    member inline _.Zero() : AstContent =
        { ModuleDecls = [] }
    
    member _.Combine(a: AstContent, b: AstContent) : AstContent =
        { ModuleDecls = List.append a.ModuleDecls b.ModuleDecls }
             
    member x.Yield(widget: WidgetBuilder<IFabLet>) =
        let topLevelBinding = TopLevelBinding(widget)
        
        let builder =
            WidgetBuilder<IFabNodeBase>(
                topLevelBinding.Key,
                topLevelBinding.Attributes
            )
        
        { ModuleDecls = [ builder ] }
        
    member x.Yield(widget: WidgetBuilder<IFabCall>) =
        let topLevelBinding = DeclExpr(widget)
        
        let builder =
            WidgetBuilder<IFabNodeBase>(
                topLevelBinding.Key,
                topLevelBinding.Attributes
            )
        
        { ModuleDecls = [ builder ] }
        
    member x.For<'t>(sequence: 't seq, f: 't -> AstContent) : AstContent =
        let mutable res: AstContent = x.Zero()

        // this is essentially Fold, not sure what is more optimal
        // handwritten version of via Seq.Fold
        for t in sequence do
            res <- x.Combine(res, f t)

        res
    
    member inline _.Delay([<InlineIfLambda>] f) : AstContent = f()
    
    member x.Run(c: AstContent) =
        Oak() {
            ModuleOrNamespace() {
                for moduleDecl in c.ModuleDecls do
                    moduleDecl
            }
        }
        
[<AutoOpen>]
module RootAstBuilder =
    let ast = RootAstBuilder()
