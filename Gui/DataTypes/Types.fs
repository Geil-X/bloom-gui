namespace Gui

open Gui.DataTypes

[<RequireQualifiedAccess>]
type Action =
    | NewFile
    | SaveAsDialog
    | SaveAs of string
    | OpenFileDialog
    | OpenFile of string
    | FileOpened of Flower seq
    | NewFlower

[<RequireQualifiedAccess>]
type ActionError =
    | ErrorSavingFile of exn
    | ErrorPickingSaveFile
    | ErrorPickingFileToOpen
    | CouldNotOpenFile of exn

[<RequireQualifiedAccess>]
type Direction =
    | Left
    | Top
    | Right
    | Bottom
