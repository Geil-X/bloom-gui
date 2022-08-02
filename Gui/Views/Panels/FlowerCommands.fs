module Gui.Panels.FlowerCommands

open Avalonia.Controls
open Avalonia.FuncUI.DSL
open Avalonia.FuncUI.Types
open Avalonia.Layout
open System
open System.IO.Ports

open Gui.DataTypes
open Gui.Views
open Gui
open Extensions

type Msg =
    | ChangePort of string
    | ChangePercentage of Flower Id * ClampedPercentage
    | ChangeSpeed of Flower Id * int
    | OpenSerialPortsDropdown
    | OpenSerialPort of SerialPort
    | CloseSerialPort of SerialPort
    | ChangeAcceleration of Flower Id * int
    | SendCommand of Command

let presets =
    {| speedEmpty = 0
       minSpeed = 0.
       maxSpeed = 65000
       accelerationEmpty = 0
       minAcceleration = 0.
       maxAcceleration = 10000. |}

[<Literal>]
let noPort = "No Serial Port"

let private serialPortView (serialPorts: string list) (serialPortOption: SerialPort option) dispatch =
    let ports =
        noPort :: serialPorts |> Array.ofList

    let portIcon =
        Icon.connection Icon.small Theme.palette.info
        |> View.withAttr (Viewbox.dock Dock.Left)

    let connectionStatus =
        match serialPortOption with
        | Some serialPort when serialPort.IsOpen ->
            let icon =
                Icon.connected Icon.small Theme.palette.success

            Button.create [
                Button.onClick (
                    (fun _ -> CloseSerialPort serialPort |> dispatch),
                    SubPatchOptions.OnChangeOf serialPort.PortName
                )
                Button.content icon
            ]

        | Some serialPort ->
            let icon =
                Icon.disconnected Icon.small Theme.palette.danger

            Button.create [
                Button.onClick (
                    (fun _ -> OpenSerialPort serialPort |> dispatch),
                    SubPatchOptions.OnChangeOf serialPort.PortName
                )
                Button.content icon
            ]

        | None ->
            let icon =
                Icon.disconnected Icon.small Theme.palette.foregroundFaded

            Button.create [
                Button.content icon
                Button.isEnabled false
            ]

        |> View.withAttrs [
            Viewbox.dock Dock.Right
            Viewbox.width 24.
           ]

    let selected =
        serialPortOption
        |> Option.map (fun serialPort -> serialPort.PortName)
        |> Option.defaultValue noPort

    let dropdown =
        ComboBox.create [
            ComboBox.margin (Theme.spacing.small, 0.)
            ComboBox.dataItems ports
            ComboBox.dock Dock.Left
            ComboBox.selectedItem selected
            ComboBox.onPointerEnter (fun _ -> dispatch OpenSerialPortsDropdown)
            ComboBox.onSelectedItemChanged (fun port -> ChangePort(port :?> string) |> dispatch)
        ]

    Form.formElement
        {| Name = "Serial Port"
           Orientation = Orientation.Vertical
           Element =
            DockPanel.create [
                DockPanel.children [ portIcon; connectionStatus; dropdown ]
            ] |}

type SliderProperties =
    { Name: string
      Value: float
      Min: float
      Max: float
      OnChanged: Flower.Id -> float -> unit
      FlowerId: Flower.Id option }

let private sliderView (properties: SliderProperties) =
    let slider =
        match properties.FlowerId with
        | Some flowerId ->
            Slider.create [
                Slider.width 140.
                Slider.minimum properties.Min
                Slider.maximum properties.Max
                Slider.value properties.Value
                Slider.onValueChanged (properties.OnChanged flowerId, SubPatchOptions.OnChangeOf flowerId)
                Slider.dock Dock.Left
            ]

        | None ->
            Slider.create [
                Slider.width 140.
                Slider.value properties.Value
                Slider.minimum properties.Min
                Slider.maximum properties.Max
                Slider.isEnabled false
                Slider.onValueChanged ((fun _ -> ()), SubPatchOptions.OnChangeOf Guid.Empty)
                Slider.dock Dock.Left
            ]

    let textInput =
        TextBlock.create [
            TextBlock.text (properties.Value |> int |> string)
            TextBlock.dock Dock.Right
            TextBlock.margin (Theme.spacing.small, 0.)
        ]

    Form.formElement
        {| Name = properties.Name
           Orientation = Orientation.Vertical
           Element =
            DockPanel.create [
                StackPanel.children [ slider; textInput ]
            ] |}


let private openPercentageView (flowerOption: Flower option) (dispatch: Msg -> unit) =
    sliderView
        { Name = "Open Percentage"
          Value =
            Option.map (fun flower -> ClampedPercentage.inPercentage flower.OpenPercent) flowerOption
            |> Option.defaultValue ClampedPercentage.minimum
          Min = ClampedPercentage.minimum
          Max = ClampedPercentage.maxPercentage
          OnChanged =
            (fun flowerId newPercent ->
                ChangePercentage(flowerId, ClampedPercentage.percent newPercent)
                |> dispatch)
          FlowerId = Option.map (fun flower -> flower.Id) flowerOption }

let private speedView (flowerOption: Flower option) (dispatch: Msg -> unit) =
    sliderView
        { Name = "Speed"
          Value =
            Option.map Flower.maxSpeed flowerOption
            |> Option.defaultValue presets.speedEmpty
            |> float
          Min = presets.minSpeed
          Max = float presets.maxSpeed
          OnChanged = (fun flowerId newSpeed -> ChangeSpeed(flowerId, int newSpeed) |> dispatch)
          FlowerId = Option.map (fun flower -> flower.Id) flowerOption }

let private accelerationView (flowerOption: Flower option) (dispatch: Msg -> unit) =
    sliderView
        { Name = "Acceleration"
          Value =
            Option.map Flower.acceleration flowerOption
            |> Option.defaultValue presets.accelerationEmpty
            |> float
          Min = presets.minAcceleration
          Max = presets.maxAcceleration
          OnChanged =
            (fun flowerId newAcceleration ->
                ChangeAcceleration(flowerId, int newAcceleration)
                |> dispatch)
          FlowerId = Option.map (fun flower -> flower.Id) flowerOption }

let private iconButton
    (name: string)
    icon
    (onClick: Flower -> Command)
    (flowerOption: Flower option)
    (serialPortOption: SerialPort option)
    dispatch
    =
    match flowerOption, serialPortOption with
    | Some flower, Some serialPort when serialPort.IsOpen ->
        Form.iconTextButton
            (icon Icon.medium Theme.palette.info)
            name
            Theme.palette.foreground
            (fun _ -> onClick flower |> SendCommand |> dispatch)
            (SubPatchOptions.OnChangeOf
                {| Id = flower.Id
                   Command = onClick flower |})
    | _ ->
        Form.iconTextButton
            (icon Icon.medium Theme.palette.info)
            name
            Theme.palette.foreground
            (fun _ -> ())
            SubPatchOptions.Never
        |> View.withAttr (Button.isEnabled false)

let view
    (flowerOption: Flower option)
    (serialPorts: string list)
    (serialPort: SerialPort option)
    (dispatch: Msg -> unit)
    =
    let children: IView list =
        [ Text.iconTitle (Icon.command Icon.medium Theme.palette.primary) "Commands" Theme.palette.foreground
          serialPortView serialPorts serialPort dispatch
          iconButton "Ping" Icon.ping (fun _ -> Ping) flowerOption serialPort dispatch
          iconButton "Home" Icon.home (fun _ -> Home) flowerOption serialPort dispatch
          iconButton "Open" Icon.openIcon (fun _ -> Open) flowerOption serialPort dispatch
          iconButton "Close" Icon.close (fun _ -> Close) flowerOption serialPort dispatch
          iconButton "Open To" Icon.openTo (Flower.openPercent >> OpenTo) flowerOption serialPort dispatch
          openPercentageView flowerOption dispatch
          iconButton "Set Speed" Icon.speed (Flower.maxSpeed >> uint >> Speed) flowerOption serialPort dispatch
          speedView flowerOption dispatch
          iconButton
              "Set Acceleration"
              Icon.acceleration
              (Flower.acceleration >> uint >> Acceleration)
              flowerOption
              serialPort
              dispatch
          accelerationView flowerOption dispatch ]

    StackPanel.create [
        StackPanel.children children
        StackPanel.minWidth 200.
    ]
