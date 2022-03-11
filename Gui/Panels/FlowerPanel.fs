module Gui.Panels.FlowerPanel

open Avalonia.Controls
open Avalonia.FuncUI.DSL

open Gui

type Msg =
    | FlowerPropertiesMsg of FlowerProperties.Msg
    | FlowerCommandsMsg of FlowerCommands.Msg

let view (flowerOption: Flower.State option) (dispatch: Msg -> Unit) =
    StackPanel.create [
        StackPanel.children
            [FlowerProperties.view flowerOption (FlowerPropertiesMsg >> dispatch)
             FlowerCommands.view flowerOption (FlowerCommandsMsg >> dispatch)
             ]
        StackPanel.minWidth 200.
    ]
