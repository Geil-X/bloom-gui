module Gui.Views.FlowerManager

open Avalonia.Input
open Avalonia.FuncUI.Types
open Avalonia.FuncUI.DSL
open Math.Geometry
open Math.Units

open Extensions
open Gui
open Gui.DataTypes

// ---- View Functions ----

let private drawFlower (state: State) (dispatch: Msg -> unit) (flower: Flower) : IView =
    let flowerState (flower: Flower) : Flower.Attribute option =
        match state.FlowerInteraction with
        | FlowerManager.Hovering id when id = flower.Id -> Flower.hovered |> Some
        | FlowerManager.Pressing pressing when pressing.Id = flower.Id -> Flower.pressed |> Some
        | FlowerManager.Dragging dragging when dragging.Id = flower.Id -> Flower.dragged |> Some
        | _ -> None

    Flower.view
        flower
        [ if Option.contains flower.Id state.Selected then
              Flower.selected
          yield! flowerState flower |> Option.toList

          Flower.onPointerEnter (FlowerPointerEvent.OnEnter >> dispatch)
          Flower.onPointerLeave (FlowerPointerEvent.OnLeave >> dispatch)
          Flower.onPointerMoved (FlowerPointerEvent.OnMoved >> dispatch)
          Flower.onPointerPressed (FlowerPointerEvent.OnPressed >> dispatch)
          Flower.onPointerReleased (FlowerPointerEvent.OnReleased >> dispatch) ]
    :> IView

let private simulationSpace state (dispatch: Msg -> unit) : IView =
    let flowers =
        state.Flowers
        |> Map.values
        |> Seq.map (drawFlower state dispatch)
        |> Seq.toList

    Canvas.create [
        Canvas.children flowers
        Canvas.background Theme.palette.canvasBackground
        Canvas.name Constants.CanvasId
        Canvas.onPointerReleased (
            Event.pointerReleased Constants.CanvasId
            >> Option.map (
                BackgroundEvent.OnReleased
                >> FlowerManagerMsg.BackgroundEvent
                >> dispatch
            )
            >> Option.defaultValue ()
        )
    ]
    :> IView


let view (state: State) (dispatch: Msg -> unit) =
    let selectedFlowerOption =
        Option.bind (fun id -> getFlower id state) state.Selected

    let flowers = Map.values state.Flowers

    DockPanel.create [
        DockPanel.children [
            DockPanel.child Dock.Top (Menu.applicationMenu state.AppConfig (MenuMsg >> dispatch))

            DockPanel.child Dock.Top (IconDock.view (IconDockMsg >> dispatch))

            DockPanel.child
                Dock.Left
                (FlowerProperties.view flowers selectedFlowerOption (FlowerPropertiesMsg >> dispatch))

            DockPanel.child
                Dock.Right
                (FlowerCommands.view
                    state.FlowerCommandsState
                    selectedFlowerOption
                    state.SerialPort
                    (FlowerCommandsMsg >> dispatch))


            simulationSpace state (Msg.FlowerManagerMsg >> dispatch)
        ]
    ]
