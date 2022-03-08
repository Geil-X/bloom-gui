module Tests.Test

open Avalonia.Media

let equal expected actual : bool =
    if expected = actual then
        true
    else
        printfn $"Expected: {expected}"
        printfn $" But Was: {actual}\n"
        false

let colorEqualWithin tolerance (expected: Color) (actual: Color) : bool =
    let equalWithin a b = max a b - min a b <= tolerance

    if equalWithin expected.R actual.R
       && equalWithin expected.G actual.G
       && equalWithin expected.B actual.B
       && equalWithin expected.A actual.A then
        true
    else
        printfn $"Expected: {expected}"
        printfn $" But Was: {actual}\n"
        false
