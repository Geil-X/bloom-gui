module Gui.FlowerFile

open System
open System.IO
open System.Threading.Tasks
open Avalonia.Controls

open Elmish
open Gui.DataTypes
open Gui.Views
open Extensions
open Gui.Generics

type FileError = exn


// ---- File Dialogs ----

let dialogTitle = "Flower File"
let extensions = Seq.singleton "bloom"
let defaultDirectory = Environment.SpecialFolder.MyDocuments

let openFileDialog (window: Window) =
    Dialogs.openFileDialog dialogTitle extensions defaultDirectory window
    |> Task.map Array.tryHead

let saveFileDialog (window: Window) =
    Dialogs.saveFileDialog dialogTitle extensions defaultDirectory window