module Gui.Menu.ApplicationMenu

open Avalonia.Controls
open Avalonia.FuncUI.DSL
open Avalonia.FuncUI.Hosts
open Avalonia.FuncUI.Types
open Elmish

open Gui

let menuActionView (item: MenuAction<'Msg>) (dispatch: 'Msg -> unit) : IView =
    MenuItem.create [
        MenuItem.header item.Name
        MenuItem.onClick (fun _ -> dispatch item.Msg)
    ]

let menuDropdownView (item: MenuDropdown<'Msg>) (dispatch: 'Msg -> unit) : IView =
    MenuItem.create [
        MenuItem.header item.Name
        MenuItem.viewItems (List.map (fun action -> menuActionView action dispatch) item.Actions)
    ]

let menuItemView (menuItem: MenuItem<'Msg>) (dispatch: 'Msg -> unit) : IView =
    match menuItem with
    | MenuItem.Action action -> menuActionView action dispatch
    | MenuItem.Dropdown dropdown -> menuDropdownView dropdown dispatch

let menuTab (menuTab: MenuTab<'Msg>) (dispatch: 'Msg -> unit) : IView =
    let menuItemHelper item = menuItemView item dispatch

    let menuItems =
        List.map menuItemHelper menuTab.Items

    MenuItem.create [
        MenuItem.header menuTab.Name
        MenuItem.viewItems menuItems
    ]

let view (menuBar: MenuBar<'Msg>) (dispatch: 'Msg -> unit) : IView<Menu> =
    let menuTabHelper item = menuTab item dispatch
    let tabs = List.map menuTabHelper menuBar

    Menu.create [ Menu.viewItems tabs ]
