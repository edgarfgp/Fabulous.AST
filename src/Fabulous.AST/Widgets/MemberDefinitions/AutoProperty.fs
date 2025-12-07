namespace Fabulous.AST

open System
open System.Runtime.CompilerServices
open Fabulous.AST
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module AutoPropertyMember =
    let XmlDocs = Attributes.defineWidget "XmlDocs"
    let Identifier = Attributes.defineScalar<string> "Identifier"
    let ReturnType = Attributes.defineWidget "Type"
    let Parameters = Attributes.defineScalar<MethodParamsType> "Parameters"
    let HasGetter = Attributes.defineScalar<bool * AccessControl> "HasGetter"
    let HasSetter = Attributes.defineScalar<bool * AccessControl> "HasSetter"

    let IsStatic = Attributes.defineScalar<bool> "IsStatic"
    let Accessibility = Attributes.defineScalar<AccessControl> "Accessibility"
    let BodyExpr = Attributes.defineWidget "BodyExpr"

    let WidgetKey =
        Widgets.register "AutoPropertyMember" (fun widget ->
            let identifier = Widgets.getScalarValue widget Identifier
            let hasGetter = Widgets.getScalarValue widget HasGetter
            let hasSetter = Widgets.getScalarValue widget HasSetter

            let accessControl =
                Widgets.tryGetScalarValue widget MemberDefn.Accessibility
                |> ValueOption.defaultValue AccessControl.Unknown

            let accessControl =
                match accessControl with
                | Public -> Some(SingleTextNode.``public``)
                | Private -> Some(SingleTextNode.``private``)
                | Internal -> Some(SingleTextNode.``internal``)
                | Unknown -> None

            let attributes =
                Widgets.tryGetScalarValue widget MemberDefn.MultipleAttributes
                |> ValueOption.map(fun x -> Some(MultipleAttributeListNode.Create(x)))
                |> ValueOption.defaultValue None

            let isStatic =
                Widgets.tryGetScalarValue widget BindingNode.IsStatic
                |> ValueOption.defaultValue false

            let returnType = Widgets.tryGetNodeFromWidget widget ReturnType

            let bodyExpr = Widgets.getNodeFromWidget widget BodyExpr

            let returnType =
                match returnType with
                | ValueSome tp -> Some tp
                | ValueNone -> None

            let xmlDocs =
                Widgets.tryGetNodeFromWidget widget MemberDefn.XmlDocs
                |> ValueOption.map(Some)
                |> ValueOption.defaultValue None

            let multipleTextsNode =
                MultipleTextsNode(
                    [ if isStatic then
                          SingleTextNode.``static``
                          SingleTextNode.``member``
                          SingleTextNode.``val``
                      else
                          SingleTextNode.``member``
                          SingleTextNode.``val`` ],
                    Range.Zero
                )

            let withGetSetText =
                match hasGetter, hasSetter with
                | (true, getterAccessibility), (true, setterAccessibility) ->
                    Some(
                        MultipleTextsNode.Create(
                            [ SingleTextNode.``with``
                              // Getter
                              match getterAccessibility with
                              | Public -> SingleTextNode.``public``
                              | Private -> SingleTextNode.``private``
                              | Internal -> SingleTextNode.``internal``
                              | Unknown -> ()
                              SingleTextNode.Create("get,")
                              // Setter
                              match setterAccessibility with
                              | Public -> SingleTextNode.``public``
                              | Private -> SingleTextNode.``private``
                              | Internal -> SingleTextNode.``internal``
                              | Unknown -> ()
                              SingleTextNode.set ]
                        )
                    )
                | (true, getterAccessibility), (false, _) ->
                    Some(
                        MultipleTextsNode.Create(
                            [ SingleTextNode.``with``
                              match getterAccessibility with
                              | Public -> SingleTextNode.``public``
                              | Private -> SingleTextNode.``private``
                              | Internal -> SingleTextNode.``internal``
                              | Unknown -> ()
                              SingleTextNode.get ]
                        )
                    )
                | (false, _), (true, setterAccessibility) ->
                    Some(
                        MultipleTextsNode.Create(
                            [ SingleTextNode.``with``
                              match setterAccessibility with
                              | Public -> SingleTextNode.``public``
                              | Private -> SingleTextNode.``private``
                              | Internal -> SingleTextNode.``internal``
                              | Unknown -> ()
                              SingleTextNode.set ]
                        )
                    )
                | (false, _), (false, _) -> None

            let node =
                MemberDefnAutoPropertyNode(
                    xmlDocs,
                    attributes,
                    multipleTextsNode,
                    accessControl,
                    SingleTextNode.Create(identifier),
                    returnType,
                    SingleTextNode.equals,
                    bodyExpr,
                    withGetSetText,
                    Range.Zero
                )

            MemberDefn.AutoProperty(node))

[<AutoOpen>]
module AutoPropertyMemberBuilders =
    type Ast with
        static member private BaseMemberVal
            (
                identifier: string,
                expr: WidgetBuilder<Expr>,
                ?hasGetter: bool,
                ?hasSetter: bool,
                ?getterAccessibility: AccessControl,
                ?setterAccessibility: AccessControl,
                ?returnType: WidgetBuilder<Type>
            ) =
            let hasGetter = defaultArg hasGetter false
            let hasSetter = defaultArg hasSetter false
            let getterAccessibility = defaultArg getterAccessibility AccessControl.Unknown
            let setterAccessibility = defaultArg setterAccessibility AccessControl.Unknown

            WidgetBuilder<MemberDefn>(
                AutoPropertyMember.WidgetKey,
                AttributesBundle(
                    StackList.three(
                        AutoPropertyMember.Identifier.WithValue(identifier),
                        AutoPropertyMember.HasGetter.WithValue(hasGetter, getterAccessibility),
                        AutoPropertyMember.HasSetter.WithValue(hasSetter, setterAccessibility)
                    ),
                    [| AutoPropertyMember.BodyExpr.WithValue(expr.Compile())
                       match returnType with
                       | None -> ()
                       | Some returnType -> AutoPropertyMember.ReturnType.WithValue(returnType.Compile()) |],
                    Array.empty
                )
            )

        /// <summary>
        /// Create an auto property member definition.
        /// </summary>
        /// <param name="identifier">The identifier of the member.</param>
        /// <param name="expr">The expression of the member.</param>
        /// <param name="hasGetter">Whether the member has a getter.</param>
        /// <param name="hasSetter">Whether the member has a setter.</param>
        /// <param name="getterAccessibility">The accessibility of the getter.</param>
        /// <param name="setterAccessibility">The accessibility of the setter.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         TypeDefn("Person", UnitPat()) {
        ///             MemberVal("Name", ConstantExpr(Constant("name")), true, true)
        ///             MemberVal("Age", ConstantExpr(Constant("age")), true, true)
        ///         }
        ///     }
        /// }
        /// </code>
        static member MemberVal
            (
                identifier: string,
                expr: WidgetBuilder<Expr>,
                ?hasGetter: bool,
                ?hasSetter: bool,
                ?getterAccessibility: AccessControl,
                ?setterAccessibility: AccessControl
            ) =
            let hasGetter = defaultArg hasGetter false
            let hasSetter = defaultArg hasSetter false
            let getterAccessibility = defaultArg getterAccessibility AccessControl.Unknown
            let setterAccessibility = defaultArg setterAccessibility AccessControl.Unknown
            Ast.BaseMemberVal(identifier, expr, hasGetter, hasSetter, getterAccessibility, setterAccessibility)

        /// <summary>
        /// Create an auto property member definition.
        /// </summary>
        /// <param name="identifier">The identifier of the member.</param>
        /// <param name="expr">The expression of the member.</param>
        /// <param name="returnType">The return type of the member.</param>
        /// <param name="hasGetter">Whether the member has a getter.</param>
        /// <param name="hasSetter">Whether the member has a setter.</param>
        /// <param name="getterAccessibility">The accessibility of the getter.</param>
        /// <param name="setterAccessibility">The accessibility of the setter.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         TypeDefn("Person", UnitPat()) {
        ///             MemberVal("Name", ConstantExpr(Constant("name")), String(), true, true)
        ///         }
        ///     }
        /// }
        /// </code>
        static member MemberVal
            (
                identifier: string,
                expr: WidgetBuilder<Expr>,
                returnType: WidgetBuilder<Type>,
                ?hasGetter: bool,
                ?hasSetter: bool,
                ?getterAccessibility: AccessControl,
                ?setterAccessibility: AccessControl
            ) =
            let hasGetter = defaultArg hasGetter false
            let hasSetter = defaultArg hasSetter false
            let getterAccessibility = defaultArg getterAccessibility AccessControl.Unknown
            let setterAccessibility = defaultArg setterAccessibility AccessControl.Unknown

            Ast.BaseMemberVal(
                identifier,
                expr,
                hasGetter,
                hasSetter,
                getterAccessibility,
                setterAccessibility,
                returnType
            )

        /// <summary>
        /// Create an auto property member definition.
        /// </summary>
        /// <param name="identifier">The identifier of the member.</param>
        /// <param name="expr">The expression of the member.</param>
        /// <param name="returnType">The return type of the member.</param>
        /// <param name="hasGetter">Whether the member has a getter.</param>
        /// <param name="hasSetter">Whether the member has a setter.</param>
        /// <param name="getterAccessibility">The accessibility of the getter.</param>
        /// <param name="setterAccessibility">The accessibility of the setter.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         TypeDefn("Person", UnitPat()) {
        ///             MemberVal("Name", ConstantExpr(Constant("name")), "string", true, true)
        ///         }
        ///     }
        /// }
        /// </code>
        static member MemberVal
            (
                identifier: string,
                expr: WidgetBuilder<Expr>,
                returnType: string,
                ?hasGetter: bool,
                ?hasSetter: bool,
                ?getterAccessibility: AccessControl,
                ?setterAccessibility: AccessControl
            ) =
            let hasGetter = defaultArg hasGetter false
            let hasSetter = defaultArg hasSetter false
            let getterAccessibility = defaultArg getterAccessibility AccessControl.Unknown
            let setterAccessibility = defaultArg setterAccessibility AccessControl.Unknown

            Ast.BaseMemberVal(
                identifier,
                expr,
                hasGetter,
                hasSetter,
                getterAccessibility,
                setterAccessibility,
                Ast.LongIdent(returnType)
            )

        /// <summary>Create an auto property member definition.</summary>
        /// <param name="identifier">The identifier of the member.</param>
        /// <param name="expr">The expression of the member.</param>
        /// <param name="hasGetter">Whether the member has a getter.</param>
        /// <param name="hasSetter">Whether the member has a setter.</param>
        /// <param name="getterAccessibility">The accessibility of the getter.</param>
        /// <param name="setterAccessibility">The accessibility of the setter.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         TypeDefn("Person", UnitPat()) {
        ///             MemberVal("Name", Constant("name"), true, true)
        ///             MemberVal("Age", Constant("age"), true, true)
        ///         }
        ///     }
        /// }
        /// </code>
        static member MemberVal
            (
                identifier: string,
                expr: WidgetBuilder<Constant>,
                ?hasGetter: bool,
                ?hasSetter: bool,
                ?getterAccessibility: AccessControl,
                ?setterAccessibility: AccessControl
            ) =
            let hasGetter = defaultArg hasGetter false
            let hasSetter = defaultArg hasSetter false
            let getterAccessibility = defaultArg getterAccessibility AccessControl.Unknown
            let setterAccessibility = defaultArg setterAccessibility AccessControl.Unknown

            Ast.MemberVal(
                identifier,
                Ast.ConstantExpr(expr),
                hasGetter,
                hasSetter,
                getterAccessibility,
                setterAccessibility
            )

        /// <summary>
        /// Create an auto property member definition.
        /// </summary>
        /// <param name="identifier">The identifier of the member.</param>
        /// <param name="expr">The expression of the member.</param>
        /// <param name="returnType">The return type of the member.</param>
        /// <param name="hasGetter">Whether the member has a getter.</param>
        /// <param name="hasSetter">Whether the member has a setter.</param>
        /// <param name="getterAccessibility">The accessibility of the getter.</param>
        /// <param name="setterAccessibility">The accessibility of the setter.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         TypeDefn("Person", UnitPat()) {
        ///             MemberVal("Name", Constant("name"), String(), true, true)
        ///         }
        ///     }
        /// }
        /// </code>
        static member MemberVal
            (
                identifier: string,
                expr: WidgetBuilder<Constant>,
                returnType: WidgetBuilder<Type>,
                ?hasGetter: bool,
                ?hasSetter: bool,
                ?getterAccessibility: AccessControl,
                ?setterAccessibility: AccessControl
            ) =
            let hasGetter = defaultArg hasGetter false
            let hasSetter = defaultArg hasSetter false
            let getterAccessibility = defaultArg getterAccessibility AccessControl.Unknown
            let setterAccessibility = defaultArg setterAccessibility AccessControl.Unknown

            Ast.BaseMemberVal(
                identifier,
                Ast.ConstantExpr(expr),
                hasGetter,
                hasSetter,
                getterAccessibility,
                setterAccessibility,
                returnType
            )

        /// <summary>
        /// Create an auto property member definition.
        /// </summary>
        /// <param name="identifier">The identifier of the member.</param>
        /// <param name="expr">The expression of the member.</param>
        /// <param name="returnType">The return type of the member.</param>
        /// <param name="hasGetter">Whether the member has a getter.</param>
        /// <param name="hasSetter">Whether the member has a setter.</param>
        /// <param name="getterAccessibility">The accessibility of the getter.</param>
        /// <param name="setterAccessibility">The accessibility of the setter.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         TypeDefn("Person", UnitPat()) {
        ///             MemberVal("Name", Constant("name"), "string", true, true)
        ///         }
        ///     }
        /// }
        /// </code>
        static member MemberVal
            (
                identifier: string,
                expr: WidgetBuilder<Constant>,
                returnType: string,
                ?hasGetter: bool,
                ?hasSetter: bool,
                ?getterAccessibility: AccessControl,
                ?setterAccessibility: AccessControl
            ) =
            let hasGetter = defaultArg hasGetter false
            let hasSetter = defaultArg hasSetter false
            let getterAccessibility = defaultArg getterAccessibility AccessControl.Unknown
            let setterAccessibility = defaultArg setterAccessibility AccessControl.Unknown

            Ast.BaseMemberVal(
                identifier,
                Ast.ConstantExpr(expr),
                hasGetter,
                hasSetter,
                getterAccessibility,
                setterAccessibility,
                Ast.LongIdent(returnType)
            )

        /// <summary>Create an auto property member definition.</summary>
        /// <param name="identifier">The identifier of the member.</param>
        /// <param name="expr">The expression of the member.</param>
        /// <param name="hasGetter">Whether the member has a getter.</param>
        /// <param name="hasSetter">Whether the member has a setter.</param>
        /// <param name="getterAccessibility">The accessibility of the getter.</param>
        /// <param name="setterAccessibility">The accessibility of the setter.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         TypeDefn("Person", UnitPat()) {
        ///             MemberVal("Name", "name", true, true)
        ///             MemberVal("Age", "age", true, true)
        ///        }
        ///     }
        /// }
        /// </code>
        static member MemberVal
            (
                identifier: string,
                expr: string,
                ?hasGetter: bool,
                ?hasSetter: bool,
                ?getterAccessibility: AccessControl,
                ?setterAccessibility: AccessControl
            ) =
            let hasGetter = defaultArg hasGetter false
            let hasSetter = defaultArg hasSetter false
            let getterAccessibility = defaultArg getterAccessibility AccessControl.Unknown
            let setterAccessibility = defaultArg setterAccessibility AccessControl.Unknown

            Ast.MemberVal(
                identifier,
                Ast.Constant(expr),
                hasGetter,
                hasSetter,
                getterAccessibility,
                setterAccessibility
            )

        /// <summary>
        /// Create an auto property member definition.
        /// </summary>
        /// <param name="identifier">The identifier of the member.</param>
        /// <param name="expr">The expression of the member.</param>
        /// <param name="returnType">The return type of the member.</param>
        /// <param name="hasGetter">Whether the member has a getter.</param>
        /// <param name="hasSetter">Whether the member has a setter.</param>
        /// <param name="getterAccessibility">The accessibility of the getter.</param>
        /// <param name="setterAccessibility">The accessibility of the setter.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         TypeDefn("Person", UnitPat()) {
        ///             MemberVal("Name", "name", String(), true, true)
        ///         }
        ///     }
        /// }
        /// </code>
        static member MemberVal
            (
                identifier: string,
                expr: string,
                returnType: WidgetBuilder<Type>,
                ?hasGetter: bool,
                ?hasSetter: bool,
                ?getterAccessibility: AccessControl,
                ?setterAccessibility: AccessControl
            ) =
            let hasGetter = defaultArg hasGetter false
            let hasSetter = defaultArg hasSetter false
            let getterAccessibility = defaultArg getterAccessibility AccessControl.Unknown
            let setterAccessibility = defaultArg setterAccessibility AccessControl.Unknown

            Ast.BaseMemberVal(
                identifier,
                Ast.ConstantExpr(expr),
                hasGetter,
                hasSetter,
                getterAccessibility,
                setterAccessibility,
                returnType
            )

        /// <summary>
        /// Create an auto property member definition.
        /// </summary>
        /// <param name="identifier">The identifier of the member.</param>
        /// <param name="expr">The expression of the member.</param>
        /// <param name="returnType">The return type of the member.</param>
        /// <param name="hasGetter">Whether the member has a getter.</param>
        /// <param name="hasSetter">Whether the member has a setter.</param>
        /// <param name="getterAccessibility">The accessibility of the getter.</param>
        /// <param name="setterAccessibility">The accessibility of the setter.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         TypeDefn("Person", UnitPat()) {
        ///             MemberVal("Name", "name", "string", true, true)
        ///         }
        ///     }
        /// }
        /// </code>
        static member MemberVal
            (
                identifier: string,
                expr: string,
                returnType: string,
                ?hasGetter: bool,
                ?hasSetter: bool,
                ?getterAccessibility: AccessControl,
                ?setterAccessibility: AccessControl
            ) =
            let hasGetter = defaultArg hasGetter false
            let hasSetter = defaultArg hasSetter false
            let getterAccessibility = defaultArg getterAccessibility AccessControl.Unknown
            let setterAccessibility = defaultArg setterAccessibility AccessControl.Unknown

            Ast.BaseMemberVal(
                identifier,
                Ast.ConstantExpr(expr),
                hasGetter,
                hasSetter,
                getterAccessibility,
                setterAccessibility,
                Ast.LongIdent(returnType)
            )
