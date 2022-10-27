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
      SerialPorts: string list
      Behavior: Behavior }

[<RequireQualifiedAccess>]
type External =
    | OpenSerialPort of SerialPort
    | CloseSerialPort of SerialPort
    | ChangePort of SerialPortName
    | SendCommand of Command
    | NoMsg

[<RequireQualifiedAccess>]
type Msg =
    | SendExternal of External
    | ChangeTargetPercent of Percent
    | ChangeMaxSpeed of AngularSpeed
    | ChangeAcceleration of AngularAcceleration
    | RefreshSerialPorts of AsyncOperationStatus<unit, SerialPortName list>
    | SendCommandOfId of Command.Id
    | BehaviorSelected of Behavior


let presets =
    {| speedEmpty = AngularSpeed.microstepsPerSecond 0.
       minSpeed = AngularSpeed.microstepsPerSecond 0.
       maxSpeed = AngularSpeed.microstepsPerSecond 65000.
       accelerationEmpty = AngularAcceleration.microstepsPerSecondSquared 0.
       minAcceleration = AngularAcceleration.microstepsPerSecondSquared 0.
       maxAcceleration = AngularAcceleration.microstepsPerSecondSquared 10000. |}

[<Literal>]
let noPort = "No Serial Port"


let init () : State * Cmd<'Msg> =
    { TargetPercent = Percent.zero
      MaxSpeed = AngularSpeed.zero
      Acceleration = AngularAcceleration.zero
      SerialPorts = []
      Behavior = UserControlled },
    Cmd.none

/// ---- Update ----------------------------------------------------------------

let getPorts: Cmd<Msg> =
    Cmd.OfTask.perform SerialPort.getPorts () (Finished >> Msg.RefreshSerialPorts)

let sendCommand commandId (state: State) : External =
    match commandId with
    | Command.Id.NoCommand -> Command.NoCommand
    | Command.Id.Setup -> Command.Setup
    | Command.Id.Home -> Command.Home
    | Command.Id.Open -> Command.Open
    | Command.Id.Close -> Command.Close
    | Command.Id.OpenTo -> Command.OpenTo state.TargetPercent
    | Command.Id.MaxSpeed -> Command.MaxSpeed state.MaxSpeed
    | Command.Id.Acceleration -> Command.Acceleration state.Acceleration
    | Command.Id.Ping -> Command.Ping
    | _ -> Command.NoCommand

    |> External.SendCommand

let update (msg: Msg) (state: State) : State * Cmd<Msg> * External =
    match msg with
    | Msg.SendExternal msg -> state, Cmd.none, msg
    | Msg.ChangeTargetPercent percentage -> { state with TargetPercent = percentage }, Cmd.none, External.NoMsg
    | Msg.ChangeMaxSpeed maxSpeed -> { state with MaxSpeed = maxSpeed }, Cmd.none, External.NoMsg
    | Msg.ChangeAcceleration acceleration -> { state with Acceleration = acceleration }, Cmd.none, External.NoMsg
    | Msg.RefreshSerialPorts asyncOperation ->
        match asyncOperation with
        | Start _ -> state, getPorts, External.NoMsg
        | Finished serialPorts -> { state with SerialPorts = serialPorts }, Cmd.none, External.NoMsg
    | Msg.SendCommandOfId commandId -> state, Cmd.none, sendCommand commandId state
    | Msg.BehaviorSelected behavior -> { state with Behavior = behavior }, Cmd.none, External.NoMsg


/// ---- View ------------------------------------------------------------------


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
            Slider.onValueChanged (properties.Conversion >> properties.OnChanged)
            Slider.dock Dock.Left
        ]

    let displayText =
        properties.Value
        |> properties.Display
        |> Float.roundFloatTo 2
        |> string

    let textInput =
        TextBlock.create [
            TextBlock.text displayText
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


let private targetPercentageView (targetPercentage: Percent) (dispatch: Msg -> unit) =
    sliderView
        { Name = "Target Percentage"
          Value = targetPercentage
          Min = Percent.zero
          Max = Percent.oneHundred
          OnChanged = (fun newPercent -> Msg.ChangeTargetPercent(newPercent) |> dispatch)
          Display = Percent.inPercentage
          Conversion = Percent.percent }

let private maxSpeedView (speed: AngularSpeed) (dispatch: Msg -> unit) =
    sliderView
        { Name = "Max Speed"
          Value = speed
          Min = presets.minSpeed
          Max = presets.maxSpeed
          OnChanged = Msg.ChangeMaxSpeed >> dispatch
          Display = AngularSpeed.inMicrostepsPerSecond
          Conversion = AngularSpeed.microstepsPerSecond }

let private accelerationView (accel: AngularAcceleration) (dispatch: Msg -> unit) =
    sliderView
        { Name = "Acceleration"
          Value = accel
          Min = presets.minAcceleration
          Max = presets.maxAcceleration
          OnChanged = Msg.ChangeAcceleration >> dispatch
          Display = AngularAcceleration.inMicrostepsPerSecondSquared
          Conversion = AngularAcceleration.microstepsPerSecondSquared }

let private iconButton
    (name: string)
    icon
    (commandId: Command.Id)
    (flowerOption: Flower option)
    (dispatch: Msg -> unit)
    =
    match flowerOption with
    | Some _ ->
        Form.iconTextButton
            (icon Icon.medium Theme.palette.info)
            name
            Theme.palette.foreground
            (fun _ -> Msg.SendCommandOfId commandId |> dispatch)
            SubPatchOptions.Never

    | None ->
        Form.iconTextButton
            (icon Icon.medium Theme.palette.info)
            name
            Theme.palette.foreground
            (fun _ -> ())
            SubPatchOptions.Never

        |> View.withAttr (Button.isEnabled false)

let behaviorsView state dispatch =
    let behaviors = [ UserControlled; Bloom ]

    Form.formElement
        {| Name = "Behaviors"
           Orientation = Orientation.Vertical
           Element =
            ListBox.create [
                ListBox.dataItems behaviors
                ListBox.selectedItem state.Behavior
                ListBox.selectionMode SelectionMode.Toggle
                ListBox.onSelectedItemChanged (fun behavior ->
                    if not <| isNull behavior then
                        behavior :?> Behavior
                        |> Msg.BehaviorSelected
                        |> dispatch)
            ] |}

let view (state: State) (flowerOption: Flower option) (serialPort: SerialPort option) (dispatch: Msg -> unit) =
    let children: IView list =
        [ Text.iconTitle (Icon.command Icon.medium Theme.palette.primary) "Commands" Theme.palette.foreground

          serialPortView state.SerialPorts serialPort dispatch

          iconButton "Ping" Icon.ping Command.Id.Ping flowerOption dispatch
          iconButton "Home" Icon.home Command.Id.Home flowerOption dispatch
          iconButton "Open" Icon.openIcon Command.Id.Open flowerOption dispatch
          iconButton "Close" Icon.close Command.Id.Close flowerOption dispatch

          iconButton "Open To" Icon.openTo Command.Id.OpenTo flowerOption dispatch
          targetPercentageView state.TargetPercent dispatch

          iconButton "Set Max Speed" Icon.speed Command.Id.MaxSpeed flowerOption dispatch
          maxSpeedView state.MaxSpeed dispatch

          iconButton "Set Acceleration" Icon.acceleration Command.Id.Acceleration flowerOption dispatch
          accelerationView state.Acceleration dispatch

          behaviorsView state dispatch ]

    StackPanel.create [
        StackPanel.children children
        StackPanel.minWidth 240.
    ]
