module Gui.Views.FlowerManager

open Avalonia.Input
open Avalonia.FuncUI.Types
open Avalonia.FuncUI.DSL
open Avalonia.Controls
open Math.Units

open Gui.DataTypes
open Extensions

// ---- View Functions ----

let private drawFlower (state: FlowerManager.State) (dispatch: FlowerManager.Msg -> unit) (flower: Flower) : IView =
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

          Flower.onPointerEnter (FlowerManager.OnFlowerEnter >> dispatch)
          Flower.onPointerLeave (FlowerManager.OnFlowerLeave >> dispatch)
          Flower.onPointerMoved (FlowerManager.OnFlowerMoved >> dispatch)
          Flower.onPointerPressed (FlowerManager.OnFlowerPressed >> dispatch)
          Flower.onPointerReleased (FlowerManager.OnFlowerReleased >> dispatch) ]
    :> IView

let view (state: FlowerManager.State) (dispatch: FlowerManager.Msg -> unit) : IView =
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
            >> Option.map (FlowerManager.OnBackgroundReleased >> dispatch)
            >> Option.defaultValue ()
        )
    ]
    :> IView
