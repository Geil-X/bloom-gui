module Gui.Panels.IconDock

open Avalonia.Controls
open Avalonia.Layout
open Avalonia.FuncUI.DSL
open Avalonia.FuncUI.Types

open Extensions
open Gui
open Gui.Widgets


type Msg =
    | NewFile
    | SaveAs
    | Open
    | NewFlower

let private iconButtons =
    [ Icons.newFile Theme.colors.offWhite, NewFile
      Icons.save Theme.colors.offWhite, SaveAs
      Icons.load Theme.colors.offWhite, Open
      Icons.newIcon Theme.colors.offWhite, NewFlower ]

let view (dispatch: Msg -> unit) =
    let button (icon, msg) =
        Form.imageButton icon (Event.handleEvent msg >> dispatch) :> IView

    let buttons: IView list = List.map button iconButtons

    StackPanel.create [
        StackPanel.orientation Orientation.Horizontal
        StackPanel.children buttons
    ]
