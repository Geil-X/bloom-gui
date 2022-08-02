module Gui.Panels.IconDock

open Avalonia.Controls
open Avalonia.Layout
open Avalonia.FuncUI.DSL
open Avalonia.FuncUI.Types
open Avalonia.Input

open Gui.DataTypes
open Gui.Views


type Msg =
    | NewFile
    | SaveAs
    | Open
    | NewFlower

let private iconButtons =
    [ Icon.newFile, NewFile
      Icon.save, SaveAs
      Icon.load, Open
      Icon.newFlower, NewFlower ]

let view (dispatch: Msg -> unit) =
    let button (icon: Icon.Size -> string -> IView<Viewbox>, msg) : IView =
        Button.create [
            Button.padding Theme.spacing.small
            Button.margin Theme.spacing.small
            Button.onClick (Event.handleEvent msg >> dispatch)
            Button.content (icon Icon.large Theme.palette.primaryLightest)
        ]
        :> IView

    let buttons: IView list =
        List.map button iconButtons

    StackPanel.create [
        StackPanel.orientation Orientation.Horizontal
        StackPanel.children buttons
    ]
