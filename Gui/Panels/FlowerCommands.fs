module Gui.Panels.FlowerCommands

open Avalonia.Controls
open Avalonia.FuncUI.DSL
open Avalonia.FuncUI.Types
open Avalonia.Layout
open System
open System.IO.Ports

open Gui.DataTypes
open Gui.Widgets
open Gui
open Extensions

type Msg =
    | ChangePort of string
    | ChangePercentage of Flower Id * ClampedPercentage
    | ChangeSpeed of Flower Id * uint
    | ChangeAcceleration of Flower Id * uint
    | SendCommand of Command
    
[<Literal>]
let noPort = "No Serial Port"

let private serialPortView (serialPortOption: SerialPort option) dispatch =
    let ports =
        Array.append [| noPort |] (SerialPort.GetPortNames())

    let portState =
        match serialPortOption with
        | Some serialPort ->
            if serialPort.IsOpen then
                Icon.connected Icon.small Theme.palette.success

            else
                Icon.connection Icon.small Theme.palette.danger

        | None -> Icon.connection Icon.small Theme.palette.info
        |> View.withAttrs [
            Viewbox.dock Dock.Left
            Viewbox.margin (0., 0., Theme.spacing.small, 0.)
           ]

    let selected =
        serialPortOption
        |> Option.map (fun serialPort -> serialPort.PortName)
        |> Option.defaultValue noPort

    let dropdown =
        ComboBox.create [
            ComboBox.dataItems ports
            ComboBox.dock Dock.Right
            ComboBox.selectedItem selected
            ComboBox.onSelectedIndexChanged
                (fun index ->
                    match Array.tryItem index ports with
                    | Some port -> ChangePort port |> dispatch
                    | None -> ())
            if Option.isSome serialPortOption then
                ComboBox.selectedItem serialPortOption.Value.PortName
        ]

    Form.formElement
        {| Name = "Serial Port"
           Orientation = Orientation.Vertical
           Element =
               DockPanel.create [
                   DockPanel.children [ portState; dropdown ]
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
                Slider.width 140
                Slider.minimum properties.Min
                Slider.maximum properties.Max
                Slider.value properties.Value
                Slider.onValueChanged (properties.OnChanged flowerId, SubPatchOptions.OnChangeOf flowerId)
                Slider.dock Dock.Left
            ]

        | None ->
            Slider.create [
                Slider.width 140
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
              Option.map Flower.speed flowerOption
              |> Option.defaultValue 0u
              |> float
          Min = 0.
          Max = 10000.
          OnChanged = (fun flowerId newSpeed -> ChangeSpeed(flowerId, uint newSpeed) |> dispatch)
          FlowerId = Option.map (fun flower -> flower.Id) flowerOption }

let private accelerationView (flowerOption: Flower option) (dispatch: Msg -> unit) =
    sliderView
        { Name = "Acceleration"
          Value =
              Option.map Flower.acceleration flowerOption
              |> Option.defaultValue 0u
              |> float
          Min = 0.
          Max = 5000.
          OnChanged =
              (fun flowerId newAcceleration ->
                  ChangeAcceleration(flowerId, uint newAcceleration)
                  |> dispatch)
          FlowerId = Option.map (fun flower -> flower.Id) flowerOption }

let private iconButton name icon (onClick: Flower -> Command) (flowerOption: Flower option) dispatch =
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

let view (flowerOption: Flower option) (serialPort: SerialPort option) (dispatch: Msg -> unit) =
    let children: IView list =
        [ Text.iconTitle (Icon.command Icon.medium Theme.palette.primary) "Commands" Theme.palette.foreground
          serialPortView serialPort dispatch
          iconButton "Home" Icon.home (fun _ -> Home) flowerOption dispatch
          iconButton "Open" Icon.openIcon (fun _ -> Open) flowerOption dispatch
          iconButton "Close" Icon.close (fun _ -> Close) flowerOption dispatch
          iconButton "Open To" Icon.openTo (Flower.openPercent >> OpenTo) flowerOption dispatch
          openPercentageView flowerOption dispatch
          iconButton "Set Speed" Icon.speed (Flower.speed >> Speed) flowerOption dispatch
          speedView flowerOption dispatch
          iconButton "Set Acceleration" Icon.acceleration (Flower.acceleration >> Acceleration) flowerOption dispatch
          accelerationView flowerOption dispatch ]

    StackPanel.create [
        StackPanel.children children
        StackPanel.minWidth 200.
    ]
