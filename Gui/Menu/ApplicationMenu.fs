module Gui.Menu.ApplicationMenu

open Avalonia.Controls
open Avalonia.FuncUI.DSL
open Avalonia.FuncUI.Hosts
open Avalonia.FuncUI.Types
open Elmish

open Gui

let menuItem (item: MenuAction<'Msg>) (dispatch: 'Msg -> unit): IView =
    MenuItem.create [
        MenuItem.header item.Name
        MenuItem.onClick (fun _ -> dispatch item.Msg )
    ]
    
let menuTab  (menuTab: MenuTab<'Msg>) (dispatch: 'Msg -> unit): IView =
    let menuItemHelper item = menuItem item dispatch
    let menuItems =List.map menuItemHelper menuTab.Items
    
    MenuItem.create [
        MenuItem.header menuTab.Name
        MenuItem.viewItems  menuItems
    ]
    
let view (menuBar: MenuBar<'Msg>) (dispatch: 'Msg -> unit): IView =
    let menuTabHelper item = menuTab item dispatch
    let tabs = List.map menuTabHelper menuBar
    
    Menu.create [
        Menu.viewItems tabs
    ]