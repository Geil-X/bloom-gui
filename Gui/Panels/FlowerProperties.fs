module Gui.FlowerProperties

open Avalonia.Controls
open Avalonia.FuncUI.DSL
open Avalonia.FuncUI.Types
open Avalonia.Layout

open Geometry
open Gui.DataTypes
open Gui.Widgets

type Msg =
    | ChangeName of Flower Id * string
    | ChangeI2cAddress of Flower Id * string

// ---- Helper Functions ----

let disabledTextBox =
    TextBox.create [ TextBox.text ""; TextBox.isEnabled false ]


// ---- Form Elements ----

let private nameView (flowerOption: Flower option) (dispatch: Msg -> unit) =
    let nameTextBox =
        match flowerOption with
        | Some flower ->
            TextBox.create [
                TextBox.text flower.Name
                TextBox.onTextChanged (
                    (fun newName -> ChangeName(flower.Id, newName) |> dispatch),
                    SubPatchOptions.OnChangeOf flower.Id
                )
            ]
        | None -> disabledTextBox


    Form.formElement
        {| Name = "Name"
           Orientation = Orientation.Vertical
           Element = nameTextBox |}

let private i2cAddressView (flowerOption: Flower option) (dispatch: Msg -> unit) =
    let i2cTextBox =
        match flowerOption with
        | Some flower ->
            TextBox.create [
                TextBox.text (string flower.I2cAddress)
                TextBox.onTextChanged (
                    (fun newI2cAddress -> ChangeI2cAddress(flower.Id, newI2cAddress) |> dispatch),
                    SubPatchOptions.OnChangeOf flower.Id
                )
            ]
        | None -> disabledTextBox

    Form.formElement
        {| Name = "I2C Address"
           Orientation = Orientation.Vertical
           Element = i2cTextBox |}

let private positionView (flowerOption: Flower option) =
    let rounded l = (Length.roundTo 0 l).value ()

    let positionToString (position: Point2D<Pixels, UserSpace>) =
        $"({rounded position.X}, {rounded position.Y})"

    let positionText =
        match flowerOption with
        | Some flower ->
            TextBlock.create [
                TextBlock.text (flower.Position |> positionToString)
            ]
        | None -> TextBlock.create [ TextBlock.text "(___, ___)" ]


    Form.formElement
        {| Name = "Position"
           Orientation = Orientation.Horizontal
           Element = positionText |}

let private id (flowerOption: Flower option) =
    let idText =
        match flowerOption with
        | Some flower ->
            TextBlock.create [
                TextBlock.text (Id.shortName flower.Id)
            ]
        | None -> TextBlock.create [ TextBlock.text "0000000" ]

    Form.formElement
        {| Name = "Id"
           Orientation = Orientation.Horizontal
           Element = idText |}

let view (flowerOption: Flower option) (dispatch: Msg -> unit) =
    let children: IView list =
        [ Text.iconTitle (Icon.flower Icon.large Theme.palette.primary) "Flower" Theme.palette.foreground
          nameView flowerOption dispatch
          i2cAddressView flowerOption dispatch
          positionView flowerOption
          id flowerOption ]

    StackPanel.create [
        StackPanel.children children
        StackPanel.minWidth 200.
    ]
