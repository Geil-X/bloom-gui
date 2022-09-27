module Gui.Views.Panels.FlowerCommands

open Avalonia.Controls
open Avalonia.FuncUI.DSL
open Avalonia.FuncUI.Types
open Avalonia.Layout
open Math.Units
open System
open System.IO.Ports

open Gui.DataTypes
open Gui.Views.Components
open Extensions

type Msg =
    | ChangePort of string
    | ChangePercentage of Flower Id * Percent
    | ChangeSpeed of Flower Id * AngularSpeed
    | ChangeMaxSpeed of Flower Id * AngularSpeed
    | ChangeAcceleration of Flower Id * AngularAcceleration
    | OpenSerialPortsDropdown
    | OpenSerialPort of SerialPort
    | CloseSerialPort of SerialPort
    | SendCommand of Command

let presets =
    {| speedEmpty = AngularSpeed.turnsPerSecond 0.
       minSpeed = AngularSpeed.turnsPerSecond 0.
       maxSpeed = AngularSpeed.turnsPerSecond 65000.
       accelerationEmpty = AngularAcceleration.turnsPerSecondSquared 0.
       minAcceleration = AngularAcceleration.turnsPerSecondSquared 0.
       maxAcceleration = AngularAcceleration.turnsPerSecondSquared 10000. |}

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

type SliderProperties<'Units> =
    { Name: string
      Value: Quantity<'Units>
      Min: Quantity<'Units>
      Max: Quantity<'Units>
      OnChanged: Flower.Id -> Quantity<'Units> -> unit
      Display: Quantity<'Units> -> float
      Conversion: float -> Quantity<'Units>
      FlowerId: Flower.Id option }

let private sliderView (properties: SliderProperties<'Units>) =
    let slider =
        match properties.FlowerId with
        | Some flowerId ->
            Slider.create [
                Slider.width 140.
                Slider.minimum (properties.Display properties.Min)
                Slider.maximum (properties.Display properties.Max)
                Slider.value (properties.Display properties.Value)
                Slider.onValueChanged (
                    properties.Conversion
                    >> properties.OnChanged flowerId,
                    SubPatchOptions.OnChangeOf flowerId
                )
                Slider.dock Dock.Left
            ]

        | None ->
            Slider.create [
                Slider.width 140.
                Slider.value (properties.Display properties.Value)
                Slider.minimum (properties.Display properties.Min)
                Slider.maximum (properties.Display properties.Max)
                Slider.isEnabled false
                Slider.onValueChanged ((fun _ -> ()), SubPatchOptions.OnChangeOf Guid.Empty)
                Slider.dock Dock.Left
            ]

    let textInput =
        TextBlock.create [
            TextBlock.text (properties.Value |> properties.Display |> string)
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
        { Name = "Target Percentage"
          Value =
            Option.map (fun flower -> flower.TargetPercent) flowerOption
            |> Option.defaultValue Quantity.zero
          Min = Percent.decimal Percent.minimum
          Max = Percent.decimal Percent.maxDecimal
          OnChanged = (fun flowerId newPercent -> ChangePercentage(flowerId, newPercent) |> dispatch)
          Display = Percent.inPercentage >> Float.roundFloatTo 2
          Conversion = Percent.percent
          FlowerId = Option.map (fun flower -> flower.Id) flowerOption }

let private speedView (flowerOption: Flower option) (dispatch: Msg -> unit) =
    sliderView
        { Name = "Max Speed"
          Value =
            Option.map Flower.maxSpeed flowerOption
            |> Option.defaultValue presets.speedEmpty
          Min = presets.minSpeed
          Max = presets.maxSpeed
          OnChanged = (fun flowerId newSpeed -> ChangeMaxSpeed(flowerId, newSpeed) |> dispatch)
          Display =
            AngularSpeed.inTurnsPerSecond
            >> Float.roundFloatTo 2
          Conversion = AngularSpeed.turnsPerSecond
          FlowerId = Option.map (fun flower -> flower.Id) flowerOption }

let private accelerationView (flowerOption: Flower option) (dispatch: Msg -> unit) =
    sliderView
        { Name = "Acceleration"
          Value =
            Option.map Flower.acceleration flowerOption
            |> Option.defaultValue presets.accelerationEmpty
          Min = presets.minAcceleration
          Max = presets.maxAcceleration
          OnChanged =
            (fun flowerId newAcceleration ->
                ChangeAcceleration(flowerId, newAcceleration)
                |> dispatch)
          Display =
            AngularAcceleration.inTurnsPerSecondSquared
            >> Float.roundFloatTo 2
          Conversion = AngularAcceleration.turnsPerSecondSquared
          FlowerId = Option.map (fun flower -> flower.Id) flowerOption }

let private iconButton
    (name: string)
    icon
    (onClick: Flower -> Command)
    (flowerOption: Flower option)
    (serialPortOption: SerialPort option)
    dispatch
    =
    match flowerOption with
    | Some flower ->
        Form.iconTextButton
            (icon Icon.medium Theme.palette.info)
            name
            Theme.palette.foreground
            (fun _ -> onClick flower |> SendCommand |> dispatch)
            (SubPatchOptions.OnChangeOf
                {| Id = flower.Id
                   Command = onClick flower |})
    | None ->
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
          iconButton "Open To" Icon.openTo (Flower.targetPercent >> OpenTo) flowerOption serialPort dispatch
          openPercentageView flowerOption dispatch

          iconButton "Set Max Speed" Icon.speed (Flower.maxSpeed >> MaxSpeed) flowerOption serialPort dispatch
          speedView flowerOption dispatch

          iconButton
              "Set Acceleration"
              Icon.acceleration
              (Flower.acceleration >> Acceleration)
              flowerOption
              serialPort
              dispatch

          accelerationView flowerOption dispatch ]

    StackPanel.create [
        StackPanel.children children
        StackPanel.minWidth 240.
    ]
