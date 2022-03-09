module Gui.FlowerProperties

open Avalonia.Controls
open Avalonia.FuncUI.DSL
open Avalonia.FuncUI.Types
open Avalonia.Layout

open Geometry
open Gui.DataTypes
open Gui.Widgets
open Extensions

type Msg =
    | ChangeName of Flower.Id * string
    | ChangeI2cAddress of Flower.Id * string
    | ChangePercentage of Flower.Id * ClampedPercentage


let private nameView (flower: Flower.State) (dispatch: Msg -> Unit) =
    Form.formElement
        {| Name = "Name"
           Orientation = Orientation.Vertical
           Element =
               TextBox.create [
                   TextBox.text flower.Name
                   TextBox.onTextChanged (
                       (fun newName -> ChangeName(flower.Id, newName) |> dispatch),
                       SubPatchOptions.OnChangeOf flower.Id
                   )
               ] |}

let private i2cAddressView (flower: Flower.State) (dispatch: Msg -> Unit) =
    Form.formElement
        {| Name = "I2C Address"
           Orientation = Orientation.Vertical
           Element =
               TextBox.create [
                   TextBox.text (string flower.I2cAddress)
                   TextBox.onTextChanged (
                       (fun newAddress ->
                           ChangeI2cAddress(flower.Id, newAddress)
                           |> dispatch),
                       SubPatchOptions.OnChangeOf flower.Id
                   )
               ] |}

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

let private positionView (flower: Flower.State) =
    let rounded l = (Length.roundTo 0 l).value ()

    let positionToString (position: Point2D<Pixels, UserSpace>) =
        $"({rounded position.X}, {rounded position.Y})"


    Form.formElement
        {| Name = "Position"
           Orientation = Orientation.Horizontal
           Element =
               TextBlock.create [
                   TextBlock.text (flower.Position |> positionToString)
               ] |}

let private id (flower: Flower.State) =
    Form.formElement
        {| Name = "Id"
           Orientation = Orientation.Horizontal
           Element =
               TextBlock.create [
                   TextBlock.text (Guid.shortName flower.Id)
               ] |}

let private selectedNone =
    Form.formElement
        {| Name = "Selected"
           Orientation = Orientation.Horizontal
           Element = TextBlock.create [ TextBlock.text "None" ] |}

let view (flowerOption: Flower.State option) (dispatch: Msg -> Unit) =
    let properties: IView list =
        match flowerOption with
        | Some flower ->
            [ nameView flower dispatch
              i2cAddressView flower dispatch
              openPercentageView flower dispatch
              positionView flower
              id flower ]
        | None -> [ selectedNone ]

    StackPanel.create [
        StackPanel.children properties
        StackPanel.minWidth 200.
    ]
