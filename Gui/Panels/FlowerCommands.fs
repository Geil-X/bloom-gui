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
    | Home of Flower Id
    | Open of Flower Id
    | Close of Flower Id
    | OpenTo of Flower Id
    | Speed of Flower Id
    | ChangeSpeed of Flower Id * uint
    | Acceleration of Flower Id
    | ChangeAcceleration of Flower Id * uint

let private serialPortView (selected: string option) dispatch =
    let ports = SerialPort.GetPortNames()


    Form.formElement
        {| Name = "Serial Port"
           Orientation = Orientation.Vertical
           Element =
               ComboBox.create [
                   ComboBox.dataItems ports
                   ComboBox.onSelectedIndexChanged
                       (fun index ->
                           match Array.tryItem index ports with
                           | Some port -> ChangePort port |> dispatch
                           | None -> ())
                   if Option.isSome selected then
                       ComboBox.selectedItem selected.Value
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
                Slider.minimum properties.Min
                Slider.maximum properties.Max
                Slider.value properties.Value
                Slider.onValueChanged (properties.OnChanged flowerId, SubPatchOptions.OnChangeOf flowerId)
            ]

        | None ->
            Slider.create [
                Slider.value properties.Value
                Slider.minimum properties.Min
                Slider.maximum properties.Max
                Slider.isEnabled false
                Slider.onValueChanged ((fun _ -> ()), SubPatchOptions.OnChangeOf Guid.Empty)
            ]

    Form.formElement
        {| Name = properties.Name
           Orientation = Orientation.Vertical
           Element = slider |}


let private openPercentageView (flowerOption: Flower option) (dispatch: Msg -> Unit) =
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

let private speedView (flowerOption: Flower option) (dispatch: Msg -> Unit) =
    sliderView
        { Name = "Speed"
          Value =
              Option.map Flower.speed flowerOption
              |> Option.defaultValue 0u
              |> float 
          Min = 0.
          Max = 10000.
          OnChanged =
              (fun flowerId newSpeed ->
                  ChangeSpeed(flowerId, uint newSpeed) |> dispatch)
          FlowerId = Option.map (fun flower -> flower.Id) flowerOption }

let private accelerationView (flowerOption: Flower option) (dispatch: Msg -> Unit) =
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
                  ChangeAcceleration(flowerId, uint newAcceleration) |> dispatch)
          FlowerId = Option.map (fun flower -> flower.Id) flowerOption }
        
let private iconButton name icon msg (flowerOption: Flower option) dispatch =
    match flowerOption with
    | Some flower ->
        Form.iconTextButton
            (icon Theme.palette.secondary)
            name
            Theme.palette.foreground
            (fun _ -> dispatch (msg flower.Id))
    | None ->
        Form.iconTextButton (icon Theme.palette.secondary) name Theme.palette.foreground (fun _ -> ())
        |> View.withAttr (Button.isEnabled false)

let view (flowerOption: Flower option) (port: string option) (dispatch: Msg -> Unit) =
    let children: IView list =
        [ Text.iconTitle (Icons.command Theme.palette.primary) "Commands" Theme.palette.foreground
          serialPortView port dispatch
          iconButton "Home" Icons.home Home flowerOption dispatch
          iconButton "Open" Icons.openIcon Open flowerOption dispatch
          iconButton "Close" Icons.close Close flowerOption dispatch
          iconButton "Open To" Icons.openTo OpenTo flowerOption dispatch
          openPercentageView flowerOption dispatch
          iconButton "Set Speed" Icons.speed Speed flowerOption dispatch
          speedView flowerOption dispatch
          iconButton "Set Acceleration" Icons.acceleration Acceleration flowerOption dispatch
          accelerationView flowerOption dispatch ]

    StackPanel.create [
        StackPanel.children children
        StackPanel.minWidth 200.
    ]
