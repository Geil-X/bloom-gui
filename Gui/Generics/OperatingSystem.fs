namespace Gui.Generics

open System

type OS =
    | OSX
    | Windows
    | Linux

module OperatingSystem =
    let get =
        match int Environment.OSVersion.Platform with
        | 4
        | 128 -> Linux
        | 6 -> OSX
        | _ -> Windows
