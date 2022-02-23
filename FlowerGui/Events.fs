namespace FlowerGui

open Avalonia.Input
open Avalonia.Interactivity
open Geometry

open Extensions

type MouseEvent<'Unit, 'Coordinates> =
    { MouseButton: MouseButton
      Position: Point2D<'Unit, 'Coordinates>
      BaseEvent: RoutedEventArgs }
    
module Events =
    open Avalonia
    open Avalonia.Controls

    let handle (e: MouseEvent<'Unit, 'Coordinates>) : unit = e.BaseEvent.Handled <- true

    let private point id (e: PointerEventArgs): Point2D<'Unit, 'Coordinates> =
        let maybeVisual =
            View.findControl id (e.Source :?> IControl)

        e.Handled <- true

        let point =
            match maybeVisual with
            | Some visual -> e.GetPosition(visual)
            | None -> Point(infinity, infinity)
            
        Point2D.xy (Length<'Unit>.create point.X) (Length<'Unit>.create point.Y)

    let pressedEvent (id: string) (e: PointerPressedEventArgs) : MouseEvent<'Unit, 'Coordinates> =
        { MouseButton = e.MouseButton
          Position = point id e
          BaseEvent = e }

    let releasedEvent (id: string) (e: PointerReleasedEventArgs) : MouseEvent<'Unit, 'Coordinates> =
        { MouseButton = e.InitialPressMouseButton
          Position = point id e
          BaseEvent = e }
