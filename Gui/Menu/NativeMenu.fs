namespace Gui.Menu

module NativeMenu =
    
    open Avalonia.Controls
    
    open Gui

    type MenuItemWithMsg<'Msg> = { Item: NativeMenuItem; Msg: 'Msg }

    let menuItem (menuAction: MenuAction<'Msg>) : MenuItemWithMsg<'Msg> =
        { Msg = menuAction.Msg
          Item = NativeMenuItem menuAction.Name }

    let menuTab (menuTabData: MenuTab<'Msg>) : MenuItemWithMsg<'Msg> list * NativeMenuItem =
        let menu = NativeMenu()
        let tab = NativeMenuItem menuTabData.Name

        let menuItems =
            List.map menuItem menuTabData.Items

        tab.Menu <- menu

        for menuItem in menuItems do
            menu.Add(menuItem.Item)

        menuItems, tab

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

    open Gui

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
