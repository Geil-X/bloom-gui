module Gui.Panels.FlowerPanel

open Avalonia.Controls
open Avalonia.FuncUI.DSL

open Gui
open Gui.DataTypes
open Gui.Views

type Msg =
    | FlowerPropertiesMsg of FlowerProperties.Msg
    | FlowerCommandsMsg of FlowerCommands.Msg

let view (flowerOption: Flower option) (port: string option) (dispatch: Msg -> Unit) =
    StackPanel.create [
        StackPanel.children [
            FlowerProperties.view flowerOption (FlowerPropertiesMsg >> dispatch)
            StackPanel.verticalSeparator (FlowerCommands.view flowerOption port (FlowerCommandsMsg >> dispatch))
        ]
        StackPanel.minWidth 200.
    ]
