module Gui.Views.Panels.FlowerProperties

open System
open Avalonia.Controls
open Avalonia.FuncUI.DSL
open Avalonia.FuncUI.Types
open Avalonia.Layout
open Math.Geometry
open Math.Units

open Gui.DataTypes
open Gui.Views.Components

type Msg =
    | Action of Action

    | ChangeName of Flower Id * string
    | ChangeI2cAddress of Flower Id * string
    | ChangeOpenPercentage of Flower Id * Percent
    | ChangeTargetPercentage of Flower Id * Percent
    | ChangeSpeed of Flower Id * AngularSpeed
    | ChangeMaxSpeed of Flower Id * AngularSpeed
    | ChangeAcceleration of Flower Id * AngularAcceleration


// ---- Helper Functions ----

let rounded l =
    Float.roundFloatTo 2 (Length.inCssPixels l)

let disabledTextBox =
    TextBox.create [
        TextBox.text ""
        TextBox.isEnabled false
    ]

let presets =
    {| speedEmpty = AngularSpeed.microstepsPerSecond 0.
       minSpeed = AngularSpeed.microstepsPerSecond 0.
       maxSpeed = AngularSpeed.microstepsPerSecond 65000.
       accelerationEmpty = AngularAcceleration.microstepsPerSecondSquared 0.
       minAcceleration = AngularAcceleration.microstepsPerSecondSquared 0.
       maxAcceleration = AngularAcceleration.microstepsPerSecondSquared 10000. |}

// ---- Form Elements ----------------------------------------------------------

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

type DisabledSliderProperties<'Units> =
    { Name: string
      Value: Quantity<'Units>
      Min: Quantity<'Units>
      Max: Quantity<'Units>
      Display: Quantity<'Units> -> float
      Conversion: float -> Quantity<'Units>
      FlowerId: Flower.Id option }

let private sliderView (properties: DisabledSliderProperties<'Units>) =
    let slider =
        Slider.create [
            Slider.width 140.
            Slider.minimum (properties.Display properties.Min)
            Slider.maximum (properties.Display properties.Max)
            Slider.value (properties.Display properties.Value)
            Slider.isEnabled false
            Slider.dock Dock.Left
        ]

    let textInput =
        TextBlock.create [
            TextBlock.text (properties.Value |> properties.Display |> string)
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


let private openPercentageView (flowerOption: Flower option) =
    sliderView
        { Name = "Open Percentage"
          Value =
            Option.map (fun flower -> flower.OpenPercent) flowerOption
            |> Option.defaultValue Quantity.zero
          Min = Percent.decimal Percent.minimum
          Max = Percent.decimal Percent.maxDecimal
          Display = Percent.inPercentage >> Float.roundFloatTo 2
          Conversion = Percent.percent
          FlowerId = Option.map (fun flower -> flower.Id) flowerOption }

let private targetPercentageView (flowerOption: Flower option) =
    sliderView
        { Name = "Target Percentage"
          Value =
            Option.map (fun flower -> flower.TargetPercent) flowerOption
            |> Option.defaultValue Quantity.zero
          Min = Percent.decimal Percent.minimum
          Max = Percent.decimal Percent.maxDecimal
          Display = Percent.inPercentage >> Float.roundFloatTo 2
          Conversion = Percent.percent
          FlowerId = Option.map (fun flower -> flower.Id) flowerOption }


let private speedView (flowerOption: Flower option) =
    let maxSpeed =
        Option.map Flower.maxSpeed flowerOption
        |> Option.defaultValue presets.maxSpeed

    sliderView
        { Name = "Speed"
          Value =
            Option.map Flower.speed flowerOption
            |> Option.defaultValue presets.speedEmpty
          Min = -maxSpeed
          Max = maxSpeed
          Display =
            AngularSpeed.inMicrostepsPerSecond
            >> Float.roundFloatTo 2
          Conversion = AngularSpeed.microstepsPerSecond
          FlowerId = Option.map (fun flower -> flower.Id) flowerOption }

let private maxSpeedView (flowerOption: Flower option) =
    let maxSpeed: float =
        flowerOption
        |> Option.map Flower.maxSpeed
        |> Option.defaultValue AngularSpeed.zero
        |> AngularSpeed.inMicrostepsPerSecond
        |> Float.roundFloatTo 2

    Form.formElement
        {| Name = "Max Speed"
           Orientation = Orientation.Vertical
           Element =
            TextBlock.create [
                TextBlock.text $"{maxSpeed} u-Steps/s"
            ] |}

let private accelerationView (flowerOption: Flower option) =
    let acceleration: float =
        flowerOption
        |> Option.map Flower.acceleration
        |> Option.defaultValue AngularAcceleration.zero
        |> AngularAcceleration.inMicrostepsPerSecondSquared
        |> Float.roundFloatTo 2

    Form.formElement
        {| Name = "Acceleration"
           Orientation = Orientation.Vertical
           Element =
            TextBlock.create [
                TextBlock.text $"{acceleration} u-Steps/s²"
            ] |}



// ---- Multiple Flower Listing ------------------------------------------------

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
        | Some flower -> Action.SelectFlower flower.Id |> dispatch
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

// ---- Main View --------------------------------------------------------------

let view (flowers: Flower seq) (selectedFlower: Flower option) (dispatch: Msg -> unit) =
    let children: IView list =
        [ Text.iconTitle (Icon.flower Icon.large Theme.palette.primary) "Flower" Theme.palette.foreground
          nameView selectedFlower dispatch
          i2cAddressView selectedFlower dispatch
          positionView selectedFlower
          id selectedFlower
          openPercentageView selectedFlower
          targetPercentageView selectedFlower
          speedView selectedFlower
          maxSpeedView selectedFlower
          accelerationView selectedFlower
          flowerListing flowers selectedFlower (Action >> dispatch) ]

    StackPanel.create [
        StackPanel.children children
        StackPanel.minWidth Theme.size.medium
    ]
