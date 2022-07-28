module Gui.DataTypes.Flower

open Avalonia.Media
open Geometry
open Avalonia.Controls
open Avalonia.FuncUI.DSL
open Avalonia.Controls.Shapes

open Gui
open Gui.DataTypes
open Extensions

type Id = Flower Id

type Attribute =
    // Flowers
    | Hovered
    | Selected
    | Pressed
    | Dragged

    // Events
    | OnPointerEnter of (Flower Id * MouseEvent<Pixels, UserSpace> -> unit)
    | OnPointerLeave of (Flower Id * MouseEvent<Pixels, UserSpace> -> unit)
    | OnPointerMoved of (Flower Id * MouseEvent<Pixels, UserSpace> -> unit)
    | OnPointerPressed of (Flower Id * MouseButtonEvent<Pixels, UserSpace> -> unit)
    | OnPointerReleased of (Flower Id * MouseButtonEvent<Pixels, UserSpace> -> unit)



// ---- Builders -----

/// The first 8 Addresses are reserved so the starting address must be the
/// 9th address.
let mutable private initialI2cAddress = 16uy

let basic name : Flower =
    initialI2cAddress <- initialI2cAddress + 1uy

    { Id = Id.create ()
      Name = name
      Position = Point2D.origin ()
      I2cAddress = initialI2cAddress
      Color = Color.hex Theme.palette.primary
      OpenPercent = ClampedPercentage.zero
      TargetPercent = ClampedPercentage.zero
      MaxSpeed = 5000
      Acceleration = 1000
      Radius = Length.pixels 20.
      ConnectionStatus = Disconnected }

// ---- Accessors ----

let name (flower: Flower) : string = flower.Name
let i2cAddress (flower: Flower) : I2cAddress = flower.I2cAddress
let color (flower: Flower) : Color = flower.Color
let position (flower: Flower) : Point2D<Pixels, UserSpace> = flower.Position
let openPercent (flower: Flower) : ClampedPercentage = flower.OpenPercent
let targetPercent (flower: Flower) : ClampedPercentage = flower.TargetPercent
let maxSpeed (flower: Flower) : Speed = flower.MaxSpeed
    
let acceleration (flower: Flower) : Acceleration = flower.Acceleration

// ---- Modifiers ----

let setName name flower : Flower = { flower with Name = name }
let setI2cAddress i2CAddress flower : Flower = { flower with I2cAddress = i2CAddress }
let setColor color flower : Flower = { flower with Color = color }
let setPosition position flower : Flower = { flower with Position = position }
let setOpenPercent percent flower : Flower = { flower with OpenPercent = percent }
let setTargetPercent percent flower : Flower = { flower with TargetPercent = percent }
let setMaxSpeed speed flower : Flower = { flower with MaxSpeed = speed }
let connected flower : Flower = { flower with ConnectionStatus = Connected }
let disconnected flower : Flower = { flower with ConnectionStatus = Disconnected }

let setAcceleration acceleration flower : Flower =
    { flower with
          Acceleration = acceleration }


// ---- Queries ----

let containsPoint point (state: Flower) =
    Circle2D.atPoint state.Position state.Radius
    |> Circle2D.containsPoint point
    
    
// ---- Attributes ----

// Flowers
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

let outerCircle (flower: Flower) (circle: Circle2D<Pixels, UserSpace>) (attributes: Attribute list) =
    let fadedColor =
        flower.Color
        |> Color.desaturate 0.15
        |> Color.lighten 0.1

    let hovered () = Theme.lighter fadedColor |> string
    let pressed () = Theme.lightest fadedColor |> string
    let dragged () = Theme.fade fadedColor |> string

    let circleAttributes =
        List.map
            (fun attribute ->
                match attribute with
                | Hovered -> hovered () |> Ellipse.fill |> Some
                | Pressed -> pressed () |> Ellipse.fill |> Some
                | Selected -> None
                | Dragged -> dragged () |> Ellipse.fill |> Some

                | OnPointerEnter enterMsg ->
                    Ellipse.onPointerEnter (
                        Events.pointerEnter Constants.CanvasId
                        >> Option.map (fun e -> enterMsg (flower.Id, e))
                        >> Option.defaultValue (),
                        SubPatchOptions.OnChangeOf flower.Id
                    )
                    |> Some

                | OnPointerLeave leaveMsg ->
                    Ellipse.onPointerLeave (
                        Events.pointerLeave Constants.CanvasId
                        >> Option.map (fun e -> leaveMsg (flower.Id, e))
                        >> Option.defaultValue (),
                        SubPatchOptions.OnChangeOf flower.Id
                    )
                    |> Some

                | OnPointerMoved movedMsg ->
                    Ellipse.onPointerMoved (
                        Events.pointerMoved Constants.CanvasId
                        >> Option.map (fun e -> movedMsg (flower.Id, e))
                        >> Option.defaultValue (),
                        SubPatchOptions.OnChangeOf flower.Id
                    )
                    |> Some

                | OnPointerPressed pressedMsg ->
                    Ellipse.onPointerPressed (
                        Events.pointerPressed Constants.CanvasId
                        >> Option.map (fun e -> pressedMsg (flower.Id, e))
                        >> Option.defaultValue (),
                        SubPatchOptions.OnChangeOf flower.Id
                    )
                    |> Some

                | OnPointerReleased releasedMsg ->
                    Ellipse.onPointerReleased (
                        Events.pointerReleased Constants.CanvasId
                        >> Option.map (fun e -> releasedMsg (flower.Id, e))
                        >> Option.defaultValue (),
                        SubPatchOptions.OnChangeOf flower.Id
                    )
                    |> Some)

            attributes
        |> List.filterNone


    Draw.circle
        circle
        (circleAttributes
         @ [ Ellipse.strokeThickness Theme.drawing.strokeWidth
             Ellipse.fill (string fadedColor) ])

let innerCircle (flower: Flower) (circle: Circle2D<Pixels, UserSpace>) (attributes: Attribute list) =
    let innerRadius =
        circle.Radius
        * ClampedPercentage.inDecimal flower.OpenPercent

    let hovered () = Theme.lighter flower.Color |> string
    let pressed () = Theme.lightest flower.Color |> string
    let dragged () = Theme.fade flower.Color |> string

    let circleAttributes =
        List.map
            (fun attribute ->
                match attribute with
                | Hovered -> hovered () |> Ellipse.fill |> Some
                | Pressed -> pressed () |> Ellipse.fill |> Some
                | Dragged -> dragged () |> Ellipse.fill |> Some
                | Selected -> None
                | OnPointerEnter enterMsg ->
                    Ellipse.onPointerEnter (
                        Events.pointerEnter Constants.CanvasId
                        >> Option.map (fun e -> enterMsg (flower.Id, e))
                        >> Option.defaultValue (),
                        SubPatchOptions.OnChangeOf flower.Id
                    )
                    |> Some

                | OnPointerLeave leaveMsg ->
                    Ellipse.onPointerLeave (
                        Events.pointerLeave Constants.CanvasId
                        >> Option.map (fun e -> leaveMsg (flower.Id, e))
                        >> Option.defaultValue (),
                        SubPatchOptions.OnChangeOf flower.Id
                    )
                    |> Some

                | OnPointerMoved movedMsg ->
                    Ellipse.onPointerMoved (
                        Events.pointerMoved Constants.CanvasId
                        >> Option.map (fun e -> movedMsg (flower.Id, e))
                        >> Option.defaultValue (),
                        SubPatchOptions.OnChangeOf flower.Id
                    )
                    |> Some

                | OnPointerPressed pressedMsg ->
                    Ellipse.onPointerPressed (
                        Events.pointerPressed Constants.CanvasId
                        >> Option.map (fun e -> pressedMsg (flower.Id, e))
                        >> Option.defaultValue (),
                        SubPatchOptions.OnChangeOf flower.Id
                    )
                    |> Some

                | OnPointerReleased releasedMsg ->
                    Ellipse.onPointerReleased (
                        Events.pointerReleased Constants.CanvasId
                        >> Option.map (fun e -> releasedMsg (flower.Id, e))
                        >> Option.defaultValue (),
                        SubPatchOptions.OnChangeOf flower.Id
                    )
                    |> Some)
            attributes
        |> List.filterNone

    Draw.circle
        (Circle2D.withRadius innerRadius circle.Center)
        (circleAttributes
         @ [ Ellipse.strokeThickness Theme.drawing.strokeWidth
             Ellipse.fill (string flower.Color) ])

let selection (circle: Circle2D<Pixels, UserSpace>) (attributes: Attribute list) =
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

let nameTag (flower: Flower) =
    TextBlock.create [ TextBlock.text flower.Name
                       TextBlock.left (flower.Position.X.value () - 20.)
                       TextBlock.top (flower.Position.Y.value () - 35.) ]


// ---- Drawing ----

let draw (flower: Flower) (attributes: Attribute list) =
    let circle =
        Circle2D.atPoint flower.Position flower.Radius

    Canvas.create [ Canvas.children [ outerCircle flower circle attributes
                                      innerCircle flower circle attributes

                                      yield! selection circle attributes |> Option.toList

                                      nameTag flower ] ]
