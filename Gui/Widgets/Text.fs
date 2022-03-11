module Gui.Widgets.Text

open Avalonia.Controls
open Avalonia.FuncUI.DSL
open Avalonia.Layout
open Gui

let iconTitle icon name =
    let titleBlock =
        TextBlock.create [
            TextBlock.fontSize Theme.font.h1
            TextBlock.verticalAlignment VerticalAlignment.Center
            TextBlock.text name
            TextBlock.margin (Theme.spacing.small, 0., 0., 0.)
        ]

    StackPanel.create [
        StackPanel.verticalAlignment VerticalAlignment.Center
        StackPanel.horizontalAlignment HorizontalAlignment.Center
        StackPanel.orientation Orientation.Horizontal
        StackPanel.children [ icon; titleBlock ]
        StackPanel.margin (0., Theme.spacing.medium)
    ]
