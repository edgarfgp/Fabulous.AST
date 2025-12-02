namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fabulous.AST
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module Parameter =
    let Value = Attributes.defineWidget "Value"
    let TypeVal = Attributes.defineWidget "Type"

    let MultipleAttributes =
        Attributes.defineScalar<AttributeNode seq> "MultipleAttributes"

    let WidgetKey =
        Widgets.register "Parameter" (fun widget ->
            let value = Widgets.getNodeFromWidget<Pattern> widget Value

            let typeValue =
                Widgets.tryGetNodeFromWidget widget TypeVal
                |> ValueOption.map(Some)
                |> ValueOption.defaultValue None

            let attributes =
                Widgets.tryGetScalarValue widget MultipleAttributes
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

type ParameterModifiers =
    /// <summary>
    /// Adds multiple attributes to a parameter pattern.
    /// </summary>
    /// <param name="this">The parameter pattern widget.</param>
    /// <param name="attributes">The sequence of attributes to add.</param>
    /// <code language="fsharp">
    /// Oak() {
    ///     AnonymousModule() {
    ///         TypeDefn(
    ///             "Class",
    ///             Constructor(ParameterPat("c", Int()).attributes([ Attribute("Obsolete"); Attribute("Required") ]))
    ///         ) {
    ///             Member("this.Value", ConstantExpr(Int(0)))
    ///         }
    ///     }
    /// }
    /// </code>
    [<Extension>]
    static member inline attributes(this: WidgetBuilder<Pattern>, attributes: WidgetBuilder<AttributeNode> seq) =
        this.AddScalar(
            Parameter.MultipleAttributes.WithValue(
                [ for attr in attributes do
                      Gen.mkOak attr ]
            )
        )

    /// <summary>
    /// Adds an attribute to a parameter pattern.
    /// </summary>
    /// <param name="this">The parameter pattern widget.</param>
    /// <param name="attribute">The attribute to add.</param>
    /// <code language="fsharp">
    /// Oak() {
    ///     AnonymousModule() {
    ///         TypeDefn("Class", Constructor(ParameterPat("c", Int()).attribute(Attribute("Obsolete")))) {
    ///             Member(
    ///                 "this.First",
    ///                 ParenPat(ParameterPat("a", String()).attribute(Attribute("Obsolete"))),
    ///                 UnitExpr()
    ///             )
    ///         }
    ///     }
    /// }
    /// </code>
    [<Extension>]
    static member inline attribute(this: WidgetBuilder<Pattern>, attribute: WidgetBuilder<AttributeNode>) =
        ParameterModifiers.attributes(this, [ attribute ])
