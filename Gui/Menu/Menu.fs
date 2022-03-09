module Gui.Menu.Menu

open Avalonia.Controls
open Avalonia.FuncUI.Types
open Avalonia.FuncUI.DSL
open Elmish

type Msg =
    | FileMsg of File.Msg

let view (dispatch: Msg -> unit) =
    let menuItems : IView list =
        [ File.view (FileMsg >> dispatch) ]

    Menu.create [ Menu.viewItems menuItems ]
