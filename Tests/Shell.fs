module Tests.Shell

open Avalonia.Input
open Gui.DataTypes
open NUnit.Framework
open Geometry

open Extensions
open Gui
open Gui.Shell

[<SetUp>]
let Setup () = ()

type BasicActionTest =
    { Name: string
      InitialState: State
      Messages: SimulationEvent list
      Expected: State }

let ``Basic actions test cases`` =
    let flower = Flower.basic "Flower"

    let initialState =
        init () |> Tuple2.first |> addFlower flower

    [ { Name = "Hovering on mouse over"
        InitialState = initialState
        Messages =
            [ FlowerPointerEvent.OnEnter(flower.Id, MouseEvent.empty ()) ]
            |> List.map SimulationEvent.FlowerEvent
        Expected =
            { initialState with
                  FlowerInteraction = Hovering flower.Id } }


      { Name = "Pressed on mouse down"
        InitialState = initialState
        Messages =
            [ FlowerPointerEvent.OnEnter(flower.Id, MouseEvent.empty ())
              FlowerPointerEvent.OnPressed(flower.Id, MouseButtonEvent.withButton MouseButton.Left) ]
            |> List.map SimulationEvent.FlowerEvent
        Expected =
            { initialState with
                  FlowerInteraction =
                      Pressing
                          { Id = flower.Id
                            MousePressedLocation = Point2D.origin ()
                            InitialFlowerPosition = Point2D.origin () } } }

      { Name = "Selected on mouse press and release at same location"
        InitialState = initialState
        Messages =
            [ FlowerPointerEvent.OnEnter(flower.Id, MouseEvent.empty ())
              FlowerPointerEvent.OnPressed(flower.Id, MouseButtonEvent.withButton MouseButton.Left)
              FlowerPointerEvent.OnReleased(flower.Id, MouseButtonEvent.withButton MouseButton.Left) ]
            |> List.map SimulationEvent.FlowerEvent
        Expected =
            { initialState with
                  FlowerInteraction = Hovering flower.Id
                  Selected = Some flower.Id } }

      { Name = "Selected on mouse press and release near the same location"
        InitialState = initialState
        Messages =
            [ FlowerPointerEvent.OnEnter(flower.Id, MouseEvent.empty ())
              FlowerPointerEvent.OnPressed(flower.Id, MouseButtonEvent.withButton MouseButton.Left)
              FlowerPointerEvent.OnMoved(flower.Id, MouseEvent.atPosition (Point2D.pixels 5. 5.))
              FlowerPointerEvent.OnReleased(
                  flower.Id,
                  MouseButtonEvent.atPositionWithButton (Point2D.pixels 5. 5.) MouseButton.Left
              ) ]
            |> List.map SimulationEvent.FlowerEvent
        Expected =
            { initialState with
                  FlowerInteraction = Hovering flower.Id
                  Selected = Some flower.Id } }

      { Name = "Select & Deselect on background released"
        InitialState = initialState
        Messages =
            [ FlowerPointerEvent.OnEnter(flower.Id, MouseEvent.empty ())
              |> SimulationEvent.FlowerEvent
              FlowerPointerEvent.OnPressed(flower.Id, MouseButtonEvent.withButton MouseButton.Left)
              |> SimulationEvent.FlowerEvent
              BackgroundEvent.OnReleased(MouseButtonEvent.withButton MouseButton.Left)
              |> SimulationEvent.BackgroundEvent ]
        Expected = { initialState with Selected = None } }

      { Name = "Dragging on mouse down and move"
        InitialState = initialState
        Messages =
            [ FlowerPointerEvent.OnEnter(flower.Id, MouseEvent.empty ())
              FlowerPointerEvent.OnPressed(
                  flower.Id,
                  MouseButtonEvent.atPositionWithButton (Point2D.pixels 5. 5.) MouseButton.Left
              )
              FlowerPointerEvent.OnMoved(flower.Id, MouseEvent.atPosition (Point2D.pixels 20. 20.)) ]
            |> List.map SimulationEvent.FlowerEvent
        Expected =
            { initialState with
                  FlowerInteraction =
                      Dragging
                          { Id = flower.Id
                            DraggingDelta = Vector2D.pixels -5. -5. }
                  Flowers = Map.update flower.Id (Flower.setPosition (Point2D.pixels 15. 15.)) initialState.Flowers } }

      // Turn test case list into test case data
      ]
    |> List.map
        (fun testCase ->
            TestCaseData(testCase.InitialState, testCase.Messages)
                .SetName(testCase.Name)
                .Returns(testCase.Expected))

[<TestCaseSource(nameof ``Basic actions test cases``)>]
let ``Basic flower actions`` (initialState: State) (messages: Shell.SimulationEvent list) : State =
    let updateWithoutCmd state msg =
        update (SimulationEvent msg) state Mock.Window |> Tuple2.first

    List.fold updateWithoutCmd initialState messages
