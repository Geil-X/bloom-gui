module Tests.Color

open Avalonia.Media
open NUnit.Framework
open FsCheck.NUnit



[<SetUp>]
let Setup () = Gen.ArbGui.Register()

[<Property>]
let ``Can convert from hsla back to color`` (color: Color) =
    let converted =
        color |> Color.toHsla |> Color.withHsla

    Test.colorEqualWithin 1uy color converted
