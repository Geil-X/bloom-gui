module Gui.Panels.FlowerPanel

open System.IO.Ports
open Avalonia.Controls
open Avalonia.FuncUI.DSL

open Gui.DataTypes
open Gui.Views

type Msg =
    | FlowerPropertiesMsg of FlowerProperties.Msg
    | FlowerCommandsMsg of FlowerCommands.Msg

let view
    (flowerOption: Flower option)
    (serialPorts: string list)
    (serialPort: SerialPort option)
    (dispatch: Msg -> unit)
    =
    StackPanel.create [
        StackPanel.children [
            FlowerProperties.view flowerOption (FlowerPropertiesMsg >> dispatch)
            StackPanel.verticalSeparator (
                FlowerCommands.view flowerOption serialPorts serialPort (FlowerCommandsMsg >> dispatch)
            )
        ]
        StackPanel.minWidth 200.
    ]
