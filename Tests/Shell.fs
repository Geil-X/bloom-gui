module Tests

open Avalonia.Input
open NUnit.Framework
open Geometry

open Gui
open Extensions

[<SetUp>]
let Setup () = ()

let ``Basic actions test cases`` =
    let flower = Flower.basic "Flower"

    let initialState =
        Shell.init ()
        |> Tuple2.first
        |> Shell.addFlower flower

    [ "Hovering on mouse over",
      initialState,
      [ Shell.OnEnter(flower.Id, MouseEvent.empty ()) ],
      { initialState with
            FlowerInteraction = Shell.Hovering flower.Id }

      "Pressed on mouse down",
      initialState,
      [ Shell.OnEnter(flower.Id, MouseEvent.empty ())
        Shell.OnPressed(flower.Id, MouseButtonEvent.withButton MouseButton.Left) ],
      { initialState with
            FlowerInteraction =
                Shell.Pressing
                    { Id = flower.Id
                      MousePressedLocation = Point2D.origin ()
                      InitialFlowerPosition = Point2D.origin () } }

      "Selected on mouse press and release at same location",
      initialState,
      [ Shell.OnEnter(flower.Id, MouseEvent.empty ())
        Shell.OnPressed(flower.Id, MouseButtonEvent.withButton MouseButton.Left)
        Shell.OnReleased(flower.Id, MouseButtonEvent.withButton MouseButton.Left) ],
      { initialState with
            FlowerInteraction = Shell.Hovering flower.Id
            Selected = Some flower.Id }

      "Selected on mouse press and release near the same location",
      initialState,
      [ Shell.OnEnter(flower.Id, MouseEvent.empty ())
        Shell.OnPressed(flower.Id, MouseButtonEvent.withButton MouseButton.Left)
        Shell.OnMoved(flower.Id, MouseEvent.atPosition (Point2D.pixels 5. 5.))
        Shell.OnReleased(flower.Id, MouseButtonEvent.atPositionWithButton (Point2D.pixels 5. 5.) MouseButton.Left) ],
      { initialState with
            FlowerInteraction = Shell.Hovering flower.Id
            Selected = Some flower.Id }

      "Dragging on mouse down and move",
      initialState,
      [ Shell.OnEnter(flower.Id, MouseEvent.empty ())
        Shell.OnPressed(flower.Id, MouseButtonEvent.atPositionWithButton (Point2D.pixels 5. 5.) MouseButton.Left)
        Shell.OnMoved(flower.Id, MouseEvent.atPosition (Point2D.pixels 20. 20.)) ],
      { initialState with
            FlowerInteraction =
                Shell.Dragging
                    { Id = flower.Id
                      DraggingDelta = Vector2D.pixels -5. -5. }
            Flowers = Map.update flower.Id (Flower.setPosition (Point2D.pixels 15. 15.)) initialState.Flowers }

      // Turn test case list into test case data
      ]
    |> List.map
        (fun (name, initialState, messages, expected) ->
            TestCaseData(initialState, messages)
                .SetName(name)
                .Returns(expected))

[<TestCaseSource(nameof ``Basic actions test cases``)>]
let ``Basic flower actions`` (initialState: Shell.State) (messages: Shell.FlowerEvent list) : Shell.State =
    let updateWithoutCmd state msg =
        Shell.update (Shell.FlowerEvent msg) state
        |> Tuple2.first

    List.fold updateWithoutCmd initialState messages
