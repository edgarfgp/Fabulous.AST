namespace Fabulous.AST

open Fabulous.AST
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module ModuleDeclAttributes =
    let DoExpression = Attributes.defineWidget "DoExpression"

    let WidgetKey =
        Widgets.register "ModuleDeclAttributes" (fun widget ->
            let doExpression = Widgets.getNodeFromWidget<Expr> widget DoExpression

            let attributes =
                Widgets.tryGetScalarValue widget ModuleDecl.MultipleAttributes
                |> ValueOption.map(fun x -> Some(MultipleAttributeListNode.Create(x)))
                |> ValueOption.defaultValue None

            let node = ModuleDeclAttributesNode(attributes, doExpression, Range.Zero)
            ModuleDecl.Attributes(node))

[<AutoOpen>]
module ModuleDeclAttributeNodeBuilders =
    type Ast with

        /// <summary>Create a module declaration attribute with a do expression.</summary>
        /// <param name="doExpr">The do expression.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         ModuleDeclAttribute(AppExpr(" printfn", String "Executing..."))
        ///     }
        /// }
        /// </code>
        static member ModuleDeclAttribute(doExpr: WidgetBuilder<Expr>) =
            WidgetBuilder<ModuleDecl>(
                ModuleDeclAttributes.WidgetKey,
                ModuleDeclAttributes.DoExpression.WithValue(Ast.SingleExpr(Ast.DoExpr doExpr).Compile())
            )

        /// <summary>Create a module declaration attribute with a do expression.</summary>
        /// <param name="doExpr">The do expression.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         ModuleDeclAttribute(Constant(" printfn \"Executing...\""))
        ///     }
        /// }
        /// </code>
        static member ModuleDeclAttribute(doExpr: WidgetBuilder<Constant>) =
            Ast.ModuleDeclAttribute(Ast.ConstantExpr(doExpr))

        /// <summary>Create a module declaration attribute with a do expression.</summary>
        /// <param name="doExpr">The do expression.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         ModuleDeclAttribute(" printfn \"Executing...\"")
        ///     }
        /// }
        /// </code>
        static member ModuleDeclAttribute(doExpr: string) =
            Ast.ModuleDeclAttribute(Ast.Constant(doExpr))
