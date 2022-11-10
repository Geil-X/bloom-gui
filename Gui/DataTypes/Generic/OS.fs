namespace Gui.DataTypes

[<RequireQualifiedAccess>]
type OS =
    | OSX
    | Windows
    | Linux
    | Raspbian

module OS =

    open System
    open System.Reflection

    let private imageFileMachine =
        let a = Assembly.GetExecutingAssembly()

        let mutable peKind: PortableExecutableKinds =
            PortableExecutableKinds.NotAPortableExecutableImage

        let mutable machine: ImageFileMachine =
            ImageFileMachine.IA64

        a.ManifestModule.GetPEKind(&peKind, &machine)

        machine

    let getOS =
        match Environment.OSVersion.Platform with
        | PlatformID.Other
        | PlatformID.Unix ->
            match imageFileMachine with
            | ImageFileMachine.I386
            | ImageFileMachine.AMD64
            | ImageFileMachine.IA64 -> OS.Linux
            | ImageFileMachine.ARM -> OS.Raspbian
            | arch -> failwith $"{arch}: architecture is not supported"

        | PlatformID.Win32NT
        | PlatformID.Win32S
        | PlatformID.Win32Windows
        | PlatformID.WinCE -> OS.Windows

        | PlatformID.MacOSX -> OS.OSX

        | platform -> failwith $"{platform}: platform is not supported"
