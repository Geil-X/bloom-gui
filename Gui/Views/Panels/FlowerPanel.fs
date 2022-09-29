module Gui.Views.Panels.FlowerPanel

open System.IO.Ports
open Avalonia.Controls
open Avalonia.FuncUI.DSL

open Gui.DataTypes
open Gui.Views.Components

type Msg =
    | FlowerPropertiesMsg of FlowerProperties.Msg
    | FlowerCommandsMsg of FlowerCommands.Msg

let view
    (flowerCommandsState: FlowerCommands.State)
    (flowers: Flower seq)
    (flowerOption: Flower option)
    (serialPort: SerialPort option)
    (dispatch: Msg -> unit)
    =
    StackPanel.create [
        StackPanel.children [
            FlowerProperties.view flowers flowerOption (FlowerPropertiesMsg >> dispatch)
            StackPanel.verticalSeparator (
                FlowerCommands.view flowerCommandsState flowerOption serialPort (FlowerCommandsMsg >> dispatch)
            )
        ]
        StackPanel.minWidth 200.
    ]
