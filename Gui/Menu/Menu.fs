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
/// means of accessing that functionality.
module Gui.Menu.Menu


open Avalonia.Controls
open Avalonia.FuncUI.Hosts
open Elmish

open Gui
open Gui.DataTypes

type Msg =
    // ---- File -----
    | NewFile
    | OpenFile
    | SaveAs

let private fileMenu =
    { Name = "File"
      Items =
        [ { Name = "New File"; Msg = NewFile }
          { Name = "Open"; Msg = OpenFile }
          { Name = "Save As"; Msg = SaveAs } ] }

let private menuBar: MenuBar<Msg> =
    [ fileMenu ]

/// Create a menu bar at the top of the application window. This is the main
/// interaction method for a lot of the core functionality of the application.
/// This menu provides access to the core functionality of the application,
/// or to window dialogues that contain more central information.
let applicationMenu (dispatch: Msg -> unit) = ApplicationMenu.view menuBar dispatch

//let addNativeMenuToWindow window =
//    nativeMenuFromWindow window menuBar
