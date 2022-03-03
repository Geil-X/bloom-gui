module Gui.FlowerFile

open System
open System.IO
open Avalonia.Controls

open Gui.Widgets

type FileError = exn


// ---- File Dialogs ----

let dialogTitle = "Flower File"
let extensions = Seq.singleton Flower.extension
let defaultDirectory = Environment.SpecialFolder.MyDocuments

let openFileDialog (window: Window) : Async<string []> =
    let dialog =
        Dialogs.openFileDialog dialogTitle extensions defaultDirectory

    dialog.ShowAsync(window) |> Async.AwaitTask

let saveFileDialog (window: Window) : Async<string> =
    let dialog =
        Dialogs.saveFileDialog dialogTitle extensions defaultDirectory

    dialog.ShowAsync(window) |> Async.AwaitTask


// ---- Read & Write ----

let private readFile path (deserializer: StreamReader -> 'T) : Result<'T, FileError> =
    try
        use reader = new StreamReader(File.OpenRead(path))
        deserializer reader |> Ok
    with
    | exn -> Error exn

/// TODO: catch json deserialization errors
let loadFlowerFile path : Result<Flower.State seq, FileError> = readFile path Flower.deserialize

let private writeFile path (serializer: StreamWriter -> 'T -> Unit) (data: 'T) : FileError option =
    try
        let writer = new StreamWriter(File.OpenWrite(path))
        serializer writer data

        None
    with
    | exn -> Some exn

let writeFlowerFile path (flowers: Flower.State seq) : FileError option = writeFile path Flower.serialize flowers
