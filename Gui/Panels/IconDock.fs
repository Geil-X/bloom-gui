module Gui.Panels.IconDock

open Avalonia.Controls
open Avalonia.Layout
open Avalonia.FuncUI.DSL
open Avalonia.FuncUI.Types

open Avalonia.Styling
open Extensions
open Gui
open Gui.Widgets


type Msg =
    | NewFile
    | SaveAs
    | Open
    | NewFlower

let private iconButtons =
    [ Icons.newFile, NewFile
      Icons.save, SaveAs
      Icons.load, Open
      Icons.newFlower, NewFlower ]

let view (dispatch: Msg -> unit) =
    let button (icon: string -> IView, msg) : IView =
        Button.create [
            Button.padding Theme.spacing.small
            Button.margin Theme.spacing.small
            Button.onClick (Event.handleEvent msg >> dispatch)
            Button.content (icon Theme.palette.primaryLightest)
        ]

    let buttons: IView list = List.map button iconButtons

    StackPanel.create [
        StackPanel.orientation Orientation.Horizontal
        StackPanel.children buttons
    ]
