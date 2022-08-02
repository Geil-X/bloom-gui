module Gui.Views.Flower

open Avalonia.Controls
open Avalonia.Controls.Shapes
open Avalonia.FuncUI.DSL
open Avalonia.Input
open Avalonia.Media
open Geometry

open Gui.DataTypes
open Gui.DataTypes.Flower
open Extensions

let outerCircle (flower: Flower) (circle: Circle2D<Pixels, UserSpace>) (attributes: Attribute list) =
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
                | Hovered -> hovered () |> Circle.fill |> Some
                | Pressed -> pressed () |> Circle.fill |> Some
                | Selected -> None
                | Dragged -> dragged () |> Circle.fill |> Some

                | OnPointerEnter enterMsg ->
                    Circle.onPointerEnter (
                        Event.pointerEnter Constants.CanvasId
                        >> Option.map (fun e -> enterMsg (flower.Id, e))
                        >> Option.defaultValue (),
                        SubPatchOptions.OnChangeOf flower.Id
                    )
                    |> Some

                | OnPointerLeave leaveMsg ->
                    Circle.onPointerLeave (
                        Event.pointerLeave Constants.CanvasId
                        >> Option.map (fun e -> leaveMsg (flower.Id, e))
                        >> Option.defaultValue (),
                        SubPatchOptions.OnChangeOf flower.Id
                    )
                    |> Some

                | OnPointerMoved movedMsg ->
                    Circle.onPointerMoved (
                        Event.pointerMoved Constants.CanvasId
                        >> Option.map (fun e -> movedMsg (flower.Id, e))
                        >> Option.defaultValue (),
                        SubPatchOptions.OnChangeOf flower.Id
                    )
                    |> Some

                | OnPointerPressed pressedMsg ->
                    Circle.onPointerPressed (
                        Event.pointerPressed Constants.CanvasId
                        >> Option.map (fun e -> pressedMsg (flower.Id, e))
                        >> Option.defaultValue (),
                        SubPatchOptions.OnChangeOf flower.Id
                    )
                    |> Some

                | OnPointerReleased releasedMsg ->
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
        circle
        (circleAttributes
         @ [ Circle.strokeThickness Theme.drawing.strokeWidth
             Circle.fill (string fadedColor) ])

let innerCircle (flower: Flower) (circle: Circle2D<Pixels, UserSpace>) (attributes: Attribute list) =
    let color =
        Theme.palette.primary |> Color.hex

    let innerRadius =
        circle.Radius
        * ClampedPercentage.inDecimal flower.OpenPercent

    let hovered () = Theme.lighter color |> string
    let pressed () = Theme.lightest color |> string
    let dragged () = Theme.fade color |> string

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
                        Event.pointerEnter Constants.CanvasId
                        >> Option.map (fun e -> enterMsg (flower.Id, e))
                        >> Option.defaultValue (),
                        SubPatchOptions.OnChangeOf flower.Id
                    )
                    |> Some

                | OnPointerLeave leaveMsg ->
                    Ellipse.onPointerLeave (
                        Event.pointerLeave Constants.CanvasId
                        >> Option.map (fun e -> leaveMsg (flower.Id, e))
                        >> Option.defaultValue (),
                        SubPatchOptions.OnChangeOf flower.Id
                    )
                    |> Some

                | OnPointerMoved movedMsg ->
                    Ellipse.onPointerMoved (
                        Event.pointerMoved Constants.CanvasId
                        >> Option.map (fun e -> movedMsg (flower.Id, e))
                        >> Option.defaultValue (),
                        SubPatchOptions.OnChangeOf flower.Id
                    )
                    |> Some

                | OnPointerPressed pressedMsg ->
                    Ellipse.onPointerPressed (
                        Event.pointerPressed Constants.CanvasId
                        >> Option.map (fun e -> pressedMsg (flower.Id, e))
                        >> Option.defaultValue (),
                        SubPatchOptions.OnChangeOf flower.Id
                    )
                    |> Some

                | OnPointerReleased releasedMsg ->
                    Ellipse.onPointerReleased (
                        Event.pointerReleased Constants.CanvasId
                        >> Option.map (fun e -> releasedMsg (flower.Id, e))
                        >> Option.defaultValue (),
                        SubPatchOptions.OnChangeOf flower.Id
                    )
                    |> Some)
            attributes
        |> List.filterNone

    Circle.from
        (Circle2D.withRadius innerRadius circle.Center)
        (circleAttributes
         @ [ Ellipse.strokeThickness Theme.drawing.strokeWidth
             Ellipse.fill (string color) ])

let selection (circle: Circle2D<Pixels, UserSpace>) (attributes: Attribute list) =
    if
        List.exists
            (fun e ->
                match e with
                | Selected -> true
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

let nameTag (flower: Flower) =
    TextBlock.create [
        TextBlock.text flower.Name
        TextBlock.left (flower.Position.X.value () - 20.)
        TextBlock.top (flower.Position.Y.value () - 35.)
    ]


// ---- Drawing ----------------------------------------------------------------

let draw (flower: Flower) (attributes: Attribute list) =
    let circle =
        Circle2D.atPoint flower.Position flower.Radius

    Canvas.create [
        Canvas.children [
            outerCircle flower circle attributes
            innerCircle flower circle attributes

            yield! selection circle attributes |> Option.toList

            nameTag flower
        ]
    ]
