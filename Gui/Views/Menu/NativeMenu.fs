namespace Gui.Views.Menu

open Gui.DataTypes

module NativeMenu =

    open Avalonia.Controls

    type MenuItemWithMsg<'Msg> = { Item: NativeMenuItem; Msg: 'Msg }

    let menuActionView (menuAction: MenuAction<'Msg>) : MenuItemWithMsg<'Msg> =
        { Msg = menuAction.Msg
          Item = NativeMenuItem menuAction.Name }

    let menuItemView (menuItem: MenuItem<'Msg>) : MenuItemWithMsg<'Msg> list * NativeMenuItem =
        match menuItem with
        | MenuItem.Action action ->
            let actionView = menuActionView action
            [ actionView ], actionView.Item

        | MenuItem.Dropdown dropdown ->
            let actions =
                List.map menuActionView dropdown.Actions

            // Create the dropdown item and menu objects
            let nativeMenuItem =
                NativeMenuItem dropdown.Name

            let nativeMenu = NativeMenu()
            nativeMenuItem.Menu <- nativeMenu

            // Assign all the actions to the dropdown menu
            for action in actions do
                nativeMenu.Add action.Item

            actions, nativeMenuItem

    let menuTab (menuTab: MenuTab<'Msg>) : MenuItemWithMsg<'Msg> list * NativeMenuItem =
        let menu = NativeMenu()
        let tab = NativeMenuItem menuTab.Name

        tab.Menu <- menu

        let mutable menuTabMsgs = []

        for menuItem in menuTab.Items do
            let menuItemsWithMsgs, nativeMenuItem =
                menuItemView menuItem

            menuTabMsgs <- menuItemsWithMsgs @ menuTabMsgs
            menu.Add(nativeMenuItem)

        menuTabMsgs, tab

    let fromMenuBar (menuBar: MenuBar<'Msg>) : MenuItemWithMsg<'Msg> list * NativeMenu =
        let menu = NativeMenu()

        let menuItems =
            List.fold
                (fun menuItemMsgs menuTabData ->
                    let tabItems, nativeTabMenu =
                        menuTab menuTabData

                    menu.Add(nativeTabMenu)

                    List.append tabItems menuItemMsgs)
                []
                menuBar

        menuItems, menu

module Program =
    open Avalonia.Controls
    open Avalonia.FuncUI.Hosts
    open Elmish

    /// Set up a native window from the base Avalonia Window class
    /// `Avalonia.FuncUI.Hosts.HostWindow` class.
    let withNativeMenu
        (window: HostWindow)
        (menuBar: MenuBar<'MenuMsg>)
        (conv: 'MenuMsg -> 'msg)
        (program: Program<'arg, 'model, 'msg, 'view>)
        : Program<'arg, 'model, 'msg, 'view> =

        let menuItemsWithMessages, menu =
            NativeMenu.fromMenuBar menuBar

        NativeMenu.SetMenu(window, menu)

        let subscription (_: 'model) =
            let sub (dispatch: 'msg -> unit) =

                for menuItem in menuItemsWithMessages do
                    menuItem.Item.Clicked.Add(fun _ -> menuItem.Msg |> conv |> dispatch)

            Cmd.ofSub sub

        Program.withSubscription subscription program
