namespace Fabulous.AST

(* This file is here to demonstrate the creation of a computation expression
   that is able to act as an implicit module, removing the need to explicitly write Oak > ModuleOrNamespace.
   The actual use case for it is still to be determined *)

open type Fabulous.AST.Ast
open Fabulous.AST.WidgetCollectionAttributeDefinitions

type AstContent = { ModuleDecls: WidgetBuilder<IFabModuleDecl> list }

type RootAstBuilder() =
    member inline _.Zero() : AstContent =
        { ModuleDecls = [] }
    
    member _.Combine(a: AstContent, b: AstContent) : AstContent =
        { ModuleDecls = List.append a.ModuleDecls b.ModuleDecls }
             
    member x.Yield(widget: WidgetBuilder<IFabBinding>) =
        let topLevelBinding = TopLevelBinding(widget)
        
        let builder =
            WidgetBuilder<IFabModuleDecl>(
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
