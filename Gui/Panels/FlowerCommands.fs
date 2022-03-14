module Gui.Panels.FlowerCommands

open System
open Avalonia.Controls
open Avalonia.FuncUI.DSL
open Avalonia.FuncUI.Types
open Avalonia.Layout

open Gui.DataTypes
open Gui.Widgets
open Gui

type Msg =
    | ChangePercentage of Flower.Id * ClampedPercentage
    | Home of Flower.Id
    | Open of Flower.Id
    | Close of Flower.Id
    | OpenTo of Flower.Id

let private openPercentageView (flowerOption: Flower.State option) (dispatch: Msg -> Unit) =
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
                Slider.value 0
                Slider.minimum 0.
                Slider.maximum 100.
                Slider.isEnabled false
                Slider.onValueChanged ((fun _ -> ()), SubPatchOptions.OnChangeOf Guid.Empty)
            ]

    Form.formElement
        {| Name = "Open Percentage"
           Orientation = Orientation.Vertical
           Element = slider |}

let iconButton name icon msg (flowerOption: Flower.State option) dispatch =
    match flowerOption with
    | Some flower ->
        Form.iconTextButton
            (icon Theme.palette.secondary)
            name
            Theme.palette.foreground
            (fun _ -> dispatch (msg flower.Id))
    | None -> Form.iconTextButton (icon Theme.palette.secondary) name Theme.palette.foreground (fun _ -> ())

let view (flowerOption: Flower.State option) (dispatch: Msg -> Unit) =
    let children: IView list =
        [ Text.iconTitle (Icons.command Theme.palette.primary) "Commands" Theme.palette.foreground
          iconButton "Home" Icons.home Home flowerOption dispatch
          iconButton "Open" Icons.openIcon Open flowerOption dispatch
          iconButton "Close" Icons.close Close flowerOption dispatch
          iconButton "OpenTo" Icons.openTo OpenTo flowerOption dispatch
          openPercentageView flowerOption dispatch ]

    StackPanel.create [
        StackPanel.children children
        StackPanel.minWidth 200.
    ]
