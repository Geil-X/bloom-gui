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

let menuItems =
    [ "New File", Msg.NewFile
      "Open", Msg.OpenFile
      "Save As", Msg.SaveAs ]