/// A native menu is the menu that
/// appears at the top system menu bar on MacOS and in some applications under
/// Linux distributions. For MacOS, this menu is generally the main location of
/// the program menu. On Linux, the native menu is generally not implemented for
/// most applications and is implemented with an inbuilt menu at the top of the
/// application. The menu at the top of the application is also the only way
/// that menus are implemented on Windows systems, so generally Linux
/// applications have the same menu configuration. Menus in this module use the
/// native menu to set up native menus on MacOS and Linux, and implement the top
/// application menus for Windows and Linux. This allows Linux to have the
/// expected menu location at the top of the application but alternatively takes
/// advantage of as many native menu features as possible to provide an alternate
/// means of accessing that functionality. The Linux built in menu support will
/// most likely not be of the same quality as the MacOS menu because support for
/// MacOS native menus in Avalonia have better support.
module Gui.Menu.Menu


open System.IO
open Avalonia.Controls
open Avalonia.FuncUI.Types

open Gui

type Msg =
    // ---- File -----
    | NewFile
    | OpenFile
    | Open of FileInfo
    | SaveAs

let private fileMenu: MenuTab<Msg> =
    { Name = "File"
      Items =
        [ MenuItem.Action { Name = "New File"; Msg = NewFile }
          MenuItem.Action { Name = "Open"; Msg = OpenFile }
          MenuItem.Dropdown
              { Name = "Open Recent"
                Actions =
                  [ { Name = "Some file name"
                      Msg = Open(FileInfo("Bad Path Name")) } ] }
          MenuItem.Action { Name = "Save As"; Msg = SaveAs } ] }

let menuBar: MenuBar<Msg> = [ fileMenu ]

/// Create a menu bar at the top of the application window. This is the main
/// interaction method for a lot of the core functionality of the application.
/// This menu provides access to the core functionality of the application,
/// or to window dialogues that contain more central information.
let applicationMenu (dispatch: Msg -> unit) : IView<Menu> = ApplicationMenu.view menuBar dispatch
