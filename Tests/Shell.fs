module Tests

open NUnit.Framework

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
      [ Shell.OnEnter flower.Id |> Shell.FlowerEvent ],
      { initialState with
            FlowerInteraction = Shell.Hovering flower.Id }

      "Pressed on mouse down",
      initialState,
      [ Shell.OnEnter flower.Id |> Shell.FlowerEvent
        Shell.OnPressed flower.Id |> Shell.FlowerEvent ],
      { initialState with
            FlowerInteraction = Shell.Pressing flower.Id }

      "Selected on mouse press and release at same location",
      initialState,
      [ Shell.OnEnter flower.Id |> Shell.FlowerEvent
        Shell.OnPressed flower.Id |> Shell.FlowerEvent
        Shell.OnReleased flower.Id |> Shell.FlowerEvent ],
      { initialState with
            FlowerInteraction = Shell.Hovering flower.Id
            Selected = Some flower.Id }

      "Dragging on mouse down and move",
      initialState,
      [ Shell.OnEnter flower.Id |> Shell.FlowerEvent
        Shell.OnPressed flower.Id |> Shell.FlowerEvent
        Shell.OnMoved flower.Id |> Shell.FlowerEvent ],
      { initialState with
            FlowerInteraction = Shell.Dragging flower.Id }
      
      "Dragging on mouse down and move",
      initialState,
      [ Shell.OnEnter flower.Id |> Shell.FlowerEvent
        Shell.OnPressed flower.Id |> Shell.FlowerEvent
        Shell.OnMoved flower.Id |> Shell.FlowerEvent ],
      { initialState with
            FlowerInteraction = Shell.Dragging flower.Id }
      
      // Turn test case list into test case data
      ]
    |> List.map
        (fun (name, initialState, messages, expected) ->
            TestCaseData(initialState, messages)
                .SetName(name)
                .Returns(expected))

[<TestCaseSource(nameof ``Basic actions test cases``)>]
let ``Basic flower actions`` (initialState: Shell.State) (messages: Shell.Msg list) : Shell.State =
    let updateWithoutCmd state msg = Shell.update msg state |> Tuple2.first
    List.fold updateWithoutCmd initialState messages
