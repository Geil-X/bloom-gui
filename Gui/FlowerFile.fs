module Gui.FlowerFile

open System
open Avalonia.Controls

open Gui.Views

type FileError = exn


// ---- File Dialogs ----

let dialogTitle = "Flower File"
let extensions = Seq.singleton "bloom"

let defaultDirectory =
    Environment.SpecialFolder.MyDocuments

let openFileDialog (window: Window) =
    Dialogs.openFileDialog dialogTitle extensions defaultDirectory window

let saveFileDialog (window: Window) =
    Dialogs.saveFileDialog dialogTitle extensions defaultDirectory window
