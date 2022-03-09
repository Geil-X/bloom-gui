module Gui.Menu.File

open Avalonia.Controls
open Avalonia.FuncUI.DSL
open Avalonia.FuncUI.Types
open System

open Extensions

type Msg =
    | NewFile
    | OpenFile
    | SaveAs

// ---- Data ----

let tabName = "File"

let fileDialogName = "Bloom File"

let menuItems =
    [ "New File", Msg.NewFile
      "Open", Msg.OpenFile
      "Save As", Msg.SaveAs ]

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
