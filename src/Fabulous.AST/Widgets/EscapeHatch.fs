namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fabulous.AST.StackAllocatedCollections
open Fantomas.Core.SyntaxOak

// This widget holds a direct reference to a manually created node so it can be composed in the widget tree
module EscapeHatch =
    let Node = Attributes.defineScalar<NodeBase> "Node"

    let WidgetKey =
        Widgets.register "EscapeHatch" (fun widget ->
            let node = Helpers.getScalarValue widget Node
            node // return the node immediately
        )

[<AutoOpen>]
module EscapeHatchBuilders =
    type Fabulous.AST.Ast with

        static member inline EscapeHatch<'T when 'T :> NodeBase>(node: 'T) =
            WidgetBuilder<'T>(EscapeHatch.WidgetKey, EscapeHatch.Node.WithValue(node))

[<Extension>]
type EscapeHatchYieldExtensions =
    [<Extension>]
    static member inline Yield(_: CollectionBuilder<'parent, ModuleDecl>, x: BindingNode) : Content =
        { Widgets = MutStackArray1.One(Ast.TopLevelBinding(Ast.EscapeHatch(x)).Compile()) }
