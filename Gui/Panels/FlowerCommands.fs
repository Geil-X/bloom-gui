module Gui.Panels.FlowerCommands

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

let private commandButton name msg : IView =
    Button.create [ Button.name name; Button.onClick msg ]

let iconButton name icon msg dispatch =
    Form.iconTextButton (icon Theme.colors.offWhite) name (fun _ -> dispatch msg)

let view (flowerOption: Flower.State option) (dispatch: Msg -> Unit) =
    let children: IView list =
        match flowerOption with
        | Some flower ->
            [ Text.iconTitle (Icons.command Theme.colors.offWhite) "Commands"
              iconButton "Home" Icons.home (Home flower.Id) dispatch
              iconButton "Open" Icons.openIcon (Home flower.Id) dispatch
              iconButton "Close" Icons.close (Home flower.Id) dispatch
              iconButton "OpenTo" Icons.openTo (Home flower.Id) dispatch
              openPercentageView flower dispatch ]
        | None -> [ selectedNone ]

    StackPanel.create [
        StackPanel.children children
        StackPanel.minWidth 200.
    ]
