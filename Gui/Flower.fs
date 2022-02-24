module Gui.Flower

open Avalonia.Controls
open Avalonia.FuncUI.DSL

open System
open Avalonia.Input
open Avalonia.Media
open Avalonia.Controls.Shapes
open Geometry

open Gui

type Id = Guid


type Attribute =
    | ViewState of ViewState
    | Event of Event

and ViewState =
    | Hovered
    | Selected
    | Pressed
    | Dragged

and Event =
    | OnEnter of (unit -> unit)
    | OnLeave of (unit -> unit)
    | OnPressed of (unit -> unit)
    | OnReleased of (unit -> unit)
    | OnMoved of (unit -> unit)

type State =
    { Id: Id
      Name: string
      I2cAddress: uint
      Position: Point2D<Pixels, UserSpace>
      Color: Color
      Radius: Length<Pixels> }


// ---- Builders -----

let basic name =
    { Id = Guid.NewGuid()
      Name = name
      Position = Point2D.origin ()
      I2cAddress = 0u
      Color = Colors.White
      Radius = Length.pixels 20 }


// ---- Modifiers ----

let setName name flower : State = { flower with Name = name }
let setI2cAddress i2CAddress flower : State = { flower with I2cAddress = i2CAddress }
let setColor color flower : State = { flower with Color = color }
let setPosition position flower : State = { flower with Position = position }


// ---- Queries ----

let containsPoint point state =
    Circle2D.atPoint state.Position state.Radius
    |> Circle2D.containsPoint point


// ---- Attributes ----

// States
let hovered = Hovered |> ViewState
let pressed = Pressed |> ViewState
let selected = Selected |> ViewState
let dragged = Dragged |> ViewState

// Events
let onEnter = OnEnter >> Event
let onLeave = OnLeave >> Event
let onMoved = OnLeave >> Event
let onPressed = OnPressed >> Event
let onReleased = OnReleased >> Event


// ---- Drawing ----

let draw (flower: State) (attributes: Attribute list) =
    let onPointerPressed (e: PointerPressedEventArgs) (msg: unit -> unit) : Unit =
        e
        |> (Events.pressedEvent Constants.CanvasId)
        |> (fun e ->
            if e.MouseButton = MouseButton.Left
               && containsPoint e.Position flower then
                Events.handle e
                msg ())

    let onPointerReleased (e: PointerReleasedEventArgs) (msg: unit -> unit) : unit =
        e
        |> (Events.releasedEvent Constants.CanvasId)
        |> (fun e ->
            if e.MouseButton = MouseButton.Left
               && containsPoint e.Position flower then
                Events.handle e
                msg ())

    let circleAttributes =
        List.map
            (fun attribute ->
                match attribute with
                | ViewState viewState ->
                    match viewState with
                    | Hovered -> Ellipse.fill Theme.palette.primaryLight
                    | Pressed -> Ellipse.fill Theme.palette.primaryLightest
                    | Selected -> Ellipse.isVisible true
                    | Dragged -> (Ellipse.fill Theme.palette.primaryDark)
                | Event event ->
                    match event with
                    | OnMoved movedMsg -> Ellipse.onPointerMoved (fun _ -> movedMsg ())
                    | OnEnter enterMsg -> Ellipse.onPointerEnter (fun _ -> enterMsg ())
                    | OnLeave leaveMsg -> Ellipse.onPointerLeave (fun _ -> leaveMsg ())
                    | OnPressed pressedMsg -> Ellipse.onPointerPressed (fun e -> (onPointerPressed e pressedMsg))
                    | OnReleased releasedMessage ->
                        Ellipse.onPointerReleased (fun e -> (onPointerReleased e releasedMessage)))
            attributes

    let circle =
        Circle2D.atPoint flower.Position flower.Radius

    let selection =
        if List.exists
            (fun e ->
                match e with
                | ViewState Selected -> true
                | _ -> false)
            attributes then

            Draw.boundingBox
                (Circle2D.boundingBox circle)
                [ Rectangle.stroke Theme.colors.blue
                  Rectangle.strokeThickness Theme.drawing.strokeWidth
                  Rectangle.strokeDashArray Theme.drawing.dashArray ]
            |> Some
        else
            None


    let circle =
        Draw.circle
            circle
            (circleAttributes
             @ [ Ellipse.strokeThickness Theme.drawing.strokeWidth
                 Ellipse.fill Theme.palette.primary ])

    Canvas.create [
        Canvas.children [
            yield! selection |> Option.toList
            circle
        ]
    ]
