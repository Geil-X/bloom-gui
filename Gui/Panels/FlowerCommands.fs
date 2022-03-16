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


let private openPercentageView (flowerOption: Flower option) (dispatch: Msg -> Unit) =
    let slider =
        match flowerOption with
        | Some flower ->
            Slider.create [
                Slider.minimum 0.
                Slider.maximum 100.
                Slider.value (ClampedPercentage.inPercentage flower.OpenPercent)
                Slider.onValueChanged (
                    (fun newPercent ->
                        ChangePercentage(flower.Id, ClampedPercentage.percent newPercent)
                        |> dispatch),
                    SubPatchOptions.OnChangeOf flower.Id
                )
            ]

        | None ->
            Slider.create [
                Slider.value 0.
                Slider.minimum 0.
                Slider.maximum 100.
                Slider.isEnabled false
                Slider.onValueChanged ((fun _ -> ()), SubPatchOptions.OnChangeOf Guid.Empty)
            ]

    Form.formElement
        {| Name = "Open Percentage"
           Orientation = Orientation.Vertical
           Element = slider |}

let private speedView (flowerOption: Flower option) (dispatch: Msg -> Unit) =
    let slider =
        match flowerOption with
        | Some flower ->
            Slider.create [
                Slider.minimum 0.
                Slider.maximum 10000.
                Slider.value (float flower.Speed)
                Slider.onValueChanged (
                    (fun newSpeed -> ChangeSpeed(flower.Id, uint newSpeed) |> dispatch),
                    SubPatchOptions.OnChangeOf flower.Id
                )
            ]

        | None ->
            Slider.create [
                Slider.value 0.
                Slider.minimum 0.
                Slider.maximum 10000.
                Slider.isEnabled false
                Slider.onValueChanged ((fun _ -> ()), SubPatchOptions.OnChangeOf Guid.Empty)
            ]

    Form.formElement
        {| Name = "Speed"
           Orientation = Orientation.Vertical
           Element = slider |}

let private accelerationView (flowerOption: Flower option) (dispatch: Msg -> Unit) =
    let slider =
        match flowerOption with
        | Some flower ->
            Slider.create [
                Slider.minimum 0.
                Slider.maximum 5000.
                Slider.value (float flower.Acceleration)
                Slider.onValueChanged (
                    (fun newAcceleration ->
                        ChangeAcceleration(flower.Id, uint newAcceleration)
                        |> dispatch),
                    SubPatchOptions.OnChangeOf flower.Id
                )
            ]

        | None ->
            Slider.create [
                Slider.value 0.
                Slider.minimum 0.
                Slider.maximum 5000.
                Slider.isEnabled false
                Slider.onValueChanged ((fun _ -> ()), SubPatchOptions.OnChangeOf Guid.Empty)
            ]

    Form.formElement
        {| Name = "Acceleration"
           Orientation = Orientation.Vertical
           Element = slider |}

let iconButton name icon msg (flowerOption: Flower option) dispatch =
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
    let children : IView list =
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
