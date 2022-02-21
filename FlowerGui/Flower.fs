module FlowerGui.Flower

open Avalonia.FuncUI.Types
open Avalonia.Layout
open Elmish
open Avalonia.Controls
open Avalonia.FuncUI.DSL
open Avalonia.FuncUI
open Avalonia.FuncUI.Components.Hosts
open Avalonia.FuncUI.Elmish

open System
open Avalonia.Input
open Avalonia.Media
open Avalonia.Controls.Shapes
open Avalonia.FuncUI.DSL
open Geometry

open FlowerGui

type Id = Guid

type UiState =
    | Hovered
    | Selected
    | Pressed
    | Dragged

type State =
    { Id: Id
      Name: string
      I2cAddress: uint
      Position: Point2D<Pixels, UserSpace>
      Color: Color
      Radius: Length<Pixels>
      States: Set<UiState> }

type Msg =
    | PointerEnter
    | PointerLeave
    | PointerPressed of Events.MouseEvent<Pixels, UserSpace>
    | PointerReleased of Events.MouseEvent<Pixels, UserSpace>

// ---- Builders -----

let basic name =
    { Id = Guid.NewGuid()
      Name = name
      Position = Point2D.origin ()
      I2cAddress = 0u
      Color = Colors.White
      Radius = Length.pixels 20
      States = Set.empty }

// ---- Modifiers ----

let setName name flower : State = { flower with Name = name }
let setI2cAddress i2CAddress flower : State = { flower with I2cAddress = i2CAddress }
let setColor color flower : State = { flower with Color = color }
let setPosition position flower : State = { flower with Position = position }

let private addState state (flower: State) =
    { flower with
          States = Set.add state flower.States }

let private removeState state (flower: State) =
    { flower with
          States = Set.remove state flower.States }

let noStates (flower: State) : State = { flower with States = Set.empty }
let hover = addState Hovered
let unhover = removeState Hovered
let select = addState Selected
let deselect = removeState Selected

// ---- Accessors ----

let inState uiState state = Set.contains uiState state.States

// ---- Queries ----

let containsPoint point state =
    Circle2D.atPoint state.Position state.Radius
    |> Circle2D.containsPoint point

// ---- Updates ----

let update msg state =
    match msg with
    | PointerEnter -> hover state

    | PointerLeave -> unhover state

    | PointerPressed e ->
        printfn $"Pressed: {e.MouseButton} at {e.Position}"

        if e.MouseButton = MouseButton.Left
           && containsPoint e.Position state then
            Events.handle e

            state
        else
            state

    | PointerReleased e ->
        printfn $"Released: {e.MouseButton} at {e.Position}"

        if e.MouseButton = MouseButton.Left
           && containsPoint e.Position state then
            Events.handle e

            select state
        else
            deselect state


// ---- Drawing ----

let draw (flower: State) dispatch =
    let circle =
        Circle2D.atPoint flower.Position flower.Radius

    let fill =
        if Set.contains Hovered flower.States then
            Theme.palette.primaryLight
        else
            Theme.palette.primary

    let selection () =
        Draw.boundingBox
            (Circle2D.boundingBox circle)
            [ Rectangle.stroke Theme.colors.blue
              Rectangle.strokeThickness Theme.drawing.strokeWidth
              Rectangle.strokeDashArray Theme.drawing.dashArray ]



    let circle =
        Draw.circle
            circle
            [ Ellipse.fill fill
              Ellipse.strokeThickness Theme.drawing.strokeWidth
              Ellipse.onPointerEnter (fun _ -> dispatch PointerEnter)
              Ellipse.onPointerLeave (fun _ -> dispatch PointerLeave)
              Ellipse.onPointerPressed (
                  Events.pressedEvent Constants.CanvasId
                  >> PointerPressed
                  >> dispatch
              )
              Ellipse.onPointerReleased (
                  Events.releasedEvent Constants.CanvasId
                  >> PointerReleased
                  >> dispatch
              ) ]


    Canvas.create [
        Canvas.children [
            if inState Selected flower then
                selection ()
            circle
        ]
    ]
