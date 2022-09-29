module Tests.FlowerManager

open Avalonia.Input
open Gui.DataTypes
open NUnit.Framework
open Math.Geometry

open Extensions
open Gui.DataTypes.FlowerManager

[<SetUp>]
let Setup () = ()

type BasicActionTest =
    { Name: string
      InitialState: State
      Messages: Msg list
      Expected: State }

let ``Basic actions test cases`` =
    let flower = Flower.empty

    let initialState =
        init () |> addFlower flower

    [ { Name = "Hovering on mouse over"
        InitialState = initialState
        Messages = [ Msg.OnFlowerEnter(flower.Id, MouseEvent.empty ()) ]
        Expected = { initialState with FlowerInteraction = Hovering flower.Id } }


      { Name = "Pressed on mouse down"
        InitialState = initialState
        Messages =
          [ Msg.OnFlowerEnter(flower.Id, MouseEvent.empty ())
            Msg.OnFlowerPressed(flower.Id, MouseButtonEvent.withButton MouseButton.Left) ]
        Expected =
          { initialState with
              FlowerInteraction =
                  Pressing
                      { Id = flower.Id
                        MousePressedLocation = Point2D.origin
                        InitialFlowerPosition = Point2D.origin } } }

      { Name = "Selected on mouse press and release at same location"
        InitialState = initialState
        Messages =
          [ Msg.OnFlowerEnter(flower.Id, MouseEvent.empty ())
            Msg.OnFlowerPressed(flower.Id, MouseButtonEvent.withButton MouseButton.Left)
            Msg.OnFlowerReleased(flower.Id, MouseButtonEvent.withButton MouseButton.Left) ]
        Expected =
          { initialState with
              FlowerInteraction = Hovering flower.Id
              Selected = Some flower.Id } }

      { Name = "Selected on mouse press and release near the same location"
        InitialState = initialState
        Messages =
          [ Msg.OnFlowerEnter(flower.Id, MouseEvent.empty ())
            Msg.OnFlowerPressed(flower.Id, MouseButtonEvent.withButton MouseButton.Left)
            Msg.OnFlowerMoved(flower.Id, MouseEvent.atPosition (Point2D.pixels 5. 5.))
            Msg.OnFlowerReleased(
                flower.Id,
                MouseButtonEvent.atPositionWithButton (Point2D.pixels 5. 5.) MouseButton.Left
            ) ]
        Expected =
          { initialState with
              FlowerInteraction = Hovering flower.Id
              Selected = Some flower.Id } }

      { Name = "Select & Deselect on background released"
        InitialState = initialState
        Messages =
          [ Msg.OnFlowerEnter(flower.Id, MouseEvent.empty ())
            Msg.OnFlowerPressed(flower.Id, MouseButtonEvent.withButton MouseButton.Left)
            Msg.OnBackgroundReleased(MouseButtonEvent.withButton MouseButton.Left) ]
        Expected = { initialState with Selected = None } }

      { Name = "Dragging on mouse down and move"
        InitialState = initialState
        Messages =
          [ Msg.OnFlowerEnter(flower.Id, MouseEvent.empty ())
            Msg.OnFlowerPressed(
                flower.Id,
                MouseButtonEvent.atPositionWithButton (Point2D.pixels 5. 5.) MouseButton.Left
            )
            Msg.OnFlowerMoved(flower.Id, MouseEvent.atPosition (Point2D.pixels 20. 20.)) ]
        Expected =
          { initialState with
              FlowerInteraction =
                  Dragging
                      { Id = flower.Id
                        DraggingDelta = Vector2D.pixels -5. -5. }
              Flowers = Map.update flower.Id (Flower.setPosition (Point2D.pixels 15. 15.)) initialState.Flowers } }

      // Turn test case list into test case data
      ]
    |> List.map (fun testCase ->
        TestCaseData(testCase.InitialState, testCase.Messages)
            .SetName(testCase.Name)
            .Returns(testCase.Expected))

[<TestCaseSource(nameof ``Basic actions test cases``)>]
let ``Basic flower actions`` (initialState: State) (messages: Msg list) : State =
    let updateWithoutCmd state msg = updateMsg msg state

    List.fold updateWithoutCmd initialState messages
