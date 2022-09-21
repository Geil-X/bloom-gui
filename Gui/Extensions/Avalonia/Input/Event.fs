namespace Avalonia.Input

open Avalonia.Interactivity
open Math.Geometry
open Math.Units

type MouseEvent<'Coordinates> =
    { Position: Point2D<Meters, 'Coordinates>
      BaseEvent: RoutedEventArgs }

type MouseButtonEvent<'Coordinates> =
    { MouseButton: MouseButton
      Position: Point2D<Meters, 'Coordinates>
      BaseEvent: RoutedEventArgs }

module MouseEvent =
    let empty () : MouseEvent<'Coordinates> =
        { Position = Point2D.origin
          BaseEvent = null }

    let atPosition pos : MouseEvent<'Coordinates> = { Position = pos; BaseEvent = null }

module MouseButtonEvent =
    let empty () : MouseButtonEvent<'Coordinates> =
        { MouseButton = MouseButton.None
          Position = Point2D.origin
          BaseEvent = null }

    let atPosition pos : MouseButtonEvent<'Coordinates> =
        { MouseButton = MouseButton.None
          Position = pos
          BaseEvent = null }

    let withButton button : MouseButtonEvent<'Coordinates> =
        { MouseButton = button
          Position = Point2D.origin
          BaseEvent = null }


    let atPositionWithButton pos button : MouseButtonEvent<'Coordinates> =
        { MouseButton = button
          Position = pos
          BaseEvent = null }


module Event =
    open Avalonia
    open Avalonia.Controls
    open Avalonia.FuncUI.DSL
    open System
    open Avalonia.Input

    let handleEvent msg (event: RoutedEventArgs) =
        event.Handled <- true
        msg

    let handleKeyPress msg (event: KeyEventArgs) =
        event.Handled <- true
        msg event.Key

    /// Given a pointer event, get the position relative to a particular control name
    /// Useful for triggering off of mouse movement events
    let positionRelativeTo (name: String) (event: PointerEventArgs) =
        event.Handled <- true

        let maybeVisual =
            View.findControl name (event.Source :?> IControl)

        match maybeVisual with
        | Some visual -> event.GetPosition(visual)
        | None -> Point(infinity, infinity)


    let private point id (e: PointerEventArgs) : Point2D<Meters, 'Coordinates> =
        let maybeVisual =
            View.findControl id (e.Source :?> IControl)

        let point =
            match maybeVisual with
            | Some visual -> e.GetPosition(visual)
            | None -> Point(infinity, infinity)

        Point2D.xy (Length.cssPixels point.X) (Length.cssPixels point.Y)

    /// Convert an Avalonia pointer event into a one that is using geometric points.
    /// Position is given relative to the screen element given by the id string.
    let private pointerEvent (parentId: string) (e: PointerEventArgs) : MouseEvent<'Coordinates> option =
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
    let pointerPressed (parentId: string) (e: PointerPressedEventArgs) : MouseButtonEvent<'Coordinates> option =
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
        : MouseButtonEvent<'Coordinates> option =
        if e.Route = RoutingStrategies.Tunnel then
            None
        else
            e.Handled <- true

            { MouseButton = e.InitialPressMouseButton
              Position = point parentId e
              BaseEvent = e }
            |> Some
