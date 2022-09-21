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

/// Determines the petal color from a list of attributes. The color is chosen by checking if any of the attributes have
/// a color and picking the last color in that list that applies to the flower.
let private petalColor attributes : string =
    let fadedColor =
        Theme.palette.primary
        |> Color.hex
        |> Color.desaturate 0.15
        |> Color.lighten 0.1

    let baseColor =
        Theme.lighter fadedColor |> string

    let petalColorHelper attribute =
        match attribute with
        | Flower.Hovered -> Theme.lighter fadedColor |> string |> Some
        | Flower.Pressed -> Theme.lightest fadedColor |> string |> Some
        | Flower.Selected -> Theme.fade fadedColor |> string |> Some
        | Flower.Dragged -> None
        | Flower.OnPointerEnter _ -> None
        | Flower.OnPointerLeave _ -> None
        | Flower.OnPointerMoved _ -> None
        | Flower.OnPointerPressed _ -> None
        | Flower.OnPointerReleased _ -> None

    List.fold
        (fun petalColor attr ->
            petalColorHelper attr
            |> Option.defaultValue petalColor)
        baseColor
        attributes


/// An individual petal portion of the flower. This petal is just the visible part of the petal and doesn't contain any
/// of the masking layers for the lower petals.
let private petalView
    (width: Length)
    (height: Length)
    (color: string)
    (angle: Angle)
    =
    let localRotationOrigin =
        RelativePoint(Length.inCssPixels width / 2., Length.inCssPixels height, RelativeUnit.Absolute)

    Ellipse.create [
        Ellipse.fill color
        Ellipse.width (Length.inCssPixels width)
        Ellipse.height (Length.inCssPixels height)
        Ellipse.renderTransformOrigin localRotationOrigin
        Ellipse.renderTransform (RotateTransform.inDegrees (Angle.inDegrees angle))
    ]
    
let private selectedView (bbox: BoundingBox2D<Meters, ScreenSpace>): IView =
    Rectangle.fromBoundingBox bbox [
        Rectangle.stroke Theme.palette.info
        Rectangle.strokeThickness Theme.drawing.strokeWidth
        Rectangle.strokeDashArray Theme.drawing.dashArray
    ]

/// The whole flower icon that is displayed in the simulation space. The flower is made up of several petals which are
/// used to show how open the flower is.
let private iconView (flower: Flower) (attributes: Flower.Attribute list) : IView =
    let width = flower.Radius 
    let height = flower.Radius * 2.

    let boundingBox =
        BoundingBox2D.fromExtrema {
            MinX = -flower.Radius * 0.5
            MaxX = flower.Radius * 1.5
            MinY = Quantity.zero
            MaxY = flower.Radius * 2.
        }
    
    let minAngle = Angle.degrees 20.
    let maxAngle = Angle.degrees 70.

    let percentage =
        Percent.inDecimal (Flower.openPercent flower)

    let angle =
        Angle.interpolateFrom minAngle maxAngle percentage

    let color = petalColor attributes

    let petalRenderer =
        petalView width height color
        
    let translation =
        Vector2D.from Point2D.origin flower.Position
        
    let isSelected =
        List.exists
            (fun e ->
                match e with
                | Flower.Selected -> true
                | _ -> false)
            attributes

    Canvas.create [
        Canvas.children [
            petalRenderer -angle
            petalRenderer Angle.zero
            petalRenderer angle
            if isSelected then selectedView boundingBox
        ]
        Canvas.renderTransform (TranslateTransform.inCssPixels translation)
    ]
    :> IView


let private nameView (flower: Flower) =
    TextBlock.create [
        TextBlock.text flower.Name
        TextBlock.left (Length.inCssPixels flower.Position.X - 20.)
        TextBlock.top (Length.inCssPixels flower.Position.Y - 35.)
    ]

// ---- Events -----------------------------------------------------------------

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
        
// ---- Flower View ------------------------------------------------------------

/// The main view for drawing a flower
let view (flower: Flower) (attributes: Flower.Attribute list) =
    let canvasAttributes =
        List.map (canvasEvent flower.Id) attributes
        |> List.filterNone

    Canvas.create [
        Canvas.children [
            nameView flower
            iconView flower attributes
        ]
        yield! canvasAttributes
    ]
