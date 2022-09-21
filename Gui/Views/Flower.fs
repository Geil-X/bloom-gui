module Gui.Views.Flower

open Avalonia
open Avalonia.Controls
open Avalonia.Controls.Shapes
open Avalonia.FuncUI.DSL
open Avalonia.FuncUI.Types
open Avalonia.Input
open Avalonia.Media
open Math.Geometry
open Math.Units

open Gui.DataTypes
open Extensions


/// An individual petal portion of the flower. This petal is just the visible part of the petal and doesn't contain any
/// of the masking layers for the lower petals.
let private petal (basePoint: Point2D<Meters, ScreenSpace>) (angle: Angle) (width: Length) (height: Length) =
    let localRotationOrigin =
        RelativePoint(Length.inCssPixels width / 2., Length.inCssPixels height, RelativeUnit.Absolute)

    Ellipse.create [
        Ellipse.fill Theme.palette.primary
        Ellipse.width (Length.inCssPixels width)
        Ellipse.height (Length.inCssPixels height)
        Ellipse.top (Length.inCssPixels basePoint.Y)
        Ellipse.left (Length.inCssPixels (basePoint.X - (width / 2.)))
        Ellipse.renderTransformOrigin localRotationOrigin
        Ellipse.renderTransform (RotateTransform.inDegrees (Angle.inDegrees angle))
    ]

/// The whole flower icon that is displayed in the simulation space. The flower is made up of several petals which are
/// used to show how open the flower is.
let private icon (flower: Flower) : IView list =
    let bbox =
        Circle2D.boundingBox (Flower.circle flower)

    let width = BoundingBox2D.width bbox / 2.
    let height = BoundingBox2D.height bbox

    let basePoint =
        (BoundingBox2D.centerPoint bbox)
        - Vector2D.xy Quantity.zero (height / 2.)
    
    let minAngle = Angle.degrees 20.
    let maxAngle = Angle.degrees 70.
    let percentage = Percent.inDecimal (Flower.openPercent flower)
    
    let angle = Angle.interpolateFrom  minAngle maxAngle percentage

    [ petal basePoint -angle width height
      petal basePoint Angle.zero width height
      petal basePoint angle width height ]


let private flowerView (flower: Flower) (attributes: Flower.Attribute list) : IView<Circle> =
    let fadedColor =
        Theme.palette.primary
        |> Color.hex
        |> Color.desaturate 0.15
        |> Color.lighten 0.1

    let hovered () = Theme.lighter fadedColor |> string
    let pressed () = Theme.lightest fadedColor |> string
    let dragged () = Theme.fade fadedColor |> string

    let circleAttributes =
        List.map
            (fun attribute ->
                match attribute with
                | Flower.Hovered -> hovered () |> Circle.fill |> Some
                | Flower.Pressed -> pressed () |> Circle.fill |> Some
                | Flower.Selected -> None
                | Flower.Dragged -> dragged () |> Circle.fill |> Some

                | Flower.OnPointerEnter enterMsg ->
                    Circle.onPointerEnter (
                        Event.pointerEnter Constants.CanvasId
                        >> Option.map (fun e -> enterMsg (flower.Id, e))
                        >> Option.defaultValue (),
                        SubPatchOptions.OnChangeOf flower.Id
                    )
                    |> Some

                | Flower.OnPointerLeave leaveMsg ->
                    Circle.onPointerLeave (
                        Event.pointerLeave Constants.CanvasId
                        >> Option.map (fun e -> leaveMsg (flower.Id, e))
                        >> Option.defaultValue (),
                        SubPatchOptions.OnChangeOf flower.Id
                    )
                    |> Some

                | Flower.OnPointerMoved movedMsg ->
                    Circle.onPointerMoved (
                        Event.pointerMoved Constants.CanvasId
                        >> Option.map (fun e -> movedMsg (flower.Id, e))
                        >> Option.defaultValue (),
                        SubPatchOptions.OnChangeOf flower.Id
                    )
                    |> Some

                | Flower.OnPointerPressed pressedMsg ->
                    Circle.onPointerPressed (
                        Event.pointerPressed Constants.CanvasId
                        >> Option.map (fun e -> pressedMsg (flower.Id, e))
                        >> Option.defaultValue (),
                        SubPatchOptions.OnChangeOf flower.Id
                    )
                    |> Some

                | Flower.OnPointerReleased releasedMsg ->
                    Circle.onPointerReleased (
                        Event.pointerReleased Constants.CanvasId
                        >> Option.map (fun e -> releasedMsg (flower.Id, e))
                        >> Option.defaultValue (),
                        SubPatchOptions.OnChangeOf flower.Id
                    )
                    |> Some)

            attributes
        |> List.filterNone


    Circle.from
        (Flower.circle flower)
        (circleAttributes
         @ [ Circle.strokeThickness Theme.drawing.strokeWidth
             Circle.fill (string fadedColor) ])

let private selectionView (circle: Circle2D<Meters, ScreenSpace>) (attributes: Flower.Attribute list) =
    if
        List.exists
            (fun e ->
                match e with
                | Flower.Selected -> true
                | _ -> false)
            attributes then

        Rectangle.fromBoundingBox
            (Circle2D.boundingBox circle)
            [ Rectangle.stroke Theme.colors.blue
              Rectangle.strokeThickness Theme.drawing.strokeWidth
              Rectangle.strokeDashArray Theme.drawing.dashArray ]
        |> Some
    else
        None

let private nameView (flower: Flower) =
    TextBlock.create [
        TextBlock.text flower.Name
        TextBlock.left (Length.inCssPixels flower.Position.X - 20.)
        TextBlock.top (Length.inCssPixels flower.Position.Y - 35.)
    ]

// ---- Drawing ----------------------------------------------------------------

let private canvasEvent (flowerId: Flower Id) (attribute: Flower.Attribute) =
    match attribute with
    | Flower.Hovered -> None
    | Flower.Pressed -> None
    | Flower.Selected -> None
    | Flower.Dragged -> None

    | Flower.OnPointerEnter enterMsg ->
        Circle.onPointerEnter (
            Event.pointerEnter Constants.CanvasId
            >> Option.map (fun e -> enterMsg (flowerId, e))
            >> Option.defaultValue (),
            SubPatchOptions.OnChangeOf flowerId
        )
        |> Some

    | Flower.OnPointerLeave leaveMsg ->
        Circle.onPointerLeave (
            Event.pointerLeave Constants.CanvasId
            >> Option.map (fun e -> leaveMsg (flowerId, e))
            >> Option.defaultValue (),
            SubPatchOptions.OnChangeOf flowerId
        )
        |> Some

    | Flower.OnPointerMoved movedMsg ->
        Circle.onPointerMoved (
            Event.pointerMoved Constants.CanvasId
            >> Option.map (fun e -> movedMsg (flowerId, e))
            >> Option.defaultValue (),
            SubPatchOptions.OnChangeOf flowerId
        )
        |> Some

    | Flower.OnPointerPressed pressedMsg ->
        Circle.onPointerPressed (
            Event.pointerPressed Constants.CanvasId
            >> Option.map (fun e -> pressedMsg (flowerId, e))
            >> Option.defaultValue (),
            SubPatchOptions.OnChangeOf flowerId
        )
        |> Some

    | Flower.OnPointerReleased releasedMsg ->
        Circle.onPointerReleased (
            Event.pointerReleased Constants.CanvasId
            >> Option.map (fun e -> releasedMsg (flowerId, e))
            >> Option.defaultValue (),
            SubPatchOptions.OnChangeOf flowerId
        )
        |> Some

/// The main view for drawing a flower
let view (flower: Flower) (attributes: Flower.Attribute list) =
    let selection =
        selectionView (Flower.circle flower) attributes
        |> Option.map (fun a -> a :> IView)
        |> Option.toList

    let canvasAttributes =
        List.map (canvasEvent flower.Id) attributes
        |> List.filterNone


    Canvas.create [
        Canvas.children [
            yield! icon flower
            yield! selection
            nameView flower
        ]
        yield! canvasAttributes
    ]
