module Gui.FlowerFile

open System
open System.IO
open System.Threading.Tasks
open Avalonia.Controls

open Gui.DataTypes
open Gui.Widgets
open Extensions

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


// ---- Read & Write ----

// Note: Can throw an exception
let private readFile path (deserializer: StreamReader -> 'T) : Task<'T> =
    task {
        use reader = new StreamReader(File.OpenRead(path))
        return deserializer reader
    }

/// TODO: catch json deserialization errors
// Note: Can throw an exception
let loadFlowerFile path : Task<Flower seq> = readFile path Flower.deserialize

// Note: Can throw an exception
let private writeFile path (serializer: StreamWriter -> 'T -> unit) (data: 'T) : Task<unit> =
    task {
        use writer = new StreamWriter(File.OpenWrite(path))
        serializer writer data
    }

/// Note: Can throw an exception
let writeFlowerFile (path, flowers: Flower seq) : Task<unit> = writeFile path Flower.serialize flowers
