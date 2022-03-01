namespace Gui

open Avalonia.Input
open Avalonia.Interactivity
open Geometry

open Extensions


type MouseEvent<'Unit, 'Coordinates> =
    { Position: Point2D<'Unit, 'Coordinates>
      BaseEvent: RoutedEventArgs }

type MouseButtonEvent<'Unit, 'Coordinates> =
    { MouseButton: MouseButton
      Position: Point2D<'Unit, 'Coordinates>
      BaseEvent: RoutedEventArgs }

module MouseEvent =
    let empty () : MouseEvent<'Unit, 'Coordinates> =
        { Position = Point2D.origin ()
          BaseEvent = null }

    let atPosition pos : MouseEvent<'Unit, 'Coordinates> = { Position = pos; BaseEvent = null }

module MouseButtonEvent =
    let empty () : MouseButtonEvent<'Unit, 'Coordinates> =
        { MouseButton = MouseButton.None
          Position = Point2D.origin ()
          BaseEvent = null }

    let atPosition pos : MouseButtonEvent<'Unit, 'Coordinates> =
        { MouseButton = MouseButton.None
          Position = pos
          BaseEvent = null }

    let withButton button : MouseButtonEvent<'Unit, 'Coordinates> =
        { MouseButton = button
          Position = Point2D.origin ()
          BaseEvent = null }


    let atPositionWithButton pos button : MouseButtonEvent<'Unit, 'Coordinates> =
        { MouseButton = button
          Position = pos
          BaseEvent = null }


module Events =
    open Avalonia
    open Avalonia.Controls

    let private point id (e: PointerEventArgs) : Point2D<'Unit, 'Coordinates> =
        let maybeVisual =
            View.findControl id (e.Source :?> IControl)

        let point =
            match maybeVisual with
            | Some visual -> e.GetPosition(visual)
            | None -> Point(infinity, infinity)

        Point2D.xy (Length<'Unit>.create point.X) (Length<'Unit>.create point.Y)

    /// Convert an Avalonia pointer event into a one that is using geometric points.
    /// Position is given relative to the screen element given by the id string.
    let private pointerEvent (parentId: string) (e: PointerEventArgs) : MouseEvent<'Unit, 'Coordinates> option =
        if e.Route = RoutingStrategies.Tunnel then
            None
        else
            e.Handled <- true

            { Position = point parentId e
              BaseEvent = e }
            |> Some

    let pointerEnter = pointerEvent
    let pointerLeave = pointerEvent
    let pointerMoved = pointerEvent


    /// Position is given relative to the screen element given by the id string.
    let pointerPressed (parentId: string) (e: PointerPressedEventArgs) : MouseButtonEvent<'Unit, 'Coordinates> option =
        if e.Route = RoutingStrategies.Tunnel then
            None
        else
            e.Handled <- true

            { MouseButton = e.MouseButton
              Position = point parentId e
              BaseEvent = e }
            |> Some

    /// Position is given relative to the screen element given by the id string.
    let pointerReleased
        (parentId: string)
        (e: PointerReleasedEventArgs)
        : MouseButtonEvent<'Unit, 'Coordinates> option =
        if e.Route = RoutingStrategies.Tunnel then
            None
        else
            e.Handled <- true

            { MouseButton = e.InitialPressMouseButton
              Position = point parentId e
              BaseEvent = e }
            |> Some
