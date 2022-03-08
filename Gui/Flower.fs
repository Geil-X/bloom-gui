module Gui.Flower

open Avalonia.Controls
open Avalonia.FuncUI.DSL
open System
open Avalonia.Media
open Avalonia.Controls.Shapes
open Geometry
open MBrace.FsPickler.Json

open Gui
open Extensions

type Id = uint

type Attribute<'Unit, 'Coordinates> =
    // States
    | Hovered
    | Selected
    | Pressed
    | Dragged

    // Events
    | OnPointerEnter of (Id * MouseEvent<'Unit, 'Coordinates> -> unit)
    | OnPointerLeave of (Id * MouseEvent<'Unit, 'Coordinates> -> unit)
    | OnPointerMoved of (Id * MouseEvent<'Unit, 'Coordinates> -> unit)
    | OnPointerPressed of (Id * MouseButtonEvent<'Unit, 'Coordinates> -> unit)
    | OnPointerReleased of (Id * MouseButtonEvent<'Unit, 'Coordinates> -> unit)

type State =
    { Id: Id
      Name: string
      I2cAddress: uint
      Position: Point2D<Pixels, UserSpace>
      Color: Color
      Radius: Length<Pixels> }


// ---- Builders -----

let mutable index = 0u

let basic name =
    index <- index + 1u

    { Id = index
      Name = name
      Position = Point2D.origin ()
      I2cAddress = 0u
      Color = Colors.White
      Radius = Length.pixels 20. }

// ---- Accessors ----

let name flower = flower.Position
let i2cAddress flower = flower.I2cAddress
let color flower = flower.Color
let position flower = flower.Position

// ---- Modifiers ----

let setName name flower : State = { flower with Name = name }
let setI2cAddress i2CAddress flower : State = { flower with I2cAddress = i2CAddress }
let setColor color flower : State = { flower with Color = color }
let setPosition position flower : State = { flower with Position = position }


// ---- Queries ----

let containsPoint point state =
    Circle2D.atPoint state.Position state.Radius
    |> Circle2D.containsPoint point

// ---- Serialization & Deserialization ----

let jsonSerializer =
    FsPickler.CreateJsonSerializer(indent = false)

let serialize (stream: IO.TextWriter) (flower: State seq) : unit =
    jsonSerializer.Serialize(stream, flower)

let deserialize (flowerStream: IO.TextReader) : State seq =
    jsonSerializer.Deserialize<State seq>(flowerStream)


// ---- Attributes ----

// States
let hovered = Attribute.Hovered
let pressed = Attribute.Pressed
let selected = Attribute.Selected
let dragged = Attribute.Dragged

// Events
let onPointerEnter = Attribute.OnPointerEnter
let onPointerLeave = Attribute.OnPointerLeave
let onPointerPressed = Attribute.OnPointerPressed
let onPointerReleased = Attribute.OnPointerReleased
let onPointerMoved = Attribute.OnPointerMoved



// ---- Drawing ----

let draw (flower: State) (attributes: Attribute<'Unit, 'Coordinates> list) =
    let circleAttributes =
        List.map
            (fun attribute ->
                match attribute with
                | Hovered -> Ellipse.fill Theme.palette.primaryLight |> Some
                | Pressed -> Ellipse.fill Theme.palette.primaryLightest |> Some
                | Selected -> None
                | Dragged -> (Ellipse.fill Theme.palette.primaryDark) |> Some

                | OnPointerEnter enterMsg ->
                    Ellipse.onPointerEnter (
                        Events.pointerEnter Constants.CanvasId
                        >> Option.map (fun e -> enterMsg (flower.Id, e))
                        >> Option.defaultValue ()
                    )
                    |> Some

                | OnPointerLeave leaveMsg ->
                    Ellipse.onPointerLeave (
                        Events.pointerLeave Constants.CanvasId
                        >> Option.map (fun e -> leaveMsg (flower.Id, e))
                        >> Option.defaultValue ()
                    )
                    |> Some

                | OnPointerMoved movedMsg ->
                    Ellipse.onPointerMoved (
                        Events.pointerMoved Constants.CanvasId
                        >> Option.map (fun e -> movedMsg (flower.Id, e))
                        >> Option.defaultValue ()
                    )
                    |> Some

                | OnPointerPressed pressedMsg ->
                    Ellipse.onPointerPressed (
                        Events.pointerPressed Constants.CanvasId
                        >> Option.map (fun e -> pressedMsg (flower.Id, e))
                        >> Option.defaultValue ()
                    )
                    |> Some

                | OnPointerReleased releasedMsg ->
                    Ellipse.onPointerReleased (
                        Events.pointerReleased Constants.CanvasId
                        >> Option.map (fun e -> releasedMsg (flower.Id, e))
                        >> Option.defaultValue ()
                    )
                    |> Some)

            attributes
        |> List.filterNone

    let circle =
        Circle2D.atPoint flower.Position flower.Radius

    let selection =
        if List.exists
            (fun e ->
                match e with
                | Selected -> true
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
            
    let name =
        TextBlock.create [
            TextBlock.text flower.Name
            TextBlock.left (flower.Position.X.value() - 20.)
            TextBlock.top (flower.Position.Y.value() - 35.)
        ]
            
    let circle =
        Draw.circle
            circle
            (circleAttributes
             @ [ Ellipse.strokeThickness Theme.drawing.strokeWidth
                 Ellipse.fill Theme.palette.primary ])

    Canvas.create [
        Canvas.children [
            circle
            yield! selection |> Option.toList
            name
        ]
    ]
