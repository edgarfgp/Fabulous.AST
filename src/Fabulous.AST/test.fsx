type MyInterface =
    abstract member Add: int -> int -> int
    abstract member Pi: float
    abstract member Area: float with get, set

type MyClass() =
    interface MyInterface with
        member this.Add (a) (b) = a + b
        member this.Area = 10.

        member this.Area
            with set value = ()

        member this.Pi = 3.14

// let myInterface =
//     Interface("MyInterface") {
//         EscapeHatch(method)
//         EscapeHatch(property)
//         EscapeHatch(getSetProperty)
//     }
//
// Class("MyClass")
//     .impelements(myInterface)
