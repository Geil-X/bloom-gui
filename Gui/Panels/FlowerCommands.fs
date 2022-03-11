module Gui.Panels.FlowerCommands

open Avalonia.Controls
open Avalonia.FuncUI.DSL
open Avalonia.FuncUI.Types
open Avalonia.Layout

open Gui.DataTypes
open Gui.Widgets
open Gui

type Msg = ChangePercentage of Flower.Id * ClampedPercentage

let private openPercentageView (flower: Flower.State) (dispatch: Msg -> Unit) =
    Form.formElement
        {| Name = "Open Percentage"
           Orientation = Orientation.Vertical
           Element =
               Slider.create [
                   Slider.minimum 0.
                   Slider.maximum 100.
                   Slider.value (ClampedPercentage.inPercentage flower.OpenPercent)
                   Slider.onValueChanged (
                       (fun newPercent ->
                           ChangePercentage(flower.Id, ClampedPercentage.percent newPercent)
                           |> dispatch),
                       SubPatchOptions.OnChangeOf(flower.Id)
                   )
               ] |}

let private selectedNone =
    Form.formElement
        {| Name = "Selected"
           Orientation = Orientation.Horizontal
           Element = TextBlock.create [ TextBlock.text "None" ] |}

let view (flowerOption: Flower.State option) (dispatch: Msg -> Unit) =
    let children: IView list =
        match flowerOption with
        | Some flower ->
            [ Text.iconTitle (Icons.command Theme.colors.offWhite) "Commands"
              openPercentageView flower dispatch
               ]
        | None -> [ selectedNone ]

    StackPanel.create [
        StackPanel.children children
        StackPanel.minWidth 200.
    ]
