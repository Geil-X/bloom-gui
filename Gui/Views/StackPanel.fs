module Gui.Views.StackPanel

open Avalonia.Controls
open Avalonia.FuncUI.DSL
open Avalonia.FuncUI.Types

open Gui

let verticalSeparator (child: IView<'a>) =
    Border.create [
        Border.borderThickness (0., Theme.border.thickness, 0., 0.)
        Border.borderBrush Theme.palette.panelAccent
        Border.child child
    ]