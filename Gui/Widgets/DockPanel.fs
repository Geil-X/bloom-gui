module Gui.Widgets.DockPanel

open Avalonia.Controls
open Avalonia.FuncUI.DSL
open Avalonia.FuncUI.Types
open Gui


let child dir (child: IView<'a>) =
    let border =
        match dir with
        | Dock.Left -> Border.borderThickness (0., 0., Theme.border.thickness, 0.)
        | Dock.Top -> Border.borderThickness (0., 0., 0., Theme.border.thickness)
        | Dock.Right -> Border.borderThickness (Theme.border.thickness, 0., 0., 0.)
        | Dock.Bottom -> Border.borderThickness (0., Theme.border.thickness, 0., 0.)
        | _ ->
            Border.borderThickness (
                Theme.border.thickness,
                Theme.border.thickness,
                Theme.border.thickness,
                Theme.border.thickness
            )

    Border.create [
        Control.dock dir
        Border.borderBrush Theme.palette.panelAccent
        border
        Border.child child
    ]
