module Gui.Views.Panels.FlowerProperties

open Avalonia.Controls
open Avalonia.FuncUI.DSL
open Avalonia.FuncUI.Types
open Avalonia.Layout
open Math.Geometry
open Math.Units

open Gui.DataTypes
open Gui.Views.Components

type Msg =
    | ChangeName of Flower Id * string
    | ChangeI2cAddress of Flower Id * string
    | Action of Action

// ---- Helper Functions ----

let disabledTextBox =
    TextBox.create [
        TextBox.text ""
        TextBox.isEnabled false
    ]


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
                TextBox.dock Dock.Left
                TextBox.onTextChanged (
                    (fun newI2cAddress ->
                        ChangeI2cAddress(flower.Id, newI2cAddress)
                        |> dispatch),
                    SubPatchOptions.OnChangeOf flower.Id
                )
            ]
        | None -> disabledTextBox

    let connectionStatus =
        match flowerOption with
        | Some flower ->
            match flower.ConnectionStatus with
            | Connected -> Icon.connected Icon.small Theme.palette.success
            | Disconnected -> Icon.disconnected Icon.small Theme.palette.danger
        | _ -> Icon.disconnected Icon.small Theme.palette.foregroundFaded
        |> View.withAttrs [
            Viewbox.dock Dock.Right
            Viewbox.margin (Theme.spacing.medium, 0., 0., 0.)
           ]

    Form.formElement
        {| Name = "I2C Address"
           Orientation = Orientation.Vertical
           Element =
            DockPanel.create [
                DockPanel.children [
                    connectionStatus
                    i2cTextBox
                ]
            ] |}


let private positionView (flowerOption: Flower option) =
    let rounded l =
        Float.roundFloatTo 2 (Length.inCssPixels l)

    let positionToString (position: Point2D<Meters, ScreenSpace>) =
        $"({rounded position.X}, {rounded position.Y})"

    let positionText =
        match flowerOption with
        | Some flower ->
            TextBlock.create [
                TextBlock.text (flower.Position |> positionToString)
            ]
        | None ->
            TextBlock.create [
                TextBlock.text "(___, ___)"
            ]


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
        | None ->
            TextBlock.create [
                TextBlock.text "0000000"
            ]

    Form.formElement
        {| Name = "Id"
           Orientation = Orientation.Horizontal
           Element = idText |}


let private flowerListing (flowers: Flower seq) (selected: Flower option) (dispatch: Action -> unit) =
    let sortedFlowers =
        Seq.sortBy Flower.i2cAddress flowers
        |> Array.ofSeq

    let flowerNames =
        Array.map Flower.name sortedFlowers

    let selectedIndex =
        Option.bind (fun flower -> Array.tryFindIndex (fun f -> f = flower) sortedFlowers) selected
        |> Option.defaultValue -1

    let selectionHandler (index: int) : unit =
        match Array.tryItem index sortedFlowers with
        | Some flower ->
            Action.SelectFlower flower.Id |> dispatch
        | None -> ()

    let selectionBox =
        ListBox.create [
            ListBox.dataItems flowerNames
            ListBox.selectedIndex selectedIndex
            ListBox.onSelectedIndexChanged (selectionHandler, SubPatchOptions.OnChangeOf(Array.length sortedFlowers))
        ]

    Form.formElement
        {| Name = "Flowers"
           Orientation = Orientation.Vertical
           Element = selectionBox |}


let view (flowers: Flower seq) (selectedFlower: Flower option) (dispatch: Msg -> unit) =
    let children: IView list =
        [ Text.iconTitle (Icon.flower Icon.large Theme.palette.primary) "Flower" Theme.palette.foreground
          nameView selectedFlower dispatch
          i2cAddressView selectedFlower dispatch
          positionView selectedFlower
          id selectedFlower
          flowerListing flowers selectedFlower (Action >> dispatch) ]

    StackPanel.create [
        StackPanel.children children
        StackPanel.minWidth 200.
    ]
