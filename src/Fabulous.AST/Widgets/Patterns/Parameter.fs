namespace Fabulous.AST

open Fabulous.AST
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module Parameter =
    let Value = Attributes.defineWidget "Value"
    let TypeVal = Attributes.defineWidget "Type"

    let WidgetKey =
        Widgets.register "Parameter" (fun widget ->
            let value = Widgets.getNodeFromWidget<Pattern> widget Value

            let typeValue =
                Widgets.tryGetNodeFromWidget widget TypeVal
                |> ValueOption.map(Some)
                |> ValueOption.defaultValue None

            let attributes =
                Widgets.tryGetScalarValue widget Pattern.MultipleAttributes
                |> ValueOption.map(fun x -> Some(MultipleAttributeListNode.Create(x)))
                |> ValueOption.defaultValue None

            Pattern.Parameter(PatParameterNode(attributes, value, typeValue, Range.Zero)))

[<AutoOpen>]
module ParameterBuilders =
    type Ast with

        static member private BaseParameter(name: WidgetBuilder<Pattern>, pType: WidgetBuilder<Type> voption) =
            WidgetBuilder<Pattern>(
                Parameter.WidgetKey,
                AttributesBundle(
                    StackList.empty(),
                    [| match pType with
                       | ValueSome pType ->
                           Parameter.Value.WithValue(name.Compile())
                           Parameter.TypeVal.WithValue(pType.Compile())
                       | ValueNone -> Parameter.Value.WithValue(name.Compile()) |],
                    Array.empty
                )
            )

        /// <summary>
        /// Creates a typed parameter pattern with a Pattern widget and Type widget.
        /// </summary>
        /// <param name="name">The pattern for the parameter name.</param>
        /// <param name="pType">The type of the parameter.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         TypeDefn("Class", Constructor(ParameterPat(NamedPat("c"), Int()))) {
        ///             Member("this.Value", ConstantExpr(Int(0)))
        ///         }
        ///     }
        /// }
        /// </code>
        static member ParameterPat(name: WidgetBuilder<Pattern>, pType: WidgetBuilder<Type>) =
            Ast.BaseParameter(name, ValueSome(pType))

        /// <summary>
        /// Creates a typed parameter pattern with a Pattern widget and string type.
        /// </summary>
        /// <param name="name">The pattern for the parameter name.</param>
        /// <param name="pType">The type of the parameter as a string.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         TypeDefn("Class", Constructor(ParameterPat(NamedPat("c"), "int"))) {
        ///             Member("this.Value", ConstantExpr(Int(0)))
        ///         }
        ///     }
        /// }
        /// </code>
        static member ParameterPat(name: WidgetBuilder<Pattern>, pType: string) =
            Ast.ParameterPat(name, Ast.EscapeHatch(Type.Create(pType)))

        /// <summary>
        /// Creates a typed parameter pattern with a Constant widget and Type widget.
        /// </summary>
        /// <param name="name">The constant for the parameter name.</param>
        /// <param name="pType">The type of the parameter.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         TypeDefn("Class", Constructor(ParameterPat(Constant("c"), Int()))) {
        ///             Member("this.Value", ConstantExpr(Int(0)))
        ///         }
        ///     }
        /// }
        /// </code>
        static member ParameterPat(name: WidgetBuilder<Constant>, pType: WidgetBuilder<Type>) =
            Ast.ParameterPat(Ast.ConstantPat(name), pType)

        /// <summary>
        /// Creates a typed parameter pattern with a Constant widget and string type.
        /// </summary>
        /// <param name="name">The constant for the parameter name.</param>
        /// <param name="pType">The type of the parameter as a string.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         TypeDefn("Class", Constructor(ParameterPat(Constant("c"), "int"))) {
        ///             Member("this.Value", ConstantExpr(Int(0)))
        ///         }
        ///     }
        /// }
        /// </code>
        static member ParameterPat(name: WidgetBuilder<Constant>, pType: string) =
            Ast.ParameterPat(name, Ast.EscapeHatch(Type.Create(pType)))

        /// <summary>
        /// Creates a typed parameter pattern with a string name and Type widget.
        /// </summary>
        /// <param name="name">The name of the parameter as a string.</param>
        /// <param name="pType">The type of the parameter.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         TypeDefn("Class", Constructor(ParameterPat("c", Int()))) {
        ///             Member("this.Value", ConstantExpr(Int(0)))
        ///         }
        ///     }
        /// }
        /// </code>
        static member ParameterPat(name: string, pType: WidgetBuilder<Type>) =
            Ast.ParameterPat(Ast.ConstantPat(name), pType)

        /// <summary>
        /// Creates a typed parameter pattern with a string name and string type.
        /// </summary>
        /// <param name="name">The name of the parameter as a string.</param>
        /// <param name="pType">The type of the parameter as a string.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         TypeDefn("Class", Constructor(ParameterPat("c", "int"))) {
        ///             Member("this.Value", ConstantExpr(Int(0)))
        ///         }
        ///     }
        /// }
        /// </code>
        static member ParameterPat(name: string, pType: string) =
            Ast.ParameterPat(name, Ast.EscapeHatch(Type.Create(pType)))

        /// <summary>
        /// Creates an untyped parameter pattern with a Pattern widget.
        /// </summary>
        /// <param name="name">The pattern for the parameter name.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         TypeDefn("Class", Constructor(ParameterPat(NamedPat("c")))) {
        ///             Member("this.Value", ConstantExpr(Int(0)))
        ///         }
        ///     }
        /// }
        /// </code>
        static member ParameterPat(name: WidgetBuilder<Pattern>) = Ast.BaseParameter(name, ValueNone)

        /// <summary>
        /// Creates an untyped parameter pattern with a Constant widget.
        /// </summary>
        /// <param name="name">The constant for the parameter name.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         TypeDefn("Class", Constructor(ParameterPat(Constant("c")))) {
        ///             Member("this.Value", ConstantExpr(Int(0)))
        ///         }
        ///     }
        /// }
        /// </code>
        static member ParameterPat(name: WidgetBuilder<Constant>) = Ast.ParameterPat(Ast.ConstantPat(name))

        /// <summary>
        /// Creates an untyped parameter pattern with a string name.
        /// </summary>
        /// <param name="name">The name of the parameter as a string.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         TypeDefn("Class", Constructor(ParameterPat("c"))) {
        ///             Member("this.Value", ConstantExpr(Int(0)))
        ///         }
        ///     }
        /// }
        /// </code>
        static member ParameterPat(name: string) = Ast.ParameterPat(Ast.Constant(name))
