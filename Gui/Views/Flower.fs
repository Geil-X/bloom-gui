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
open Gui.DataTypes.Flower
open Extensions

let outerCircle (flower: Flower) (circle: Circle2D<Meters, ScreenSpace>) (attributes: Attribute list) : IView<Circle> =
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

let selection (circle: Circle2D<Meters, ScreenSpace>) (attributes: Attribute list) =
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
        TextBlock.left (Length.inCssPixels flower.Position.X - 20.)
        TextBlock.top (Length.inCssPixels flower.Position.Y - 35.)
    ]

// ---- Drawing ----------------------------------------------------------------

let draw (flower: Flower) (attributes: Attribute list) =
    let circle =
        Circle2D.atPoint flower.Position flower.Radius

    let selectionView =
        selection circle attributes
        |> Option.map (fun a -> a :> IView)
        |> Option.toList


    Canvas.create [
        Canvas.children [
            outerCircle flower circle attributes
            yield! selectionView
            nameTag flower
        ]
    ]
