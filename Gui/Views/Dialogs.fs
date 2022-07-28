module Gui.Views.Dialogs

open System
open System.IO
open System.Threading.Tasks
open Avalonia.Controls
open Avalonia.Threading

open Extensions

/// Open a dialog to pick a folder on the users computer
let openFolderDialog (title: string) (directory: Environment.SpecialFolder) (window: Window) : Task<DirectoryInfo> =
    let dialog = OpenFolderDialog()

    dialog.Title <- $"Open Folder {title}"
    dialog.Directory <- Environment.GetFolderPath(directory)

    Dispatcher.UIThread.InvokeAsync<string>(fun () -> dialog.ShowAsync(window))
    |> Task.map DirectoryInfo

/// Open up a file dialog for selecting files of a particular type.
let openFileDialog
    (title: string)
    (extensions: string seq)
    (directory: Environment.SpecialFolder)
    (window: Window)
    : Task<FileInfo seq> =
    let dialog = OpenFileDialog()

    let filters =
        let filter = FileDialogFilter()
        filter.Extensions <- Collections.Generic.List(extensions)
        filter.Name <- title
        Collections.Generic.List(seq { filter })

    dialog.Title <- $"Open {title}"
    dialog.Filters <- filters
    dialog.AllowMultiple <- false
    dialog.Directory <- Environment.GetFolderPath(directory)

    Dispatcher.UIThread.InvokeAsync<string []>(fun () -> dialog.ShowAsync(window))
    |> Task.map (Seq.map FileInfo)

/// Open up a file dialog for saving a file type.
let saveFileDialog
    (title: string)
    (extensions: string seq)
    (directory: Environment.SpecialFolder)
    (window: Window)
    : Task<FileInfo> =
    let dialog = SaveFileDialog()

    let filters =
        let filter = FileDialogFilter()
        filter.Extensions <- Collections.Generic.List(extensions)
        filter.Name <- title
        Collections.Generic.List(seq { filter })

    let defaultExtension =
        Seq.tryHead extensions
        |> Option.defaultValue "txt"

    dialog.Title <- $"Save {title}"
    dialog.InitialFileName <- $"{title}.{defaultExtension}"
    dialog.Filters <- filters
    dialog.Directory <- Environment.GetFolderPath(directory)

    Dispatcher.UIThread.InvokeAsync<string>(fun () -> dialog.ShowAsync(window))
    |> Task.map FileInfo
