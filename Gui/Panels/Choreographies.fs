module Gui.Panels.Choreographies

open Avalonia.Controls
open Avalonia.FuncUI.DSL
open Avalonia.FuncUI.Types

open Gui
open Gui.Widgets

type Msg = | SelectChoreography

let view (dispatch: Msg -> unit) =
    let children : IView list =
        [ Text.iconTitle (Icon.choreography Icon.large Theme.palette.primary) "Choreographies" Theme.palette.foreground ]

    StackPanel.create [
        StackPanel.children children
        StackPanel.minWidth 200.
    ]
