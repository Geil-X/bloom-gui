namespace FlowerGui

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
    let private pointerEvent (id: string) (e: PointerEventArgs): MouseEvent<'Unit, 'Coordinates> =
        e.Handled <- true
        { Position = point id e
          BaseEvent = e }
        
    let pointerEnter = pointerEvent
    let pointerLeave = pointerEvent
    let pointerMoved = pointerEvent
        

    /// Position is given relative to the screen element given by the id string.
    let pointerPressed (id: string) (e: PointerPressedEventArgs) : MouseButtonEvent<'Unit, 'Coordinates> =
        e.Handled <- true
        { MouseButton = e.MouseButton
          Position = point id e
          BaseEvent = e }

    /// Position is given relative to the screen element given by the id string.
    let pointerReleased (id: string) (e: PointerReleasedEventArgs) : MouseButtonEvent<'Unit, 'Coordinates> =
        e.Handled <- true
        { MouseButton = e.InitialPressMouseButton
          Position = point id e
          BaseEvent = e
          }
