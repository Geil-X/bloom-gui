module Gui.Panels.Choreographies

open Avalonia.Controls
open Avalonia.FuncUI.DSL
open Avalonia.FuncUI.Types

open Gui
open Gui.Views
open Gui.Views.Components
open Utilities.Extensions

type Msg = Action of Action

let title =
    Text.iconTitle (Icon.choreography Icon.large Theme.palette.primary) "Choreographies" Theme.palette.foreground

let choreographies dispatch =
    ListBox.create [
        ListBox.dataItems DiscriminatedUnion.allCases<Choreography>
        ListBox.onSelectedItemChanged
            (fun item ->
                if not (isNull item) then
                    Action.SelectChoreography(item :?> Choreography)
                    |> Action
                    |> dispatch)
    ]


let view (dispatch: Msg -> unit) =
    let children : IView list =
        [ title
          choreographies dispatch
          RadialSlider.create [] ]

    StackPanel.create [
        StackPanel.children children
        StackPanel.minWidth 200.
    ]
