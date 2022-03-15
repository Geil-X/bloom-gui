namespace Gui

open Gui.DataTypes

type Action =
    // File Actions
    | NewFile
    | SaveAsDialog
    | SaveAs of string
    | OpenFileDialog
    | OpenFile of string
    | FileOpened of Flower seq

    // Flower Actions
    | NewFlower
    | SelectFlower of Flower Id
    | DeselectFlower
    | DeleteFlower

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
