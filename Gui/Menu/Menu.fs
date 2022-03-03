module Gui.Menu.Menu

open Avalonia.Controls
open Avalonia.FuncUI.Types
open Avalonia.FuncUI.DSL
open Elmish

type External =
    | FileExternal of File.External

type Msg =
    | FileMsg of File.Msg

let update (msg: Msg) flowers window: Cmd<Msg> * External =
    match msg  with
    | FileMsg fileMsg ->
        let fileCmd, fileExternal = File.update fileMsg flowers window
        Cmd.map FileMsg fileCmd, FileExternal fileExternal

let view (dispatch: Msg -> unit) =
    let menuItems : IView list =
        [ File.view (FileMsg >> dispatch) ]

    Menu.create [ Menu.viewItems menuItems ]
