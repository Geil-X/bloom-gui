module Gui.Menu.File

open Avalonia.Controls
open Avalonia.FuncUI.DSL
open Avalonia.FuncUI.Types
open Elmish
open System

open Gui
open Gui.Widgets
open Extensions

[<RequireQualifiedAccess>]
type External =
    | NewFile
    | FileLoaded of Result<Flower.State seq, FlowerFile.FileError>
    | SavedFile
    | ErrorSavingFile of FlowerFile.FileError
    | DoNothing

type Msg =
    | NewFile
    | OpenFile
    | LoadFiles of string array
    | LoadFile of string
    | SaveAs
    | SaveFileToPath of string

// ---- Data ----

let tabName = "File"

let fileDialogName = "Bloom File"

let menuItems =
    [ "New File", Msg.NewFile
      "Open", Msg.OpenFile
      "Save As", Msg.SaveAs ]

// ---- Update ----

let update msg flowers window : Cmd<Msg> * External =
    match msg with
    | NewFile ->
        Log.debug "Create a new file"
        Cmd.none, External.NewFile

    | OpenFile ->
        Log.debug "Open a file"
        
        let fileDialogTask =
            FlowerFile.openFileDialog
            
        Cmd.OfAsync.perform fileDialogTask window LoadFiles, External.DoNothing
        

    | LoadFiles paths ->
        match Array.tryHead paths with
        | Some path -> Cmd.ofMsg (LoadFile path), External.DoNothing
        | None -> Cmd.none, External.DoNothing

    | LoadFile path ->
        Log.debug $"Load {path}"
        // Todo: Make this an asynchronous command
        Cmd.none, External.FileLoaded(FlowerFile.loadFlowerFile path)

    | SaveAs ->
        Log.debug "Save As"

        // Todo: save file task
//        let fileDialogTask =
//            Dialogs.saveFileDialogTask "Fold File" [ Flower.extension ] window
//
//        Cmd.OfAsync.perform fileDialogTask () SaveFileToPath, External.DoNothing
        Cmd.none, External.DoNothing

    | SaveFileToPath path ->
        Log.debug $"Save {path}"

        match FlowerFile.writeFlowerFile path flowers with
        | None -> Cmd.none, External.SavedFile
        | Some error -> Cmd.none, External.ErrorSavingFile error

let view (dispatch: Msg -> unit) =
    let menuOption (name: String) msg : IView =
        MenuItem.create
        <| [ MenuItem.header name
             MenuItem.onClick (fun _ -> dispatch msg) ]
        :> IView

    let menuOptions: IView list =
        List.map (Tuple2.map menuOption) menuItems

    MenuItem.create
    <| [ MenuItem.header tabName
         MenuItem.viewItems menuOptions ]
