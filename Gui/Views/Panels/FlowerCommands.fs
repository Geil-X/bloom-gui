module Gui.Views.Panels.FlowerCommands

open Avalonia.Controls
open Avalonia.FuncUI.DSL
open Avalonia.FuncUI.Types
open Avalonia.Layout
open Elmish
open Math.Units
open System
open System.IO.Ports

open Gui.DataTypes
open Gui.Views.Components
open Extensions

type State =
    { TargetPercent: Percent
      MaxSpeed: AngularSpeed
      Acceleration: AngularAcceleration
      SerialPorts: string list }

[<RequireQualifiedAccess>]
type External =
    | OpenSerialPort of SerialPort
    | CloseSerialPort of SerialPort
    | ChangePort of string
    | SendCommand of Command
    | NoMsg

[<RequireQualifiedAccess>]
type Msg =
    | ChangeTargetPercent of Percent
    | ChangeMaxSpeed of AngularSpeed
    | ChangeAcceleration of AngularAcceleration
    | RefreshSerialPorts of AsyncOperationStatus<unit, string list>
    | SendExternal of External


let init () : State * Cmd<'Msg> =
    { TargetPercent = Percent.zero
      MaxSpeed = AngularSpeed.zero
      Acceleration = AngularAcceleration.zero
      SerialPorts = [] },
    Cmd.none

/// ---- Update ----------------------------------------------------------------

let update (msg: Msg) (state: State) : State * Cmd<Msg> * External =
    match msg with
    | Msg.ChangeTargetPercent percentage -> { state with TargetPercent = percentage }, Cmd.none, External.NoMsg

    | Msg.ChangeMaxSpeed speed -> { state with MaxSpeed = speed }, Cmd.none, External.NoMsg

    | Msg.ChangeAcceleration acceleration -> { state with Acceleration = acceleration }, Cmd.none, External.NoMsg

    | Msg.RefreshSerialPorts asyncOperation ->
        match asyncOperation with
        | Start _ ->
            state, Cmd.OfTask.perform SerialPort.getPorts () (Finished >> Msg.RefreshSerialPorts), External.NoMsg
        | Finished serialPorts -> { state with SerialPorts = serialPorts }, Cmd.none, External.NoMsg

    | Msg.SendExternal msg -> state, Cmd.none, msg

/// ---- View ------------------------------------------------------------------

let presets =
    {| speedEmpty = AngularSpeed.turnsPerSecond 0.
       minSpeed = AngularSpeed.turnsPerSecond 0.
       maxSpeed = AngularSpeed.turnsPerSecond 65000.
       accelerationEmpty = AngularAcceleration.turnsPerSecondSquared 0.
       minAcceleration = AngularAcceleration.turnsPerSecondSquared 0.
       maxAcceleration = AngularAcceleration.turnsPerSecondSquared 10000. |}

[<Literal>]
let noPort = "No Serial Port"

let private serialPortView (serialPorts: string list) (serialPortOption: SerialPort option) (dispatch: Msg -> unit) =
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
                    (fun _ ->
                        External.CloseSerialPort serialPort
                        |> Msg.SendExternal
                        |> dispatch),
                    SubPatchOptions.OnChangeOf serialPort.PortName
                )
                Button.content icon
            ]

        | Some serialPort ->
            let icon =
                Icon.disconnected Icon.small Theme.palette.danger

            Button.create [
                Button.onClick (
                    (fun _ ->
                        External.OpenSerialPort serialPort
                        |> Msg.SendExternal
                        |> dispatch),
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
            ComboBox.onPointerEnter (fun _ -> Start() |> Msg.RefreshSerialPorts |> dispatch)
            ComboBox.onSelectedItemChanged (fun port ->
                External.ChangePort(port :?> string)
                |> Msg.SendExternal
                |> dispatch)
        ]

    Form.formElement
        {| Name = "Serial Port"
           Orientation = Orientation.Vertical
           Element =
            DockPanel.create [
                DockPanel.children [
                    portIcon
                    connectionStatus
                    dropdown
                ]
            ] |}

type SliderProperties<'Units> =
    { Name: string
      Value: Quantity<'Units>
      Min: Quantity<'Units>
      Max: Quantity<'Units>
      OnChanged: Quantity<'Units> -> unit
      Display: Quantity<'Units> -> float
      Conversion: float -> Quantity<'Units> }

let private sliderView (properties: SliderProperties<'Units>) =
    let slider =
        Slider.create [
            Slider.width 140.
            Slider.minimum (properties.Display properties.Min)
            Slider.maximum (properties.Display properties.Max)
            Slider.value (properties.Display properties.Value)
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
                StackPanel.children [
                    slider
                    textInput
                ]
            ] |}


let private openPercentageView (flowerOption: Flower option) (dispatch: Msg -> unit) =
    sliderView
        { Name = "Target Percentage"
          Value =
            flowerOption
            |> Option.map (fun flower -> flower.TargetPercent)
            |> Option.defaultValue Quantity.zero
          Min = Percent.decimal Percent.minimum
          Max = Percent.decimal Percent.maxDecimal
          OnChanged = (fun newPercent -> Msg.ChangeTargetPercent(newPercent) |> dispatch)
          Display = Percent.inPercentage >> Float.roundFloatTo 2
          Conversion = Percent.percent }

let private speedView (speed: AngularSpeed) (dispatch: Msg -> unit) =
    sliderView
        { Name = "Max Speed"
          Value = speed
          Min = presets.minSpeed
          Max = presets.maxSpeed
          OnChanged = Msg.ChangeMaxSpeed >> dispatch
          Display =
            AngularSpeed.inTurnsPerSecond
            >> Float.roundFloatTo 2
          Conversion = AngularSpeed.turnsPerSecond }

let private accelerationView (accel: AngularAcceleration) (dispatch: Msg -> unit) =
    sliderView
        { Name = "Acceleration"
          Value = accel
          Min = presets.minAcceleration
          Max = presets.maxAcceleration
          OnChanged = Msg.ChangeAcceleration >> dispatch
          Display =
            AngularAcceleration.inTurnsPerSecondSquared
            >> Float.roundFloatTo 2
          Conversion = AngularAcceleration.turnsPerSecondSquared }

let private iconButton
    (name: string)
    icon
    (onClick: unit -> Command)
    (flowerOption: Flower option)
    (dispatch: Msg -> unit)
    =
    match flowerOption with
    | Some _ ->
        Form.iconTextButton
            (icon Icon.medium Theme.palette.info)
            name
            Theme.palette.foreground
            (fun _ ->
                onClick ()
                |> External.SendCommand
                |> Msg.SendExternal
                |> dispatch)
            SubPatchOptions.Never

    | None ->
        Form.iconTextButton
            (icon Icon.medium Theme.palette.info)
            name
            Theme.palette.foreground
            (fun _ -> ())
            SubPatchOptions.Never

        |> View.withAttr (Button.isEnabled false)

let view (state: State) (flowerOption: Flower option) (serialPort: SerialPort option) (dispatch: Msg -> unit) =
    let children: IView list =
        [ Text.iconTitle (Icon.command Icon.medium Theme.palette.primary) "Commands" Theme.palette.foreground

          serialPortView state.SerialPorts serialPort dispatch
          iconButton "Ping" Icon.ping (fun _ -> Ping) flowerOption dispatch
          iconButton "Home" Icon.home (fun _ -> Home) flowerOption dispatch
          iconButton "Open" Icon.openIcon (fun _ -> Open) flowerOption dispatch
          iconButton "Close" Icon.close (fun _ -> Close) flowerOption dispatch
          iconButton "Open To" Icon.openTo (fun _ -> OpenTo state.TargetPercent) flowerOption dispatch
          openPercentageView flowerOption dispatch

          iconButton "Set Max Speed" Icon.speed (fun _ -> MaxSpeed state.MaxSpeed) flowerOption dispatch
          speedView state.MaxSpeed dispatch

          iconButton
              "Set Acceleration"
              Icon.acceleration
              (fun _ -> Acceleration state.Acceleration)
              flowerOption
              dispatch
          accelerationView state.Acceleration dispatch ]

    StackPanel.create [
        StackPanel.children children
        StackPanel.minWidth 240.
    ]
