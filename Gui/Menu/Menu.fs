module Gui.Menu.Menu

open Avalonia.Controls
open Avalonia.FuncUI.Hosts
open Elmish

type Msg = FileMsg of File.Msg

let menuItem name = NativeMenuItem name

let fileMenuItems: NativeMenuItem list =
    File.menuItems
    |> List.map fst
    |> List.map menuItem

let fileMenu: NativeMenuItem =
    let menuItem = NativeMenuItem File.tabName
    let menu = NativeMenu()
    menuItem.Menu <- menu
    
    List.iter menu.Add fileMenuItems

    menuItem

let nativeMenu: NativeMenu =
    let menu = NativeMenu()
    menu.Add fileMenu

    menu

let subscription (conv: Msg -> 'Msg) (_: 'State) =
    let sub (dispatch: 'Msg -> unit) =
        let fileMsgs = List.map snd File.menuItems

        List.iter
            (fun (menuItem: NativeMenuItem, msg) -> menuItem.Clicked.Add(fun _ -> FileMsg msg |> conv |> dispatch))
            (List.zip fileMenuItems fileMsgs)

    Cmd.ofSub sub

let setMenu (window: HostWindow) = NativeMenu.SetMenu(window, nativeMenu)
